using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBDevices.Classes
{
 public   class VolumeInfo
    {
        public string VolumeName { get; }
        public uint VolumeSerialNumber { get; }
        public string VolumeSerialNumberHex { get; }

     public VolumeInfo(string volumeName, uint volumeSerialNumber, string volumeSerialNumberHex )
     {
         VolumeName = volumeName;
         VolumeSerialNumber = volumeSerialNumber;
         VolumeSerialNumberHex = volumeSerialNumberHex;
     }


    }
}
