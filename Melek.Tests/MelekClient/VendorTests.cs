using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melek.Tests.MelekClient
{
    [TestClass]
    public class VendorTests
    {
        // TODO: my job
        //[TestMethod]
        //public void MtgoTradersGetLinkWorks()
        //{
        //    string expected = "http://www.mtgotraders.com/store/AVR_Restoration_Angel.html";
        //    MtgoTradersClient vendorClient = new MtgoTradersClient();

        //    Assert.AreEqual(expected, vendorClient.GetLink(new Card() { Name = "Restoration Angel" }, new Set() { Code = "AVR" }));
        //}

        //[TestMethod]
        //public void MtgoTradersPriceIsReasonable()
        //{
        //    string expectedRegex = @"^\$[0-9\.]+$";
        //    MtgoTradersClient vendorClient = new MtgoTradersClient();
        //    string price = vendorClient.GetPrice(new Card() { Name = "Restoration Angel" }, new Set() { Code = "AVR" });
        //    Assert.IsTrue(Regex.IsMatch(price, expectedRegex));
        //}

        //[TestMethod]
        //public void MtgoTradersApostropheCardWorks()
        //{
        //    string expected = "http://www.mtgotraders.com/store/M15_Ajanis_Pridemate.html";

        //    Assert.AreEqual(expected, new MtgoTradersClient().GetLink(new Card() { Name = "Ajani's Pridemate" }, new Set() { Code = "M15" }));
        //}
    }
}