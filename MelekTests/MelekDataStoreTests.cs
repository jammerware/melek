using System;
using System.IO;
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
        private static readonly LoggingNinja _LoggingNinja = new LoggingNinja("C:\\Users\\Jammer\\AppData\\Roaming\\Jammerware.MtGBar.Test\\errors.log");
        private static MelekDataStore _TestStore;
        private static readonly string _TestStoreDirectory = "C:\\Users\\Jammer\\AppData\\Roaming\\Jammerware.MtGBar.Test";

        [ClassInitialize]
        public async static void Initialize(TestContext context)
        {
            try {
                Directory.CreateDirectory(_TestStoreDirectory);
                _TestStore = new MelekDataStore(_TestStoreDirectory, false, _LoggingNinja, true);

                await _TestStore.CheckForPackageUpdates();
                await _TestStore.ForceLoad();
            }
            catch (Exception ex) {
                throw new Exception("The initialization of the test store failed: " + ex.Message);
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if (Directory.Exists(_TestStoreDirectory)) {
                Directory.Delete(_TestStoreDirectory, true);
            }
        }

        //[TestMethod]
        //public void LoadsInActiveMelekFolder()
        //{
        //    try {
        //        Task.Run(async () => {
        //            MelekDataStore productionStore = new MelekDataStore("C:\\Users\\Jammer\\AppData\\Roaming\\Jammerware.MtGBar", true, _LoggingNinja, true);
        //            await productionStore.CheckForPackageUpdates();
        //            await productionStore.ForceLoad();
        //        }).GetAwaiter().GetResult();
        //    }
        //    catch (Exception e) {
        //        Assert.Fail(e.Message);
        //    }
        //}

        [TestMethod]
        public void SearchByColorWorks()
        {
            Card[] results = _TestStore.Search("c:u");
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
            Card[] results = _TestStore.Search("c:u r:mythic");
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void SearchByNameExactWorks()
        {
            Card[] results = _TestStore.Search("Melek, izzet");
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