using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Registry;
using USBDevices.Classes;

namespace USBDevices
{
	//http://msdn.microsoft.com/en-us/library/windows/hardware/ff541389(v=vs.85).aspx
	//http://hackingexposedcomputerforensicsblog.blogspot.com/2013/08/daily-blog-67-understanding-artifacts.html
	//http://www.forensicswiki.org/wiki/USB_History_Viewing


	public class UsbDevices
	{
		public enum WinVerEnum
		{
			WindowsXp,
			WindowsVista,
			Windows7,
			Windows8,
			Unsupported
		}

		public UsbDevices(string systemRegistryFilePath, string softwareRegistryFilePath, string setupApiDevLogPath,
			List<string> ntUserFilePaths)
		{
			WindowsVersion = GetWindowsVersion(softwareRegistryFilePath);
			TimeZone = GetTimeZoneFromRegistry(systemRegistryFilePath);

			var usbEnums = GetEnumUSBData(systemRegistryFilePath);
			var mountedDevices = GetMountedDevices(systemRegistryFilePath);
			var windowsPortableDevices = GetWindowsPortableDevices(softwareRegistryFilePath);
			var emdMgmtDevices = GetEmdMgmtList(softwareRegistryFilePath);

			var storageClasses = GetStorageClassesDevices(systemRegistryFilePath);

			var ntusers = new List<Tuple<string, List<MountPoint2Device>>>();

			foreach (var ntUserFilePath in ntUserFilePaths)
			{
				var profileName = GetProfileName(ntUserFilePath);
				var mountPoint2Devices = GetMountpoint2Devices(ntUserFilePath);

				foreach (var mountPoint2Device in mountPoint2Devices)
				{
					var mp2Device = mountPoint2Device;
					var mountedDev =
						mountedDevices.Volumes.SingleOrDefault(
							y => y.GUID.ToLowerInvariant() == mp2Device.Guid.ToLowerInvariant());

					mountedDev?.UsersWhoMountedDevice.Add(new Tuple<string, DateTimeOffset>(profileName,
						mp2Device.LastWriteTime));
				}

				ntusers.Add(new Tuple<string, List<MountPoint2Device>>(profileName, mountPoint2Devices));
			}


			USBDeviceList = GetUSBDeviceList(systemRegistryFilePath);

			var setupApiRecords = new SetUpApiDevLog(setupApiDevLogPath,TimeZone);

			foreach (var usbDevice in USBDeviceList)
			{
				var device = usbDevice;

				var setupApiDevRecord =
					setupApiRecords.SetUpApiRecords.SingleOrDefault(
						y => y.SerialNumber.ToLowerInvariant() == usbDevice.SerialNumber.ToLowerInvariant());

				if (setupApiDevRecord != null)
				{
					usbDevice.FirstDateTimeConnectedSetupApi = setupApiDevRecord.FirstConnectedDatetime;
				}


				var enumRecord =
					usbEnums.SingleOrDefault(
						y => y.SerialNumber.ToLowerInvariant() == usbDevice.SerialNumber.ToLowerInvariant());

				if (enumRecord != null)
				{
					usbDevice.VendorId = enumRecord.VID_ID;
					usbDevice.ProductId = enumRecord.PID_ID;
					usbDevice.VendorNameFromID = enumRecord.ProductInfo.VendorName;
					usbDevice.ProductNameFromID = enumRecord.ProductInfo.ProductDescription;
				}

				var portDel =
					windowsPortableDevices.SingleOrDefault(
						y => y.SerialNumber.ToLowerInvariant() == usbDevice.SerialNumber.ToLowerInvariant());

				if (portDel != null)
				{
					usbDevice.FriendlyName = portDel.FriendlyName;
				}

				var mountedDevsDOS =
					mountedDevices.DosDevices.Where(
						y => y.SerialNumber.ToLowerInvariant() == device.SerialNumber.ToLowerInvariant());

				if (mountedDevsDOS.Any())
				{
					usbDevice.DriveLetters = string.Join(", ", mountedDevsDOS.Select(y => y.DriveLetter));
				}


				var emdDevices =
					emdMgmtDevices.Where(
						y => y.DeviceSerialNumber.ToLowerInvariant() == device.SerialNumber.ToLowerInvariant());

				if (emdDevices.Any())
				{
					foreach (var emdMgmtDevice in emdDevices)
					{
						usbDevice.Volumes.Add(new Tuple<string, uint, string>(emdMgmtDevice.VolumeName,
							emdMgmtDevice.VolumeSerialNumber, emdMgmtDevice.VolumeSerialNumberHex));
					}
				}

				var storage =
					storageClasses.FirstOrDefault(
						y => y.SerialNumber.ToLowerInvariant() == usbDevice.SerialNumber.ToLowerInvariant());

				if (storage != null)
				{
					usbDevice.FirstDateTimeConnectedStorageClass = storage.LastWriteTime;
				}


				var volume =
					mountedDevices.Volumes.SingleOrDefault(
						y => y.SerialNumber.ToLowerInvariant() == device.SerialNumber.ToLowerInvariant());

				if (volume != null)
				{
					//TODO needs test
					device.UsersWhoMountedDevice = volume.UsersWhoMountedDevice;
				}
			}
		}

