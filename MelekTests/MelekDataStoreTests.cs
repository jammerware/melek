using System;
using System.Threading.Tasks;
using Melek.DataStore;
using Melek.Models;
using Melek.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MelekTests
{
    [TestClass]
    public class MelekDataStoreTests
    {
        private static readonly LoggingNinja _LoggingNinja = new LoggingNinja("errors.log");
        private static readonly MelekDataStore _ProductionStore = new MelekDataStore("C:\\Users\\bstein\\AppData\\Roaming\\Jammerware.MtGBar", true, _LoggingNinja, true);

        [TestInitialize]
        public void Initialize()
        {
            Task.Run(async () => {
                await _ProductionStore.ForceLoad();
            });
        }

        [TestMethod]
        public void LoadsInActiveMelekFolder()
        {
            try {
                Task.Run(async () => {
                    _ProductionStore.CheckForPackageUpdates();
                    await _ProductionStore.ForceLoad();
                }).GetAwaiter().GetResult();
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void SearchByColorWorks()
        {
            Card[] results = _ProductionStore.Search("c:u");
            if (results.Length == 0) {
                Assert.Fail("The datastore didn't return any results for a color-based search.");
            }
            foreach (Card card in results) {
                if (!card.Cost.ToString().Contains("u")) {
                    Assert.Fail("One or more of the results returned from a color-based search didn't have the right color.");
                }
            }
        }

        [TestMethod]
        public void SearchByMultipleWorks()
        {
            Card[] results = _ProductionStore.Search("c:u r:mythic");
        }

        [TestMethod]
        public void SearchByNameExactWorks()
        {
            Card[] results = _ProductionStore.Search("Melek, izzet");
            if (results.Length > 1) {
                Assert.Fail("An exact name search returned too many results.");
            }
            else if (results.Length == 0) {
                Assert.Fail("An exact name search didn't find its card.");
            }
            else if (results[0].Name != "Melek, Izzet Paragon") {
                Assert.Fail("An exact name search found the wrong card.");
            }
        }
    }
}