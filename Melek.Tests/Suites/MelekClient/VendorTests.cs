using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Melek.Client.Vendors;
using NUnit.Framework;

namespace Melek.Tests.Suites
{
    [TestFixture]
    public class VendorTests
    {
        [Test]
        public void MtgoTradersGetLinkWorks()
        {
            string expected = "http://www.mtgotraders.com/store/AVR_Restoration_Angel.html";
            var vendorClient = new MtgoTradersClient();

            Assert.AreEqual(expected, vendorClient.GetLink(new Card() { Name = "Restoration Angel" }, new Set() { Code = "AVR" }));
        }

        [Test]
        public async Task MtgoTradersPriceIsReasonable()
        {
            string expectedRegex = @"^\$[0-9\.]+$";
            var vendorClient = new MtgoTradersClient();
            var price = await vendorClient.GetPrice(new Card() { Name = "Restoration Angel" }, new Set() { Code = "AVR" });
            Assert.IsTrue(Regex.IsMatch(price, expectedRegex));
        }

        [Test]
        public void MtgoTradersApostropheCardWorks()
        {
            string expected = "http://www.mtgotraders.com/store/M15_Ajanis_Pridemate.html";

            Assert.AreEqual(expected, new MtgoTradersClient().GetLink(new Card() { Name = "Ajani's Pridemate" }, new Set() { Code = "M15" }));
        }
    }
}