		public WinVerEnum WindowsVersion { get; private set; }
		public TimeZoneInfo TimeZone { get; }
		public List<USBDevice> USBDeviceList { get; }

		private string GetProfileName(string ntUserFilePath)
		{
			var ntuserHive = new RegistryHive(ntUserFilePath);

			const string keyname = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";

			var key = ntuserHive.FindKey(keyname);

			return key.Values.Single(t => t.ValueName == "Desktop").ValueData.Split('\\')[2];
		}

		private List<MountPoint2Device> GetMountpoint2Devices(string ntUserFilePath)
		{
			var mp2 = new List<MountPoint2Device>();

			var ntuserHive = new RegistryHive(ntUserFilePath);

			const string keyname = @"Software\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2";

			var key = ntuserHive.FindKey(keyname);

			foreach (var k in key.SubKeys)
			{
				var mp = new MountPoint2Device();
				mp.Guid = k.KeyName.Replace("{", "").Replace("}", "");
				mp.LastWriteTime = k.LastWriteTime.Value;

				mp2.Add(mp);
			}

			return mp2;
		}

		private List<StorageClass> GetStorageClassesDevices(string systemRegistryFilePath)
		{
			var systemRegistryHive = new RegistryHive(systemRegistryFilePath);

			//TODO determine real controlset
			const string keyname53fa = @"ControlSet001\Control\DeviceClasses\{53f56307-b6bf-11d0-94f2-00a0c91efb8b}";
			const string keyname53fb = @"ControlSet001\Control\DeviceClasses\{53f5630d-b6bf-11d0-94f2-00a0c91efb8b}";
			const string keynamea5d = @"ControlSet001\Control\DeviceClasses\{a5dcbf10-6530-11d2-901f-00c04fb951ed}";


			var scList = new List<StorageClass>();

			var key = systemRegistryHive.FindKey(keyname53fa);

			foreach (var k in key.SubKeys)
			{
				var segs = k.KeyName.Split('#');

				var sc = new StorageClass
				{
					SourceGUID = "53f56307-b6bf-11d0-94f2-00a0c91efb8b",
					DeviceType = segs[3],
					LastWriteTime = k.LastWriteTime.Value
				};


				var segs2 = segs[4].Split('&');

				sc.DeviceClass = segs2[0];

				sc.Vendor = segs2[1].Substring(4);
				sc.Product = segs2[2].Substring(5);


				var sn = segs[5];
				sc.SerialNumber = sn;
				if (sn[1] == '&')
				{
					sc.SerialNumber = sn.Split('&')[1];
				}

				if (sc.DeviceType == "USBSTOR")
				{
					sc.Revision = segs2[3].Substring(4);
				}

				if (sc.SerialNumber.EndsWith("&0"))
				{
					sc.SerialNumber = sc.SerialNumber.Replace("&0", "");
				}

				//TODO VOLUME GUID?

				scList.Add(sc);
			}

			key = systemRegistryHive.FindKey(keyname53fb);

			foreach (var k in key.SubKeys)
			{
				var segs = k.KeyName.Split('#');

				var sc = new StorageClass
				{
					SourceGUID = "53f5630d-b6bf-11d0-94f2-00a0c91efb8b",
					DeviceType = segs[3],
					LastWriteTime = k.LastWriteTime.Value
				};

				var segs2 = segs[6].Split('&');

				if (segs2.Length == 1)
				{
					continue;
				}

				sc.DeviceClass = segs2[0];

				sc.Vendor = segs2[1].Substring(4);
				sc.Product = segs2[2].Substring(5);


				var sn = segs[7];
				sc.SerialNumber = sn;
				if (sn[1] == '&')
				{
					sc.SerialNumber = sn.Split('&')[1];
				}

				if (sc.DeviceType == "USBSTOR")
				{
					sc.Revision = segs2[3].Substring(4);
				}

				if (sc.SerialNumber.EndsWith("&0"))
				{
					sc.SerialNumber = sc.SerialNumber.Replace("&0", "");
				}

				//TODO VOLUME GUID?

				scList.Add(sc);
			}

			key = systemRegistryHive.FindKey(keynamea5d);

			foreach (var k in key.SubKeys)
			{
				var segs = k.KeyName.Split('#');

				var sc = new StorageClass
				{
					SourceGUID = "a5dcbf10-6530-11d2-901f-00c04fb951ed",
					DeviceType = segs[3],
					LastWriteTime = k.LastWriteTime.Value
				};

				var sn = segs[5];
				sc.SerialNumber = sn;
				if (sn[1] == '&')
				{
					sc.SerialNumber = sn.Split('&')[1];
				}

				if (sc.DeviceType == "USBSTOR")
				{
					sc.Revision = segs[1].Substring(4);
				}

				if (sc.SerialNumber.EndsWith("&0"))
				{
					sc.SerialNumber = sc.SerialNumber.Replace("&0", "");
				}

				//TODO VOLUME GUID?

				scList.Add(sc);
			}

			return scList;
		}

