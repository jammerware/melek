using System;
using System.Threading.Tasks;
using Melek.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MelekTests
{
    [TestClass]
    public class MelekDataStoreTests
    {
        [TestMethod]
        public void LoadsInActiveMelekFolder()
        {
            try {
                LoggingNinja logger = new LoggingNinja("errors.log");
                MelekDataStore dataStore = new MelekDataStore("C:\\Users\\bstein\\AppData\\Roaming\\Jammerware.MtGBar", true, logger, true, "iStHEdEVaUTHhAPPENING4621");
                Task.Run(async () => {
                    dataStore.CheckForPackageUpdates();
                    await dataStore.ForceLoad(); 
                }).GetAwaiter().GetResult();
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
    }
}