using System;
using System.Collections.Generic;
using System.Text;

namespace USBDevices
{
    public class MountedDevicesInfo
    {
        public MountedDevicesInfo()
        {
            DosDevices = new List<DosDevice>();
            Volumes = new List<Volume>();
        }

        public List<DosDevice> DosDevices { get; set; }
        public List<Volume> Volumes { get; set; }
    }

    public class DosDevice
    {
        public string DriveLetter { get; set; }
        public string Vendor { get; set; }
        public string Revision { get; set; }
        public string SerialNumber { get; set; }
        public string GUID { get; set; }
    }

    public class Volume
    {
        public Volume()
        {
            UsersWhoMountedDevice = new List<UserMountInformation>();
        }

        public string GUID { get; set; }
        public string Vendor { get; set; }
        public string Revision { get; set; }
        public string SerialNumber { get; set; }
        public string ValueGUID { get; set; }
        public List<UserMountInformation> UsersWhoMountedDevice { get; set; }
    }

    public class UserMountInformation
    {
        public string ProfileName { get;  }
        public DateTimeOffset MountedDateTimeOffset { get; }

        public UserMountInformation(string profileName, DateTimeOffset mountTimestamp)
        {
            ProfileName = profileName;
            MountedDateTimeOffset = mountTimestamp;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Profile name: {ProfileName}, Mounted DateTimeOffset: {MountedDateTimeOffset}");

            return sb.ToString();
        }
    }
}