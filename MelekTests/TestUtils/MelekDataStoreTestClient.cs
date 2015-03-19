using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melek.DataStore;
using Melek.Utilities;

namespace MelekTests.TestUtils
{
    public class MelekDataStoreTestClient
    {
        private string _TestDirectory;
        public MelekDataStore Store { get; private set; }

        public MelekDataStoreTestClient()
        {
            try {
                string _TestStoreDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Jammerware.MtGBar.Test");
                string errorLogFile = Path.Combine(_TestStoreDirectory, "errors.log");
                LoggingNinja loggingNinja = new LoggingNinja(errorLogFile);
                Directory.CreateDirectory(_TestStoreDirectory);

                Store = new MelekDataStore(_TestStoreDirectory, false, loggingNinja, true);
                Store.CheckForPackageUpdates().Wait();
            }
            catch (Exception ex) {
                throw new Exception("The initialization of the test store failed: " + ex.Message);
            }
        }

        public void Cleanup()
        {
            if (Directory.Exists(_TestDirectory)) {
                Directory.Delete(_TestDirectory, true);
            }
        }
    }
}