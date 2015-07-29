using System;
using System.IO;
using Bazam.SharpZipLibHelpers;
using Melek.Client.DataStore;
using Microsoft.Framework.Runtime;

namespace Melek.Client.Dojo
{
    public class Program
    {
        private IApplicationEnvironment _ApplicationEnvironment;

        public Program(IApplicationEnvironment appEnvironment)
        {
            _ApplicationEnvironment = appEnvironment;
        }

        public void Main(string[] args)
        {
            MelekClient client = new MelekClient() {
                StorageDirectory = Path.Combine(_ApplicationEnvironment.ApplicationBasePath, "storage"),
                StoreCardImagesLocally = true,
                UpdateCheckInterval = TimeSpan.FromSeconds(10)
            };
            client.UpdateCheckOccurred += () => { Console.WriteLine("Checking for update..."); };
            
            Console.ReadLine();
        }
    }
}