namespace USBDevices
{
	internal class EmdMgmtDevice
	{
		public EmdMgmtDevice()
		{
			DeviceSerialNumber = string.Empty;
			VolumeSerialNumberHex = string.Empty;

			VolumeName = string.Empty;
			DriveLetter = string.Empty;
		}

		//_??_USBSTOR#Disk
		//Ven_Flash
		//Prod_Drive_SM_USB20
		//Rev_1100#AA04012700013494
		//0#{53f56307-b6bf-11d0-94f2-00a0c91efb8b}PHOTOS BACK_1141839789


		public string DeviceSerialNumber { get; set; }
		public string VolumeSerialNumberHex { get; set; }
		public uint VolumeSerialNumber { get; set; }
		public string VolumeName { get; set; }
		public string DriveLetter { get; set; }
	}
}