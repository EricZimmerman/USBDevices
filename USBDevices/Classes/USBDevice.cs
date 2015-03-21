using System;
using System.Collections.Generic;

namespace USBDevices
{
	public class USBDevice
	{
		public USBDevice()
		{
			Type = string.Empty;
			Vendor = string.Empty;
			Product = string.Empty;
			FriendlyName = string.Empty;
			DriveLetters = string.Empty;
			ProductNameFromID = "Unknown";
			VendorNameFromID = "Unknown";

			Volumes = new List<Tuple<string, uint, string>>();

			UsersWhoMountedDevice = new List<Tuple<string, DateTimeOffset>>();
		}

		public string Type { get; set; }
		public string Vendor { get; set; }
		public string Product { get; set; }
		public int VendorID { get; set; }
		public int ProductID { get; set; }
		public string ProductNameFromID { get; set; }
		public string VendorNameFromID { get; set; }
		public string FriendlyName { get; set; }
		public string DriveLetters { get; set; }
		public List<Tuple<string, uint, string>> Volumes { get; set; }
		public List<Tuple<string, DateTimeOffset>> UsersWhoMountedDevice { get; set; }
		public string Revision { get; set; }
		public DateTimeOffset USBStorTimestamp { get; set; }
		public string SerialNumber { get; set; }
		public DateTimeOffset? FirstDateTimeConnected0064 { get; set; }
		public DateTimeOffset? LastDateTimeConnected0066 { get; set; }
		public DateTimeOffset? LastDateTimeRemoved0067 { get; set; }
		public DateTimeOffset? FirstDateTimeConnectedStorageClass { get; set; }
		public DateTimeOffset? FirstDateTimeConnectedSetupAPI { get; set; }
	}
}