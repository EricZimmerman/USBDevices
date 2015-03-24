using System;
using System.Collections.Generic;
using System.Text;
using USBDevices.Classes;

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

            Volumes = new List<VolumeInfo>();

            UsersWhoMountedDevice = new List<UserMountInformation>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Type: {Type}");
            sb.AppendLine($"Vendor: {Vendor}, Vendor ID: {VendorId}, Vendor Name From ID: {VendorNameFromID}");
            sb.AppendLine($"Product: {Product}, Product ID: {ProductId}, Product Name From ID: {ProductNameFromID}");
            sb.AppendLine($"Revision: {Revision}");
            sb.AppendLine($"Serial Number: {SerialNumber}");
            sb.AppendLine();
            
            sb.AppendLine($"Friendly Name (drive label): {FriendlyName}");
            sb.AppendLine($"Drive Letters: {DriveLetters}");
            sb.AppendLine($"Volume count: {Volumes.Count}");

            sb.AppendLine();
            
            sb.AppendLine($"Users Who Mounted Device count: {UsersWhoMountedDevice.Count}");
            sb.AppendLine($"Users Who Mounted Device: {string.Join(",", UsersWhoMountedDevice)}");
            
            sb.AppendLine("Timestamps");

            if (FirstDateTimeConnectedSetupApi.HasValue)
            {
                sb.AppendLine($"First DateTime Connected (SetupApi log): {FirstDateTimeConnectedSetupApi.Value}");
            }

            if (FirstDateTimeConnected0064.HasValue)
            {
                sb.AppendLine($"First DateTime Connected (0064 key): {FirstDateTimeConnected0064.Value}");
            }

            if (FirstDateTimeConnectedStorageClass.HasValue)
            {
                sb.AppendLine($"First DateTime Connected (StorageClass key): {FirstDateTimeConnectedStorageClass.Value}");
            }
            
            if (LastDateTimeConnected0066.HasValue)
            {
                sb.AppendLine($"Last DateTime Connected (0066 key): {LastDateTimeConnected0066.Value}");
            }
            if (LastDateTimeRemoved0067.HasValue)
            {
                sb.AppendLine($"Last DateTime Removed (0067 key): {LastDateTimeRemoved0067.Value}");
            }

            sb.AppendLine($"UsbStor Key LastWriteTime: {UsbStorKeyLastWriteTime}");


            return sb.ToString();
        }

        public string Type { get; set; }
        public string Vendor { get; set; }
        public string Product { get; set; }
        public int VendorId { get; set; }
        public int ProductId { get; set; }
        public string ProductNameFromID { get; set; }
        public string VendorNameFromID { get; set; }
        public string FriendlyName { get; set; }
        public string DriveLetters { get; set; }
        public List<VolumeInfo> Volumes { get; set; }
        public List<UserMountInformation> UsersWhoMountedDevice { get; set; }
        public string Revision { get; set; }
        public DateTimeOffset UsbStorKeyLastWriteTime { get; set; }
        public string SerialNumber { get; set; }
        public DateTimeOffset? FirstDateTimeConnected0064 { get; set; }
        public DateTimeOffset? LastDateTimeConnected0066 { get; set; }
        public DateTimeOffset? LastDateTimeRemoved0067 { get; set; }
        public DateTimeOffset? FirstDateTimeConnectedStorageClass { get; set; }
        public DateTimeOffset? FirstDateTimeConnectedSetupApi { get; set; }
    }
}