		private List<USBDevice> GetUSBDeviceList(string systemRegistryFilePath)
		{
			var systemRegistryHive = new RegistryHive(systemRegistryFilePath);

			const string keyname = @"ControlSet001\Enum\USBSTOR";

			var devList = new List<USBDevice>();

			var key = systemRegistryHive.FindKey(keyname);

			foreach (var k in key.SubKeys)
			{
				var segs = k.KeyName.Split('&');

				var usbd = new USBDevice
				{
					Type = segs[0],
					Vendor = segs[1].Substring(4),
					Product = segs[2].Substring(5),
					Revision = segs[3].Substring(4),
					UsbStorKeyLastWriteTime = k.LastWriteTime.Value
				};


				if (usbd.Type == "CdRom")
				{
					continue;
				}

				var serialKey = k.SubKeys.FirstOrDefault();

				if (serialKey != null)
				{
					usbd.SerialNumber = serialKey.KeyName.Split('&')[0];

					//SYSTEM\ControlSet001\Enum\USBSTOR\Disk&Ven_Multiple&Prod_Card__Reader&Rev_1.00\058F63666438&0\Properties\{83da6326-97a6-4088-9453-a1923f573b29}\0064

					var props = serialKey.SubKeys.Single(t => t.KeyName == "Properties");

					foreach (var registryKey in props.SubKeys)
					{
						if (registryKey.KeyName == "{83da6326-97a6-4088-9453-a1923f573b29}")
						{
							foreach (var subKey in registryKey.SubKeys)
							{
								if (subKey.KeyName == "0064")
								{
									usbd.FirstDateTimeConnected0064 = subKey.LastWriteTime;
								}

								if (subKey.KeyName == "0066")
								{
									usbd.LastDateTimeConnected0066 = subKey.LastWriteTime;
								}

								if (subKey.KeyName == "0067")
								{
									usbd.LastDateTimeRemoved0067 = subKey.LastWriteTime;
								}
							}
						}
					}
				}

				devList.Add(usbd);
			}

			return devList;
		}

		

