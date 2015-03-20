using System;
using System.Linq;
using Melek.DataStore;
using Melek.Models;
using MelekTests.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MelekTests
{
    [TestClass]
    public class CardTests
    {
        private MelekDataStoreTestClient _TestClient;

        [TestInitialize]
        public void Initialize()
        {
            _TestClient = new MelekDataStoreTestClient();
        }

        [TestMethod]
        public void GetLastPrintingWorks()
        {
            Card card = _TestClient.Store.Search("Lightning Bolt").First();
            CardPrinting lastPrinting = card.GetLastPrinting();

            Assert.IsTrue(lastPrinting.Set.Code.Equals("M11", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void GetLastPrintingPromoOnly()
        {
            Card card = _TestClient.Store.Search("Sakashima's Student").First();
            CardPrinting printing = card.GetLastPrinting();

            Assert.IsNotNull(printing);
        }

        [TestMethod]
        public void IsColorWorks()
        {
            Card card = _TestClient.Store.Search("Niv-Mizzet, Dracogenius").First();
            Assert.IsTrue(card.IsColor(MagicColor.U));
            Assert.IsTrue(card.IsColor(MagicColor.R));
        }

        [TestMethod]
        public void IsColorsWorks()
        {
            Card card = _TestClient.Store.Search("Niv-Mizzet, Dracogenius").First();
            Assert.IsTrue(card.IsColors(new MagicColor[] { MagicColor.U, MagicColor.R }));
        }

        [TestMethod]
        public void IsColorlessWorks()
        {
            Card card = _TestClient.Store.Search("Ugin, the Spirit Dragon").First();
            Assert.IsTrue(card.IsColorless);
        }

        [TestMethod]
        public void IsMulticoloredWorks()
        {
            Card card = _TestClient.Store.Search("Niv-Mizzet, Dracogenius").First();
            Assert.IsTrue(card.IsMulticolored);
        }
    }
}
