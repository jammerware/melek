using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melek.Client.DataStore;
using Microsoft.Extensions.PlatformAbstractions;

namespace Melek.DebugConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var appEnv = PlatformServices.Default.Application;
            Console.WriteLine("Loading data...");

            var client = new MelekClient()
            {
                StoreCardImagesLocally = false,
                UpdateCheckInterval = TimeSpan.FromMinutes(10)
            };

            client.UpdateCheckOccurred += () => { Console.WriteLine("Checking for update..."); };
            client.DataLoaded += () => {
                Console.WriteLine("Data loaded.");
                Console.WriteLine(client.GetRandomCardName());
            };

            Task t = client.LoadFromDirectory(Path.Combine(appEnv.ApplicationBasePath, "storage"));

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "exit") break;

                ICard card = client.Search(input).First();
                Console.WriteLine(client.GetImageUri(card.GetLastPrinting()).GetAwaiter().GetResult().AbsoluteUri);
            };
        }
    }
}