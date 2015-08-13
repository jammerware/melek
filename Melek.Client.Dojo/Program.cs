using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Melek.Client.DataStore;
using Melek.Domain;
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
            Console.WriteLine("Loading data...");
            string searchTerm = "melek";

            MelekClient client = new MelekClient() {
                StoreCardImagesLocally = true,
                UpdateCheckInterval = TimeSpan.FromMinutes(10)
            };

            client.UpdateCheckOccurred += () => { Console.WriteLine("Checking for update..."); };
            client.DataLoaded += () => {
                Console.WriteLine("Data loaded.");

                IReadOnlyList<ICard> cards = client.GetCards();
                IOrderedEnumerable<ICard> nameStarts = cards.OrderBy(c => string.IsNullOrEmpty(searchTerm) || c.Name.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1);
                IOrderedEnumerable<ICard> nameStartsWith = cards.OrderBy(c => string.IsNullOrEmpty(searchTerm) || c.Nicknames.Count() > 0 && c.Nicknames.Any(n => n.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase)));
                IOrderedEnumerable<ICard> nicknamesIs = cards.OrderBy(c => string.IsNullOrEmpty(searchTerm) || c.Nicknames.Count() > 0 && c.Nicknames.Any(n => n.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase)));
                IOrderedEnumerable<ICard> nicknameStartsWith = cards.OrderBy(c => string.IsNullOrEmpty(searchTerm) || c.Nicknames.Count() > 0 && c.Nicknames.Any(n => n.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase)));
                IOrderedEnumerable<ICard> regexMatch = cards.OrderBy(c => string.IsNullOrEmpty(searchTerm) || Regex.IsMatch(c.Name, searchTerm, RegexOptions.IgnoreCase) ? 0 : 1);
                IOrderedEnumerable<ICard> nickNameRegexMatch = cards.OrderBy(c => string.IsNullOrEmpty(searchTerm) || c.Nicknames.Count() > 0 && c.Nicknames.Any(n => Regex.IsMatch(n, searchTerm, RegexOptions.IgnoreCase)) ? 0 : 1);
                IOrderedEnumerable<ICard> strictName = cards.OrderBy(c => c.Name);
                string things = "things";
            };

            Task t = client.LoadFromDirectory(Path.Combine(_ApplicationEnvironment.ApplicationBasePath, "storage"));

            Console.ReadLine();
        }
    }
}