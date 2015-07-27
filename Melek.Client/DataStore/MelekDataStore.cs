using System.Collections.Generic;
using Melek.Client.Utilities;
using Melek.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Melek.Client.DataStore
{
    public class MelekDataStore
    {
        public IList<ICard> Cards { get; set; }
        public string ReleaseNotes { get; set; }
        public IList<Set> Sets { get; set; }
        public string Version { get; set; }

        public static JsonConverter[] GetRequiredConverters()
        {
            return new JsonConverter[] {
                new StringEnumConverter(),
                new CardJsonConverter(),
                new PrintingJsonConverter()
            };
        }
    }
}