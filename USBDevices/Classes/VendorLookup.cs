using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using USBDevices.Properties;

#if DEBUG
	[assembly: InternalsVisibleTo("USBDevices.Test")]
#endif

namespace USBDevices.Classes
{
	internal class VendorLookup
	{
		private readonly Dictionary<int, VendorInformation> _vendorInformations = new Dictionary<int, VendorInformation>();

		public VendorLookup()
		{
			var rawList = Resources.vendorproductlist;

			var lines = rawList.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

			var lastVendorId = 0;

			foreach (var line in lines)
			{
				if (line.StartsWith("#"))
				{
					continue;
				}
				if (line.StartsWith("\t"))
				{
					var num = line.Substring(1, 4);
                    var prodId = int.Parse(num, System.Globalization.NumberStyles.HexNumber);
					var prodName = line.Substring(7);
					_vendorInformations[lastVendorId].ProductInformation.Add(prodId, prodName);
				}
				else
				{
					var num = line.Substring(0, 4);
                    var vendorId = int.Parse(num, System.Globalization.NumberStyles.HexNumber);
					lastVendorId = vendorId;
					var vendorName = line.Substring(6);
					_vendorInformations.Add(vendorId,new VendorInformation(vendorId,vendorName));
				}
			}

			rawList = Resources.usbVendors;
			lines = rawList.Split('\n');
			foreach (var line in lines)
			{
				if (line.StartsWith("#"))
				{
					continue;

				}
				var segs = line.Split('|');
				if (segs.Length < 2)
				{
					continue;
				}
				var hexId = int.Parse(segs[0]);
				var vendor = segs[1];

				if (!_vendorInformations.ContainsKey(hexId))
				{
					_vendorInformations.Add(hexId,
						new VendorInformation(hexId,vendor));
				}
			}
		}

		public ProductInformation GetVendorandProductsFromIDs(int vendorId, int productId)
		{
			
			var ans = new ProductInformation();

			if (_vendorInformations.ContainsKey(vendorId))
			{
				ans.VendorName = _vendorInformations[vendorId].VendorName;

				var prod = _vendorInformations[vendorId].ProductInformation.SingleOrDefault(t => t.Key == productId);

				if (prod.Value != null)
				{
					ans.ProductDescription = prod.Value;
                }
			}

			return ans;
		}
	}
}