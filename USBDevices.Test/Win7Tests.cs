using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;
using USBDevices.Classes;

namespace USBDevices.Test
{
	[TestFixture]
	class Win7Tests
	{
		private SetUpApiDevLog _devLog;
	    private SoftwareHiveInfo _softwareHiveInfo;
	    private SystemHiveInfo _systemHiveInfo;
	    private NtUserHiveInfo _ntuserHiveInfo;

		[TestFixtureSetUp]
		public void Initialize()
		{
			const string logPath = @"..\..\Hives\Win7\setupapi.dev.log";
            _devLog = new SetUpApiDevLog(logPath,TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));

            _softwareHiveInfo = new SoftwareHiveInfo(@"..\..\Hives\Win7\SOFTWARE");
            _systemHiveInfo = new SystemHiveInfo(@"..\..\Hives\Win7\SYSTEM");
            _ntuserHiveInfo = new NtUserHiveInfo(@"..\..\Hives\Win7\NTUser.DAT");
        }

		[Test]
		public void SetupApiLogShouldFindOneEntryAndValidate()
		{
			Check.That(_devLog.SetUpApiRecords.Count).IsEqualTo(8);

			var setupRecord = _devLog.SetUpApiRecords.First();

			Check.That(setupRecord).IsNotNull();

			Check.That(setupRecord.SerialNumber).IsEqualTo("07AC0A07DBC96520");

			Check.That(setupRecord.FirstConnectedDatetime.ToString()).IsEqualTo("3/24/2015 12:38:03 PM +00:00");

		    setupRecord = _devLog.SetUpApiRecords.Last();

            Check.That(setupRecord.SerialNumber).IsEqualTo("VOLUME");

            Check.That(setupRecord.FirstConnectedDatetime.ToString()).IsEqualTo("3/24/2015 12:40:36 PM +00:00");
        }

		[Test]
		public void SetupApiLogShouldReturnEmptyListWhenLogDoesNotExist()
		{
			var devLog = new SetUpApiDevLog(@"ThisIsABadPath", TimeZoneInfo.Utc);

			Check.That(devLog.SetUpApiRecords.Count).IsEqualTo(0);
		}

        [Test]
        public void SoftwareShouldGetWindows7ForOperatingSystem()
        {
            Check.That(_softwareHiveInfo.WindowsVersion).IsEqualTo(SoftwareHiveInfo.WindowsVersions.Windows7);
        }

        [Test]
        public void SoftwareShouldGetFourPortableMappedDevices()
        {
            Check.That(_softwareHiveInfo.PortableMappedDevices.Count).IsEqualTo(4);

            var port = _softwareHiveInfo.PortableMappedDevices.Single(t => t.SerialNumber == "2361808400440061");

            Check.That(port.SerialNumber).IsEqualTo("2361808400440061");
            Check.That(port.FriendlyName).IsEqualTo("X-Ways Portable");
            Check.That(port.GUID).IsEqualTo("");
            Check.That(port.Product).IsEqualTo("USB_FLASH_DRIVE");
            Check.That(port.Revision).IsEqualTo("1.00");
            Check.That(port.Vendor).IsEqualTo("ADATA");

            port = _softwareHiveInfo.PortableMappedDevices.Single(t => t.SerialNumber == "070723618BD74E22");

            Check.That(port.SerialNumber).IsEqualTo("070723618BD74E22");
            Check.That(port.FriendlyName).IsEqualTo("Thumb");
            Check.That(port.GUID).IsEqualTo("");
            Check.That(port.Product).IsEqualTo("MKNUFDVP32GB");
            Check.That(port.Revision).IsEqualTo("PMAP");
            Check.That(port.Vendor).IsEqualTo("MUSHKIN");

            port = _softwareHiveInfo.PortableMappedDevices.Single(t => t.SerialNumber == "07AC0A07DBC96520");

            Check.That(port.SerialNumber).IsEqualTo("07AC0A07DBC96520");
            Check.That(port.FriendlyName).IsEqualTo("IRM_SHV_X64");
            Check.That(port.GUID).IsEqualTo("");
            Check.That(port.Product).IsEqualTo("MKNUFDMH32GB");
            Check.That(port.Revision).IsEqualTo("PMAP");
            Check.That(port.Vendor).IsEqualTo("MUSHKIN");
            Check.That(port.ToString()).IsNotEmpty();

        }

