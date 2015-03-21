using System;
using System.Collections.Generic;

namespace USBDevices
{
	internal class MountedDevice
	{
		public MountedDevice()
		{
			DosDevices = new List<DosDevice>();
			Volumes = new List<Volume>();
		}

		public List<DosDevice> DosDevices { get; set; }
		public List<Volume> Volumes { get; set; }
	}

	internal class DosDevice
	{
		public string DriveLetter { get; set; }
		public string Vendor { get; set; }
		public string Revision { get; set; }
		public string SerialNumber { get; set; }
		public string GUID { get; set; }
	}

	internal class Volume
	{
		public Volume()
		{
			UsersWhoMountedDevice = new List<Tuple<string, DateTimeOffset>>();
		}

		public string GUID { get; set; }
		public string Vendor { get; set; }
		public string Revision { get; set; }
		public string SerialNumber { get; set; }
		public string ValueGUID { get; set; }
		public List<Tuple<string, DateTimeOffset>> UsersWhoMountedDevice { get; set; }
	}
}