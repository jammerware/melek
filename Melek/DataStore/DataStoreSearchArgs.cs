using System.Collections.Generic;
using Melek.Models;

namespace Melek.DataStore
{
    public class DataStoreSearchArgs
    {
        public IList<MagicColor> Colors { get; set; }
        public string Name { get; set; }
        public IList<CardRarity> Rarities { get; set; }
        public string SetCode { get; set; }

        public bool HasArguments()
        {
            return 
                (Colors != null && Colors.Count > 0) || 
                !string.IsNullOrEmpty(Name) ||
                (Rarities != null && Rarities.Count > 0) ||
                !string.IsNullOrEmpty(SetCode);
        }
    }
}