		private List<EmdMgmtDevice> GetEmdMgmtList(string softwareRegistryFilePath)
		{
			var emdDevices = new List<EmdMgmtDevice>();

			var registryHive = new RegistryHive(softwareRegistryFilePath);

			const string keyname = @"Microsoft\Windows NT\CurrentVersion\EMDMgmt";

			var key = registryHive.FindKey(keyname);

			foreach (var v in key.SubKeys)
			{
				//_??_USBSTOR#Disk&Ven_Flash&Prod_Drive_SM_USB20&Rev_1100#AA04012700013494&0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}PHOTOS BACK_1141839789

				//_??_USBSTOR#Disk
				//Ven_Flash
				//Prod_Drive_SM_USB20
				//Rev_1100#AA04012700013494
				//0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}PHOTOS BACK_1141839789

				var segs = v.KeyName.Split('&');

				if (!segs[0].Contains("USBSTOR#Disk"))
				{
					continue;
				}

				var md = new EmdMgmtDevice();
				var segs2 = segs[3].Split('#');

				md.DeviceSerialNumber = segs2[1];
				var nameVol = segs[4].Split('}')[1];
				var separatorPosition = nameVol.LastIndexOf('_');
				md.VolumeName = nameVol.Substring(0, separatorPosition);
				md.VolumeSerialNumber = uint.Parse(nameVol.Substring(separatorPosition + 1));
				md.VolumeSerialNumberHex = md.VolumeSerialNumber.ToString("X");

				emdDevices.Add(md);
			}

			return emdDevices;
		}

		private List<PortableMappedDevice> GetWindowsPortableDevices(string softwareRegistryFilePath)
		{
			var portableDevs = new List<PortableMappedDevice>();

			var registryHive = new RegistryHive(softwareRegistryFilePath);

			const string keyname = @"Microsoft\Windows Portable Devices\Devices";

			var key = registryHive.FindKey(keyname);

			foreach (var v in key.SubKeys)
			{
				//SWD#WPDBUSENUM#_??_USBSTOR#DISK
				//VEN_SANDISK
				//PROD_SANDISK_ULTRA
				//REV_PMAP#A200435	D2C03E709
				//0#{53F56307-B6BF-11D0-94F2-00A0C91EFB8B}

				var segs = v.KeyName.Split('&');

				if (!segs[0].Contains("USBSTOR#DISK"))
				{
					continue;
				}

				var pd = new PortableMappedDevice();
				pd.Vendor = segs[1].Substring(4);
				pd.Product = segs[2].Substring(5);
				var segs2 = segs[3].Split('#');
				pd.Revision = segs2[0].Substring(4);
				pd.SerialNumber = segs2[1];
				pd.GUID = segs[4].Split('{')[1].Replace("}", "");

				pd.FriendlyName = v.Values.Single(t => t.ValueName == "FriendlyName").ValueData;

				portableDevs.Add(pd);
			}

			return portableDevs;
		}

