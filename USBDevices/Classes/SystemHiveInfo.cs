using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Registry;

namespace USBDevices.Classes
{
    public class SystemHiveInfo
    {
        private readonly string _controlSetPath = "ControlSet001";
        private readonly RegistryHiveOnDemand _hive;

        public SystemHiveInfo(string systemHivePath)
        {
            if (File.Exists(systemHivePath) == false)
            {
                throw new FileNotFoundException();
            }

            _hive = new RegistryHiveOnDemand(systemHivePath);

            _controlSetPath = GetCurrentControlSet();

            TimeZone = GetTimeZoneFromRegistry();
            USBEnums = GetEnumUSBData();
            MountedDevices = GetMountedDevices();
            StorageClasses = GetStorageClassesDevices();
            UsbDevices = GetUSBDeviceList();
        }

        public TimeZoneInfo TimeZone { get; }
        public List<USBEnum> USBEnums { get; }
        public MountedDevicesInfo MountedDevices { get; }
        public List<StorageClass> StorageClasses { get; }
        public List<USBDevice> UsbDevices { get; }

        private string GetCurrentControlSet()
        {
            const string keyname = @"Select";

            var key = _hive.GetKey(keyname);

            var setNumber = key.Values.Single(t => t.ValueName == "Current").ValueData;

            return $"Controlset00{setNumber}";
        }

        private TimeZoneInfo GetTimeZoneFromRegistry()
        {
            string keyname = $"{_controlSetPath}\\Control\\TimeZoneInformation";

            var key = _hive.GetKey(keyname);

            var tzname = key.Values.Single(t => t.ValueName == "TimeZoneKeyName").ValueData;

            return TimeZoneInfo.FindSystemTimeZoneById(tzname);
        }

        private List<USBEnum> GetEnumUSBData()
        {
            var usbEnumList = new List<USBEnum>();

            string keyname = $"{_controlSetPath}\\Enum\\USB";

            var vendorinfo = new VendorLookup();

            var key = _hive.GetKey(keyname);

            var subkeys = key.SubKeys.ToList();

            foreach (var k in subkeys.Where(u => u.KeyName.StartsWith("VID")))
            {
                var u = new USBEnum {LastDateTimeConnected = k.LastWriteTime.Value};

                var segs = k.KeyName.Split('&');
                u.VID_ID = int.Parse(segs[0].Substring(4), NumberStyles.HexNumber);

                u.PID_ID = int.Parse(segs[1].Substring(4), NumberStyles.HexNumber);

                var details = vendorinfo.GetVendorandProductsFromIDs(u.VID_ID, u.PID_ID);

                u.ProductInfo = details;

                var subkey = _hive.GetKey(k.KeyPath);

                var sn = subkey.SubKeys.FirstOrDefault();
                if (sn != null)
                {
                    u.SerialNumber = sn.KeyName;
                    u.WindowsGeneratedSerial = (u.SerialNumber[1] == '&');
                }

                usbEnumList.Add(u);
            }

            return usbEnumList;
        }

        private MountedDevicesInfo GetMountedDevices()
        {
            var mountedList = new MountedDevicesInfo();

            const string keyname = @"MountedDevices";

            var key = _hive.GetKey(keyname);

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

        private List<StorageClass> GetStorageClassesDevices()
        {
            var keyname53fa = $"{_controlSetPath}\\Control\\DeviceClasses\\" + "{53f56307-b6bf-11d0-94f2-00a0c91efb8b}";
            var keyname53fb = $"{_controlSetPath}\\Control\\DeviceClasses\\" + "{53f5630d-b6bf-11d0-94f2-00a0c91efb8b}";
            var keynamea5d = $"{_controlSetPath}\\Control\\DeviceClasses\\" + "{a5dcbf10-6530-11d2-901f-00c04fb951ed}";

            var scList = new List<StorageClass>();

            var key = _hive.GetKey(keyname53fa);

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

                //TODO VOLUME GUID from mounted devices??

                scList.Add(sc);
            }

            key = _hive.GetKey(keyname53fb);

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

                if (sc.DeviceClass == "DISK")
                {
                    sc.DeviceClass = "Disk";
                }

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

                //TODO VOLUME GUID from mounted devices??

                scList.Add(sc);
            }

            key = _hive.GetKey(keynamea5d);

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

                //TODO VOLUME GUID from mounted devices??

                scList.Add(sc);
            }

            return scList;
        }

        private List<USBDevice> GetUSBDeviceList()
        {
            string keyName = $"{_controlSetPath}\\Enum\\USBSTOR";

            var devList = new List<USBDevice>();

            var key = _hive.GetKey(keyName);

            foreach (var k in key.SubKeys)
            {
                var segments = k.KeyName.Split('&');

                var usbDevice = new USBDevice
                {
                    Type = segments[0],
                    Vendor = segments[1].Substring(4),
                    Product = segments[2].Substring(5),
                    Revision = segments[3].Substring(4),
                    UsbStorKeyLastWriteTime = k.LastWriteTime.Value
                };

                if (usbDevice.Type == "CdRom")
                {
                    continue;
                }

                var subkey1 = _hive.GetKey(k.KeyPath);

                var serialKey = _hive.GetKey(subkey1.SubKeys.FirstOrDefault()?.KeyPath);

                if (serialKey != null)
                {
                    usbDevice.SerialNumber = serialKey.KeyName.Split('&')[0];

                    //SYSTEM\ControlSet001\Enum\USBSTOR\Disk&Ven_Multiple&Prod_Card__Reader&Rev_1.00\058F63666438&0\Properties\{83da6326-97a6-4088-9453-a1923f573b29}\0064

                    var props = serialKey.SubKeys.Single(t => t.KeyName == "Properties");

                    var subkey = _hive.GetKey(props.KeyPath);

                    foreach (var registryKey in subkey.SubKeys)
                    {
                        var guidKey = _hive.GetKey(registryKey.KeyPath);

                        if (guidKey.KeyName == "{83da6326-97a6-4088-9453-a1923f573b29}")
                        {
                            foreach (var subKey in guidKey.SubKeys)
                            {
                                var tsKey = _hive.GetKey(subKey.KeyPath);

                                if (tsKey.KeyName.Contains("0064"))
                                {
                                    usbDevice.FirstDateTimeConnected0064 = tsKey.LastWriteTime;
                                }

                                if (tsKey.KeyName.Contains("0066"))
                                {
                                    usbDevice.LastDateTimeConnected0066 = tsKey.LastWriteTime;
                                }

                                if (tsKey.KeyName.Contains("0067"))
                                {
                                    usbDevice.LastDateTimeRemoved0067 = tsKey.LastWriteTime;
                                }
                            }
                        }
                    }
                }

                devList.Add(usbDevice);
            }

            return devList;
        }
    }
}