using System;
using System.IO;
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
                UpdateCheckInterval = TimeSpan.FromMinutes(10)
            };
            client.UpdateCheckOccurred += () => { Console.WriteLine("Checking for update..."); };
            client.DataLoaded += () => {
                Console.WriteLine("Data loaded.");
                Console.WriteLine(client.Search("melek"));
            };
            
            Console.ReadLine();
        }
    }
}