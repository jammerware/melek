﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melek.DataStore;
using Melek.Models;
using Melek.Utilities;
using Melek.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MelekTests
{
    [TestClass]
    public class MelekDataStoreTests
    {
        private static MelekDataStoreTestClient _TestClient;
        private static string _TestStoreDirectory = string.Empty;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _TestClient = new MelekDataStoreTestClient();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _TestClient.Cleanup();
        }

        [TestMethod]
        public void GetPromoCardUriWorks()
        {
            Uri uri = _TestClient.Store.GetCardImageUri(new CardPrinting() {
                MultiverseID = "WMCQ_2",
                Set = new Set() { IsPromo = true }
            }).GetAwaiter().GetResult();

            Assert.AreEqual(new Uri("http://magiccards.info/scans/en/wmcq/2.jpg").ToString(), uri.ToString());
        }

        [TestMethod]
        public void LoadsInActiveMelekFolder()
        {
            try {
                Task.Run(async () => {
                    string productionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Jammerware.MtGBar.Test");
                    LoggingNinja productionNinja = new LoggingNinja(Path.Combine(productionPath, "errors.log"));
                    MelekDataStore productionStore = new MelekDataStore(productionPath, true, productionNinja, true);

                    await productionStore.CheckForPackageUpdates();
                    await productionStore.ForceLoad();
                }).GetAwaiter().GetResult();
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void RulingsAreLoading()
        {
            Card sledge = _TestClient.Store.Search("Behemoth Sledge").FirstOrDefault();

            Assert.IsTrue(sledge.Rulings.Count() > 0);
        }


        [TestMethod]
        public void SearchByColorWorks()
        {
            Card[] results = _TestClient.Store.Search("c:u");
            if (results.Length == 0) {
                Assert.Fail("The datastore didn't return any results for a color-based search. There are totally blue cards in magic. Just saying.");
            }
            foreach (Card card in results) {
                if (!card.Cost.ToString().ToLower().Contains("u")) {
                    Assert.Fail("One or more of the results returned from a color-based search didn't have the right color.");
                }
            }
        }

        [TestMethod]
        public void SearchByMultipleWorks()
        {
            Card[] results = _TestClient.Store.Search("c:u r:mythic");

            Assert.IsTrue(results.Length > 0, "A search for blue mythics didn't return any cards.");
            Assert.IsTrue(results.All(c => c.IsColor(MagicColor.U) && c.Printings.Any(p => p.Rarity == CardRarity.MythicRare)), "A search for blue mythics returned something that wasn't blue or wasn't mythic or both.");
        }

        [TestMethod]
        public void SearchByMultiverseIDWorks()
        {
            Card[] results = _TestClient.Store.Search("mid:369062");

            Assert.IsTrue(results.Length == 1, "A search by multiverseID failed.");
            Assert.IsTrue(results[0].Name == "Melek, Izzet Paragon");
        }

        [TestMethod]
        public void SearchByNameExactWorks()
        {
            Card[] results = _TestClient.Store.Search("Melek, izzet");
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

        [TestMethod]
        public void SearchByNameWithAEWorks()
        {
            Card[] results = _TestClient.Store.Search("aetherize");

            Assert.IsTrue(results.Length > 0);
            Assert.IsTrue(results[0].Name == "Ætherize", "Searching for a card with an Æ in its name failed.");
        }

        [TestMethod]
        public void SearchByNameWithFunnyCharWorks()
        {
            Card[] results = _TestClient.Store.Search("lim-dul the necromancer");

            Assert.IsTrue(results.Length == 1);
            Assert.IsTrue(results[0].Name == "Lim-Dûl the Necromancer", "Searching a card with funny characters in its name failed. (Expected \"Lim-Dûl the Necromancer\")");
        }

        [TestMethod]
        public void SearchByNameWithANormalVersionOfAFunnyChar()
        {
            Card[] results = _TestClient.Store.Search("nuckla");
            Assert.IsTrue(results.Length == 1);
            Assert.IsTrue(results[0].Name == "Nucklavee", "Searching for a card with a normal version of a funny character (like how \"Nucklavee\" contains \"u\" which is replaced with \"û\" in \"Lim-Dûl the Necromancer\") failed.");
        }

        [TestMethod]
        public void SearchByNicknameExactWorks()
        {
            Card[] results = _TestClient.Store.Search("sad robot");
            
            Assert.IsTrue(results.Length == 1, "Searching for a card by exact nickname returned the wrong number of results (should be 1).");
            Assert.IsTrue(results[0].Name == "Solemn Simulacrum", "Searching for a card by exact nickname found the wrong card.");
        }

        [TestMethod]
        public void SearchByRarityWorks()
        {
            Card[] results = _TestClient.Store.Search("r:uncommon");

            Assert.IsTrue(results.Length > 0, "A search by rarity returned no results.");
            Assert.IsTrue(results.All(c => c.Printings.Any(p => p.Rarity == CardRarity.Uncommon)), "A search by rarity returned a card that doesn't have a printing of appropriate rarity.");
        }

        [TestMethod]
        public void SearchResultSortWorks()
        {
            Card[] results = _TestClient.Store.Search("r");

            Assert.IsTrue(results.First().Name.StartsWith("R"));
        }
    }
}