using System;

namespace USBDevices
{
	//##?#SCSI#Disk&Ven_Samsung&Prod_SSD_840_PRO_Seri#4&1fa8c96&0&000000#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}
	//##?#SCSI#Disk&Ven_WT055&Prod_ST9120822AS#4&1fa8c96&0&030000#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}
	//##?#USBSTOR#Disk&Ven_&Prod_USB_2.0&Rev_#0&0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}


	//##?#USB#VID_0000&PID_0002#5&328737cf&0&3#{a5dcbf10-6530-11d2-901f-00c04fb951ed}
	internal class StorageClass
	{
		public StorageClass()
		{
			DeviceType = string.Empty;
			DeviceClass = string.Empty;
			Vendor = string.Empty;
			Product = string.Empty;
			Revision = string.Empty;
			SerialNumber = string.Empty;
			VolumeGUID = string.Empty;
			SourceGUID = string.Empty;
		}

		public string DeviceType { get; set; }
		public string DeviceClass { get; set; }
		public string Vendor { get; set; }
		public string Product { get; set; }
		public string Revision { get; set; }
		public string SerialNumber { get; set; }
		public string VolumeGUID { get; set; }
		public string SourceGUID { get; set; }
		public DateTimeOffset LastWriteTime { get; set; }
	}
}