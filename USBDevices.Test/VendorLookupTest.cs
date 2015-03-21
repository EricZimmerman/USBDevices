using System.Collections.Generic;
using System.Linq;
using NFluent;
using NUnit.Framework;
using USBDevices.Classes;

namespace USBDevices.Test
{
	[TestFixture]
	public class VendorLookupTest
	{
		private VendorLookup _vendorLooup;

		[TestFixtureSetUp]
		public void Initialize()
		{
			_vendorLooup = new VendorLookup();
		}

		[Test]
		public void ShouldReturnVendorInformation()
		{
			var vendorInfo = new VendorInformation(10101, "Test vendor");
            Check.That(vendorInfo).IsNotNull();
			Check.That(vendorInfo.VendorName).IsEqualTo("Test vendor");
			Check.That(vendorInfo.VendorId).IsEqualTo(10101);
			Check.That(vendorInfo.ProductInformation.Count).IsEqualTo(0);

			vendorInfo.ProductInformation.Add(20202, "Test product");
			Check.That(vendorInfo.ProductInformation.Count).IsEqualTo(1);

			var prodInfo =  vendorInfo.ProductInformation.First();
			Check.That(prodInfo.Value).IsNotNull();
			Check.That(prodInfo.Key).IsEqualTo(20202);
			Check.That(prodInfo.Value).IsEqualTo("Test product");
		}

		[Test]
		public void NonExistingVendorInfoShouldReturnUnknown()
		{
			var vendorInfo = _vendorLooup.GetVendorandProductsFromIDs(-1, -1);
			Check.That(vendorInfo).IsNotNull();
			Check.That(vendorInfo.VendorName).IsEqualTo("Unknown");
			Check.That(vendorInfo.ProductDescription).IsEqualTo("Unknown");
			Check.That(vendorInfo.ProductDescription).IsEqualTo("Unknown");
		}

		[Test]
		public void ShouldReturnInformationOnBerndWithMixedCase()
		{
			var vendorInfo = _vendorLooup.GetVendorandProductsFromIDs(0x03dA, 0x0002);
			Check.That(vendorInfo).IsNotNull();
			Check.That(vendorInfo.VendorName).IsEqualTo("Bernd Walter Computer Technology");
			Check.That(vendorInfo.ProductDescription).IsEqualTo("HD44780 LCD interface");
		}

		[Test]
		public void ShouldReturnInformationOnEndPointsIntAndWebCam()
		{
			var vendorInfo = _vendorLooup.GetVendorandProductsFromIDs(0x03e8, 0x004);
			Check.That(vendorInfo).IsNotNull();
			Check.That(vendorInfo.VendorName).IsEqualTo("EndPoints, Inc.");
			Check.That(vendorInfo.ProductDescription).IsEqualTo("SE401 Webcam");
		}

		[Test]
		public void ShouldReturnInformationOnEndPointsIntAndWebCamWithUpperCase()
		{
			var vendorInfo = _vendorLooup.GetVendorandProductsFromIDs(0x03E8, 0x0004);
			Check.That(vendorInfo).IsNotNull();
			Check.That(vendorInfo.VendorName).IsEqualTo("EndPoints, Inc.");
			Check.That(vendorInfo.ProductDescription).IsEqualTo("SE401 Webcam");
		}

		[Test]
		public void ShouldReturnInformationOnFrysElectronicsAndUnknownForBadProductId()
		{
			var vendorInfo = _vendorLooup.GetVendorandProductsFromIDs(1, 0x142b);
			Check.That(vendorInfo).IsNotNull();
			Check.That(vendorInfo.VendorName).IsEqualTo("Fry's Electronics");
			Check.That(vendorInfo.ProductDescription).IsEqualTo("Unknown");
		}

		[Test]
		public void ShouldReturnInformationOnOculusVendorAndAlwaysUnknownProduct()
		{
			var vendorInfo = _vendorLooup.GetVendorandProductsFromIDs(10291, 0x0002);
			Check.That(vendorInfo).IsNotNull();
			Check.That(vendorInfo.VendorName).IsEqualTo("Oculus VR, Inc.");
			Check.That(vendorInfo.ProductDescription).IsEqualTo("Unknown");

			vendorInfo = _vendorLooup.GetVendorandProductsFromIDs(10291, 0x01010);
			Check.That(vendorInfo).IsNotNull();
			Check.That(vendorInfo.VendorName).IsEqualTo("Oculus VR, Inc.");
			Check.That(vendorInfo.ProductDescription).IsEqualTo("Unknown");
		}
	}
}