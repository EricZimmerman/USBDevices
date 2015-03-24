using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Registry;

namespace USBDevices.Classes
{
    public class SoftwareHiveInfo
    {
        public enum WindowsVersions
        {
            WindowsXp,
            WindowsVista,
            Windows7,
            Windows8X,
            Windows10,
            Unsupported
        }

        private readonly RegistryHiveOnDemand _hive;

        public SoftwareHiveInfo(string softwareHivePath)
        {
            if (File.Exists(softwareHivePath) == false)
            {
                throw new FileNotFoundException();
            }

            _hive = new RegistryHiveOnDemand(softwareHivePath);

            WindowsVersion = GetWindowsVersion();
            PortableMappedDevices = GetWindowsPortableDevices();
            EmdMgmtDevices = GetEmdMgmtList();
        }

        public WindowsVersions WindowsVersion { get; }
        public List<PortableMappedDevice> PortableMappedDevices { get; }
        public List<EmdMgmtDevice> EmdMgmtDevices { get; }

        private WindowsVersions GetWindowsVersion()
        {
            const string keyName = @"Microsoft\Windows NT\CurrentVersion";

            var key = _hive.GetKey(keyName);

            var productName = key.Values.Single(t => t.ValueName == "ProductName").ValueData;

            if (productName.Contains("XP"))
            {
                return WindowsVersions.WindowsXp;
            }

            if (productName.Contains("Vista"))
            {
                return WindowsVersions.WindowsVista;
            }

            if (productName.Contains("7"))
            {
                return WindowsVersions.Windows7;
            }

            if (productName.Contains("8"))
            {
                return WindowsVersions.Windows8X;
            }

            if (productName.Contains("10"))
            {
                return WindowsVersions.Windows10;
            }

            return WindowsVersions.Unsupported;
        }

        private List<PortableMappedDevice> GetWindowsPortableDevices()
        {
            var portableDevs = new List<PortableMappedDevice>();

            const string keyname = @"Microsoft\Windows Portable Devices\Devices";

            var key = _hive.GetKey(keyname);

            foreach (var v in key.SubKeys)
            {
                //SWD#WPDBUSENUM#_??_USBSTOR#DISK
                //VEN_SANDISK
                //PROD_SANDISK_ULTRA
                //REV_PMAP#A200435	D2C03E709
                //0#{53F56307-B6BF-11D0-94F2-00A0C91EFB8B}

                //WPDBUSENUMROOT#UMB#2&37C186B&0&STORAGE#VOLUME#_??_USBSTOR#DISK&VEN_ADATA&PROD_USB_FLASH_DRIVE&REV_1.00#2361808400440061&0#
                //WPDBUSENUMROOT#UMB#2&37C186B&0&STORAGE#VOLUME#_??_USBSTOR#DISK&VEN_MUSHKIN&PROD_MKNUFDMH32GB&REV_PMAP#07AC0A07DBC96520&0#
                //WPDBUSENUMROOT#UMB#2&37C186B&0&STORAGE#VOLUME#_??_USBSTOR#DISK&VEN_MUSHKIN&PROD_MKNUFDVP32GB&REV_PMAP#070723618BD74E22&0#
                //WPDBUSENUMROOT#UMB#2&37C186B&0&STORAGE#VOLUME#_??_USBSTOR#DISK&VEN_SANDISK&PROD_EXTREMEPRO&REV_0001#AA011004141209081070&0#

                //Win81
                //SWD#WPDBUSENUM#_??_USBSTOR#DISK&VEN_MUSHKIN&PROD_MKNUFDMH32GB&REV_PMAP#07AC0A07DBC96520&0#{53F56307-B6BF-11D0-94F2-00A0C91EFB8B}

                //Win10
                //SWD#WPDBUSENUM#_??_USBSTOR#DISK&VEN_ADATA&PROD_USB_FLASH_DRIVE&REV_1.00#2361808400440061&0#{53F56307-B6BF-11D0-94F2-00A0C91EFB8B}
                //SWD#WPDBUSENUM#_??_USBSTOR#DISK&VEN_MUSHKIN&PROD_MKNUFDMH32GB&REV_PMAP#07AC0A07DBC96520&0#{53F56307-B6BF-11D0-94F2-00A0C91EFB8B}

                var subKey = _hive.GetKey(v.KeyPath);

                if (!subKey.KeyName.Contains("USBSTOR#DISK"))
                {
                    continue;
                }

                var chunks = subKey.KeyName.Split(new[] {"??"}, StringSplitOptions.RemoveEmptyEntries);
                var segs = chunks[1].Split('&');

                var pd = new PortableMappedDevice
                {
                    Vendor = segs[1].Substring(4),
                    Product = segs[2].Substring(5)
                };
                var segs2 = segs[3].Split('#');
                pd.Revision = segs2[0].Substring(4);
                pd.SerialNumber = segs2[1];
                try
                {
                    pd.GUID = segs[4].Split('{')[1].Replace("}", "");
                }
                catch
                {
                    //sometimes the GUID isn't there
                    //TODO. do we need to add 53F56307-B6BF-11D0-94F2-00A0C91EFB8B as default?
                }


                pd.FriendlyName = subKey.Values.Single(t => t.ValueName == "FriendlyName").ValueData;

                portableDevs.Add(pd);
            }

            return portableDevs;
        }

        private List<EmdMgmtDevice> GetEmdMgmtList()
        {
            var emdDevices = new List<EmdMgmtDevice>();

            const string keyname = @"Microsoft\Windows NT\CurrentVersion\EMDMgmt";

            var key = _hive.GetKey(keyname);

            foreach (var v in key.SubKeys)
            {
                //Win7
                //_??_USBSTOR#Disk&Ven_ADATA&Prod_USB_Flash_Drive&Rev_1.00#2361808400440061&0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}X-Ways Portable_3489848603
                //_??_USBSTOR#Disk&Ven_MUSHKIN&Prod_MKNUFDMH32GB&Rev_PMAP#07AC0A07DBC96520&0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}IRM_SHV_X64_1817879396

                //Win81
                //_??_USBSTOR#Disk&Ven_MUSHKIN&Prod_MKNUFDMH32GB&Rev_PMAP#07AC0A07DBC96520&0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}IRM_SHV_X64_1817879396

                //Win10
                //NOT IN WINDOWS 10!!

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
    }
}