		private MountedDevice GetMountedDevices(string systemRegistryFilePath)
		{
			var mountedList = new MountedDevice();

			var registryHive = new RegistryHive(systemRegistryFilePath);

			const string keyname = @"MountedDevices";

			var key = registryHive.FindKey(keyname);

			foreach (var v in key.Values)
			{
				// //"USBSTOR#Disk&Ven_SMI&Prod_USB_DISK&Rev_1100#AA04012700011123&0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}"
				if (v.ValueName.Contains("Volume"))
				{
					//Name: "\\??\\Volume{5c7fcd3b-2303-11e3-be7f-24fd52566ede}"
					var rawBytes = v.ValueDataRaw;

					var vol = new Volume();
					var subname = v.ValueName.Substring(11);
					vol.GUID = subname.Substring(0, subname.Length - 1);


					if (rawBytes[0] == 0x5f)
					{
						var someval = Encoding.Unicode.GetString(rawBytes, 8, rawBytes.Length - 8);

						var segs = someval.Split('&');

						vol.ValueGUID = segs[4].Split('{')[1].Replace("}", "");
						var segs2 = segs[3].Split('#');
						vol.Revision = segs2[0].Substring(4);
						vol.SerialNumber = segs2[1];
						vol.Vendor = segs[1].Substring(4);

						mountedList.Volumes.Add(vol);
					}
				}
				else if (v.ValueName.Contains("DosDevice"))
				{
					var rawBytes = v.ValueDataRaw;


					var dev = new DosDevice();
					//\DosDevices\C:
					dev.DriveLetter = v.ValueName.Substring(12);

					if (rawBytes[0] == 0x5f)
					{
						var someval = Encoding.Unicode.GetString(rawBytes, 8, rawBytes.Length - 8);

						var segs = someval.Split('&');

						dev.GUID = segs[4].Split('{')[1].Replace("}", "");
						var segs2 = segs[3].Split('#');
						dev.Revision = segs2[0].Substring(4);
						dev.SerialNumber = segs2[1];
						dev.Vendor = segs[1].Substring(4);

						mountedList.DosDevices.Add(dev);
					}
				}
			}

			return mountedList;
		}

		private List<USBEnum> GetEnumUSBData(string systemRegistryFilePath)
		{
			var usbEnumList = new List<USBEnum>();

			var registryHive = new RegistryHive(systemRegistryFilePath);

			//TODO get actual control set value
			const string keyname = @"ControlSet001\Enum\USB";

			var vendorinfo = new VendorLookup();

			var key = registryHive.FindKey(keyname);

			var subkeys = key.SubKeys.ToList();

			foreach (var k in subkeys.Where(u => u.KeyName.StartsWith("VID")))
			{
				var u = new USBEnum {LastDateTimeConnected = k.LastWriteTime.Value};

				var segs = k.KeyName.Split('&');
				u.VID_ID = int.Parse(segs[0].Substring(4), System.Globalization.NumberStyles.HexNumber);

				u.PID_ID = int.Parse(segs[1].Substring(4), System.Globalization.NumberStyles.HexNumber); 

				var details = vendorinfo.GetVendorandProductsFromIDs(u.VID_ID, u.PID_ID);

				u.ProductInfo = details;

				var sn = k.SubKeys.FirstOrDefault();
				if (sn != null)
				{
					u.SerialNumber = sn.KeyName;
					u.WindowsGeneratedSerial = (u.SerialNumber[1] == '&');
				}

				usbEnumList.Add(u);
			}

			return usbEnumList;
		}

		private TimeZoneInfo GetTimeZoneFromRegistry(string regFilePath)
		{
			var registryHive = new RegistryHive(regFilePath);

			//TODO determine currentcontrol set
			const string keyname = @"ControlSet001\Control\TimeZoneInformation";

			var key = registryHive.FindKey(keyname);

			var tzname = key.Values.Single(t => t.ValueName == "TimeZoneKeyName").ValueData;

			return TimeZoneInfo.FindSystemTimeZoneById(tzname);
		}

		private WinVerEnum GetWindowsVersion(string regFilePath)
		{
			var registryHive = new RegistryHive(regFilePath);

			const string keyname = @"Microsoft\Windows NT\CurrentVersion";

			var key = registryHive.FindKey(keyname);

			var productName = key.Values.Single(t => t.ValueName == "ProductName").ValueData;

			if (productName.Contains("XP"))
			{
				return WinVerEnum.WindowsXp;
			}

			if (productName.Contains("Vista"))
			{
				return WinVerEnum.WindowsVista;
			}

			if (productName.Contains("7"))
			{
				return WinVerEnum.Windows7;
			}

			if (productName.Contains("8"))
			{
				return WinVerEnum.Windows8;
			}

			return WinVerEnum.Unsupported;
		}
	}
}