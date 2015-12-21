using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melek.Client.DataStore;
using Microsoft.Extensions.PlatformAbstractions;

namespace Melek.Client.Dojo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Loading data...");

            MelekClient client = new MelekClient() {
                StoreCardImagesLocally = false,
                UpdateCheckInterval = TimeSpan.FromMinutes(10)
            };

            client.UpdateCheckOccurred += () => { Console.WriteLine("Checking for update..."); };
            client.DataLoaded += () => {
                Console.WriteLine("Data loaded.");
                Console.WriteLine(client.GetRandomCardName());
            };
            
            Task t = client.LoadFromDirectory(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "storage"));

            while (true) {
                string input = Console.ReadLine();
                if (input == "exit") break;

                ICard card = client.Search(input).First();
                Console.WriteLine(client.GetImageUri(card.GetLastPrinting()).GetAwaiter().GetResult().AbsoluteUri);
            };
        }
    }
}