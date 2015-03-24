using System.Collections.Generic;
using System.IO;
using System.Linq;
using Registry;

namespace USBDevices.Classes
{
    public class NtUserHiveInfo
    {
        private readonly RegistryHiveOnDemand _hive;

        public NtUserHiveInfo(string ntUserHivePath)
        {
            if (File.Exists(ntUserHivePath) == false)
            {
                throw new FileNotFoundException();
            }

            _hive = new RegistryHiveOnDemand(ntUserHivePath);

            ProfileName = GetProfileName();
            MountPoint2Devices = GetMountpoint2Devices();
        }

        public string ProfileName { get; }
        public List<MountPoint2Device> MountPoint2Devices { get; }

        private string GetProfileName()
        {
            const string keyname = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";

            var key = _hive.GetKey(keyname);

            return key.Values.Single(t => t.ValueName == "Desktop").ValueData.Split('\\')[2];
        }

        private List<MountPoint2Device> GetMountpoint2Devices()
        {
            var mp2 = new List<MountPoint2Device>();

            const string keyname = @"Software\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2";

            var key = _hive.GetKey(keyname);

            foreach (var k in key.SubKeys)
            {
                var mp = new MountPoint2Device();
                mp.Guid = k.KeyName.Replace("{", "").Replace("}", "");
                mp.LastWriteTime = k.LastWriteTime.Value;

                mp2.Add(mp);
            }

            return mp2;
        }
    }
}