        [Test]
        public void SoftwareShouldThrowFileNotFoundExeptionOnBadPath()
        {
            Check.ThatCode(() => {var r = new SoftwareHiveInfo(@"SomeUnknownPath"); }).Throws<FileNotFoundException>();
        }

        [Test]
        public void SoftwareShouldGetFourEmdDevices()
        {
            Check.That(_softwareHiveInfo.EmdMgmtDevices.Count).IsEqualTo(4);
        }


        [Test]
        public void SystemShouldThrowFileNotFoundExeptionOnBadPath()
        {
            Check.ThatCode(() => { var r = new SystemHiveInfo(@"SomeUnknownPath"); }).Throws<FileNotFoundException>();
        }


        [Test]
        public void SystemShouldFindMountainTimeZone()
        {
            Check.That(_systemHiveInfo.TimeZone).IsEqualTo(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
        }

        [Test]
        public void SystemShouldFindTwoMountedDevices()
        {
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.Count).IsEqualTo(2);

            Check.That(_systemHiveInfo.MountedDevices.DosDevices.First().DriveLetter).IsEqualTo("E:");
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.First().GUID).IsEqualTo("53f56307-b6bf-11d0-94f2-00a0c91efb8b");
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.First().Revision).IsEqualTo("0001");
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.First().SerialNumber).IsEqualTo("AA011004141209081070");
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.First().Vendor).IsEqualTo("SanDisk");

            Check.That(_systemHiveInfo.MountedDevices.DosDevices.Last().DriveLetter).IsEqualTo("F:");
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.Last().GUID).IsEqualTo("53f56307-b6bf-11d0-94f2-00a0c91efb8b");
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.Last().Revision).IsEqualTo("1.00");
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.Last().SerialNumber).IsEqualTo("2361808400440061");
            Check.That(_systemHiveInfo.MountedDevices.DosDevices.Last().Vendor).IsEqualTo("ADATA");
           
        }

        [Test]
        public void SystemShouldFindFourVolumes()
        {
            Check.That(_systemHiveInfo.MountedDevices.Volumes.Count).IsEqualTo(4);
            Check.That(_systemHiveInfo.MountedDevices.Volumes.First().Vendor).IsEqualTo("MUSHKIN");
            Check.That(_systemHiveInfo.MountedDevices.Volumes.First().GUID).IsEqualTo("75bc6505-d221-11e4-be20-000c2949b31f");
            Check.That(_systemHiveInfo.MountedDevices.Volumes.First().Revision).IsEqualTo("PMAP");
            Check.That(_systemHiveInfo.MountedDevices.Volumes.First().SerialNumber).IsEqualTo("07AC0A07DBC96520");
            Check.That(_systemHiveInfo.MountedDevices.Volumes.First().UsersWhoMountedDevice).IsNotNull();
            Check.That(_systemHiveInfo.MountedDevices.Volumes.First().ValueGUID).IsEqualTo("53f56307-b6bf-11d0-94f2-00a0c91efb8b");

            Check.That(_systemHiveInfo.MountedDevices.Volumes.Last().Vendor).IsEqualTo("SanDisk");
            Check.That(_systemHiveInfo.MountedDevices.Volumes.Last().GUID).IsEqualTo("75bc6529-d221-11e4-be20-000c2949b31f");
            Check.That(_systemHiveInfo.MountedDevices.Volumes.Last().Revision).IsEqualTo("0001");
            Check.That(_systemHiveInfo.MountedDevices.Volumes.Last().SerialNumber).IsEqualTo("AA011004141209081070");
            Check.That(_systemHiveInfo.MountedDevices.Volumes.Last().UsersWhoMountedDevice).IsNotNull();
            Check.That(_systemHiveInfo.MountedDevices.Volumes.Last().ValueGUID).IsEqualTo("53f56307-b6bf-11d0-94f2-00a0c91efb8b");
        }


        [Test]
        public void SystemShouldFindTwelveUsbEnums()
        {
            Check.That(_systemHiveInfo.USBEnums.Count).IsEqualTo(12);

            var usbEnum = _systemHiveInfo.USBEnums.Single(t => t.VID_ID == 1204);

            Check.That(usbEnum.VID_ID).IsEqualTo(1204);
            Check.That(usbEnum.PID_ID).IsEqualTo(25952);
            Check.That(usbEnum.ProductInfo.VendorName).IsEqualTo("Cypress Semiconductor Corp.");
            Check.That(usbEnum.ProductInfo.ProductDescription).IsEqualTo("CY7C65640 USB-2.0 \"TetraHub\"");
            Check.That(usbEnum.SerialNumber).IsEqualTo("");
            Check.That(usbEnum.WindowsGeneratedSerial).IsFalse();
            Check.That(usbEnum.LastDateTimeConnected.ToString()).IsEqualTo("11/21/2010 3:57:50 AM +00:00");
            Check.That(usbEnum.ToString()).IsNotEmpty();


            usbEnum = _systemHiveInfo.USBEnums.Single(t => t.VID_ID == 5118 && t.PID_ID == 12544);

            Check.That(usbEnum.VID_ID).IsEqualTo(5118);
            Check.That(usbEnum.PID_ID).IsEqualTo(12544);
            Check.That(usbEnum.ProductInfo.VendorName).IsEqualTo("Kingston Technology Company Inc.");
            Check.That(usbEnum.ProductInfo.ProductDescription).IsEqualTo("2/4 GB stick");
            Check.That(usbEnum.SerialNumber).IsEqualTo("07AC0A07DBC96520");
            Check.That(usbEnum.WindowsGeneratedSerial).IsFalse();
            Check.That(usbEnum.LastDateTimeConnected.ToString()).IsEqualTo("3/24/2015 12:38:02 PM +00:00");
        }


        [Test]
        public void SystemShouldFindTwentyOneStorageClasses()
        {
            Check.That(_systemHiveInfo.StorageClasses.Count).IsEqualTo(21);

            var storageClass = _systemHiveInfo.StorageClasses.Single(t => t.SerialNumber == "17b13437");

            Check.That(storageClass.SerialNumber).IsEqualTo("17b13437");
            Check.That(storageClass.DeviceType).IsEqualTo("SCSI");
            Check.That(storageClass.DeviceClass).IsEqualTo("Disk");
            Check.That(storageClass.Product).IsEqualTo("VIRTUAL_DISK");
            Check.That(storageClass.Revision).IsEqualTo("");
            Check.That(storageClass.SourceGUID).IsEqualTo("53f56307-b6bf-11d0-94f2-00a0c91efb8b");
            Check.That(storageClass.VolumeGUID).IsEqualTo("");
            Check.That(storageClass.LastWriteTime.ToString()).IsEqualTo("7/14/2009 5:08:05 AM +00:00");
            Check.That(storageClass.ToString()).IsNotEmpty();

            storageClass = _systemHiveInfo.StorageClasses.Single(t => t.SerialNumber == "07AC0A07DBC96520" && t.DeviceType == "USBSTOR");

            Check.That(storageClass.SerialNumber).IsEqualTo("07AC0A07DBC96520");
            Check.That(storageClass.DeviceType).IsEqualTo("USBSTOR");
            Check.That(storageClass.DeviceClass).IsEqualTo("Disk");
            Check.That(storageClass.Product).IsEqualTo("MKNUFDMH32GB");
            Check.That(storageClass.Revision).IsEqualTo("PMAP");
            Check.That(storageClass.SourceGUID).IsEqualTo("53f56307-b6bf-11d0-94f2-00a0c91efb8b");
            Check.That(storageClass.VolumeGUID).IsEqualTo("");
            Check.That(storageClass.LastWriteTime.ToString()).IsEqualTo("3/24/2015 12:38:03 PM +00:00");

            storageClass =
                _systemHiveInfo.StorageClasses.Single(
                    t => t.SerialNumber == "AA011004141209081070" && t.DeviceClass == "Disk" && t.DeviceType == "USBSTOR");

            Check.That(storageClass.SerialNumber).IsEqualTo("AA011004141209081070");
            Check.That(storageClass.DeviceType).IsEqualTo("USBSTOR");
            Check.That(storageClass.DeviceClass).IsEqualTo("Disk");
            Check.That(storageClass.Product).IsEqualTo("ExtremePro");
            Check.That(storageClass.Revision).IsEqualTo("0001");
            Check.That(storageClass.SourceGUID).IsEqualTo("53f56307-b6bf-11d0-94f2-00a0c91efb8b");
            Check.That(storageClass.VolumeGUID).IsEqualTo("");
            Check.That(storageClass.LastWriteTime.ToString()).IsEqualTo("3/24/2015 12:40:33 PM +00:00");

            storageClass =
         _systemHiveInfo.StorageClasses.Single(
             t => t.SerialNumber == "2GE3NQAF" && t.DeviceClass == "Disk" && t.DeviceType == "USBSTOR");

            Check.That(storageClass.SerialNumber).IsEqualTo("2GE3NQAF");
            Check.That(storageClass.DeviceType).IsEqualTo("USBSTOR");
            Check.That(storageClass.DeviceClass).IsEqualTo("Disk");
            Check.That(storageClass.Product).IsEqualTo("FreeAgent_Go");
            Check.That(storageClass.Revision).IsEqualTo("102F");
            Check.That(storageClass.SourceGUID).IsEqualTo("53f56307-b6bf-11d0-94f2-00a0c91efb8b");
            Check.That(storageClass.VolumeGUID).IsEqualTo("");
            Check.That(storageClass.LastWriteTime.ToString()).IsEqualTo("3/24/2015 12:38:19 PM +00:00");
        }


        [Test]
        public void SystemShouldFindFiveUsbDevices()
        {
            Check.That(_systemHiveInfo.UsbDevices.Count).IsEqualTo(5);

            var usbD = _systemHiveInfo.UsbDevices.Single(t => t.SerialNumber == "2361808400440061");

            Check.That(usbD.SerialNumber).IsEqualTo("2361808400440061");
            Check.That(usbD.Type).IsEqualTo("Disk");
            Check.That(usbD.Vendor).IsEqualTo("ADATA");
            Check.That(usbD.Product).IsEqualTo("USB_Flash_Drive");
            Check.That(usbD.Revision).IsEqualTo("1.00");
            Check.That(usbD.DriveLetters).IsEqualTo("");
            Check.That(usbD.FriendlyName).IsEqualTo("");
            Check.That(usbD.Volumes.Count).IsEqualTo(0);
            Check.That(usbD.FirstDateTimeConnected0064.ToString()).IsEqualTo("3/24/2015 12:39:04 PM +00:00");
            Check.That(usbD.UsbStorKeyLastWriteTime.ToString()).IsEqualTo("3/24/2015 12:39:04 PM +00:00");
            Check.That(usbD.ToString()).IsNotEmpty();

            usbD = _systemHiveInfo.UsbDevices.Single(t => t.SerialNumber == "AA011004141209081070");

            Check.That(usbD.SerialNumber).IsEqualTo("AA011004141209081070");
            Check.That(usbD.Type).IsEqualTo("Disk");
            Check.That(usbD.Vendor).IsEqualTo("SanDisk");
            Check.That(usbD.Product).IsEqualTo("ExtremePro");
            Check.That(usbD.Revision).IsEqualTo("0001");
            Check.That(usbD.DriveLetters).IsEqualTo("");
            Check.That(usbD.FriendlyName).IsEqualTo("");
            Check.That(usbD.Volumes.Count).IsEqualTo(0);
            Check.That(usbD.FirstDateTimeConnected0064.ToString()).IsEqualTo("3/24/2015 12:40:34 PM +00:00");
            Check.That(usbD.UsbStorKeyLastWriteTime.ToString()).IsEqualTo("3/24/2015 12:40:33 PM +00:00");

            usbD = _systemHiveInfo.UsbDevices.Single(t => t.SerialNumber == "2GE3NQAF");

            Check.That(usbD.SerialNumber).IsEqualTo("2GE3NQAF");
            Check.That(usbD.Type).IsEqualTo("Disk");
            Check.That(usbD.Vendor).IsEqualTo("Seagate");
            Check.That(usbD.Product).IsEqualTo("FreeAgent_Go");
            Check.That(usbD.Revision).IsEqualTo("102F");
            Check.That(usbD.DriveLetters).IsEqualTo("");
            Check.That(usbD.FriendlyName).IsEqualTo("");
            Check.That(usbD.Volumes.Count).IsEqualTo(0);
            Check.That(usbD.FirstDateTimeConnected0064.ToString()).IsEqualTo("3/24/2015 12:38:19 PM +00:00");
            Check.That(usbD.UsbStorKeyLastWriteTime.ToString()).IsEqualTo("3/24/2015 12:38:18 PM +00:00");
        }


	    [Test]
	    public void NtUserShouldFindEightMountPoint2Devices()
	    {
	        Check.That(_ntuserHiveInfo.MountPoint2Devices.Count).IsEqualTo(8);

	        var mp = _ntuserHiveInfo.MountPoint2Devices.Single(t => t.Guid == "CPC");

            Check.That(mp.LastWriteTime.ToString()).IsEqualTo("3/24/2015 12:28:29 PM +00:00");

            mp = _ntuserHiveInfo.MountPoint2Devices.Single(t => t.Guid == "142b7d2f-d229-11e4-bed0-806e6f6e6963");

            Check.That(mp.LastWriteTime.ToString()).IsEqualTo("3/24/2015 12:39:55 PM +00:00");

            mp = _ntuserHiveInfo.MountPoint2Devices.Single(t => t.Guid == "142b7d32-d229-11e4-bed0-806e6f6e6963");

            Check.That(mp.LastWriteTime.ToString()).IsEqualTo("3/24/2015 12:39:55 PM +00:00");

            mp = _ntuserHiveInfo.MountPoint2Devices.Single(t => t.Guid == "75bc651f-d221-11e4-be20-000c2949b31f");

            Check.That(mp.LastWriteTime.ToString()).IsEqualTo("3/24/2015 12:39:05 PM +00:00");
            Check.That(mp.ToString()).IsNotEmpty();
        }

	    [Test]
	    public void NtUserShouldFindProfileName()
	    {
            Check.That(_ntuserHiveInfo.ProfileName).IsEqualTo("eric");
            
        }

        [Test]
        public void NtUserShouldThrowFileNotFoundExeptionOnBadPath()
        {
            Check.ThatCode(() => { var r = new NtUserHiveInfo(@"SomeUnknownPath"); }).Throws<FileNotFoundException>();
        }


        [Test]
        public void UsbDevicesShouldBePopulated()
        {
            const string logPath = @"..\..\Hives\Win7\setupapi.dev.log";
            _devLog = new SetUpApiDevLog(logPath, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));

            _softwareHiveInfo = new SoftwareHiveInfo(@"..\..\Hives\Win7\SOFTWARE");
            _systemHiveInfo = new SystemHiveInfo(@"..\..\Hives\Win7\SYSTEM");
            _ntuserHiveInfo = new NtUserHiveInfo(@"..\..\Hives\Win7\NTUser.DAT");

            var users = new List<string>();
            users.Add(@"..\..\Hives\Win7\NTUSER.DAT");

            var usbDevices = new UsbDevices(@"..\..\Hives\Win7\SYSTEM", @"..\..\Hives\Win7\SOFTWARE", @"..\..\Hives\Win7\setupapi.dev.log", users);

            Check.That(usbDevices).IsNotNull();

            foreach (var usbDevice in usbDevices.SystemHive.UsbDevices)
            {
                Console.WriteLine(usbDevice);
            }
        }

    }
    }
