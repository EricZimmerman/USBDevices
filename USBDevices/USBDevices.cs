using System;
using System.Collections.Generic;
using System.Linq;
using USBDevices.Classes;

namespace USBDevices
{
    //http://msdn.microsoft.com/en-us/library/windows/hardware/ff541389(v=vs.85).aspx
    //http://hackingexposedcomputerforensicsblog.blogspot.com/2013/08/daily-blog-67-understanding-artifacts.html
    //http://www.forensicswiki.org/wiki/USB_History_Viewing
    //http://www.forensicmag.com/articles/2012/06/windows-7-registry-forensics-part-5

    public class UsbDevices
    {
        public UsbDevices(string systemRegistryFilePath, string softwareRegistryFilePath, string setupApiDevLogPath,
            List<string> ntUserFilePaths)
        {
            SoftwareHive = new SoftwareHiveInfo(softwareRegistryFilePath);
            SystemHive = new SystemHiveInfo(systemRegistryFilePath);

            var setupApiRecords = new SetUpApiDevLog(setupApiDevLogPath, SystemHive.TimeZone);


            var ntusers = new List<NtUserHiveInfo>();

            foreach (var ntUserFilePath in ntUserFilePaths)
            {
                var userInfo = new NtUserHiveInfo(ntUserFilePath);


                foreach (var mountPoint2Device in userInfo.MountPoint2Devices)
                {
                    var mp2Device = mountPoint2Device;
                    var mountedDev =
                        SystemHive.MountedDevices.Volumes.SingleOrDefault(
                            y => y.GUID.ToLowerInvariant() == mp2Device.Guid.ToLowerInvariant());

                    mountedDev?.UsersWhoMountedDevice.Add(new UserMountInformation(userInfo.ProfileName,
                        mp2Device.LastWriteTime));
                }

                ntusers.Add(userInfo);
            }


            foreach (var usbDevice in SystemHive.UsbDevices)
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
                    SystemHive.USBEnums.SingleOrDefault(
                        y => y.SerialNumber.ToLowerInvariant() == usbDevice.SerialNumber.ToLowerInvariant());

                if (enumRecord != null)
                {
                    usbDevice.VendorId = enumRecord.VID_ID;
                    usbDevice.ProductId = enumRecord.PID_ID;
                    usbDevice.VendorNameFromID = enumRecord.ProductInfo.VendorName;
                    usbDevice.ProductNameFromID = enumRecord.ProductInfo.ProductDescription;
                }

                var portDel =
                    SoftwareHive.PortableMappedDevices.SingleOrDefault(
                        y => y.SerialNumber.ToLowerInvariant() == usbDevice.SerialNumber.ToLowerInvariant());

                if (portDel != null)
                {
                    usbDevice.FriendlyName = portDel.FriendlyName;
                }

                var mountedDevsDOS =
                    SystemHive.MountedDevices.DosDevices.Where(
                        y => y.SerialNumber.ToLowerInvariant() == device.SerialNumber.ToLowerInvariant());

                if (mountedDevsDOS.Any())
                {
                    usbDevice.DriveLetters = string.Join(", ", mountedDevsDOS.Select(y => y.DriveLetter));
                }


                var emdDevices =
                    SoftwareHive.EmdMgmtDevices.Where(
                        y => y.DeviceSerialNumber.ToLowerInvariant() == device.SerialNumber.ToLowerInvariant());

                if (emdDevices.Any())
                {
                    foreach (var emdMgmtDevice in emdDevices)
                    {
                        usbDevice.Volumes.Add(new VolumeInfo(emdMgmtDevice.VolumeName,emdMgmtDevice.VolumeSerialNumber, emdMgmtDevice.VolumeSerialNumberHex));
                    }
                }

                var storage =
                    SystemHive.StorageClasses.FirstOrDefault(
                        y => y.SerialNumber.ToLowerInvariant() == usbDevice.SerialNumber.ToLowerInvariant());

                if (storage != null)
                {
                    usbDevice.FirstDateTimeConnectedStorageClass = storage.LastWriteTime;
                }


                var volume =
                    SystemHive.MountedDevices.Volumes.SingleOrDefault(
                        y => y.SerialNumber.ToLowerInvariant() == device.SerialNumber.ToLowerInvariant());

                if (volume != null)
                {
                    //TODO needs test
                    device.UsersWhoMountedDevice = volume.UsersWhoMountedDevice;
                }
            }
        }

        public SoftwareHiveInfo SoftwareHive { get; }
        public SystemHiveInfo SystemHive { get; }
    }
}