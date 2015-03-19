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
    }
}
