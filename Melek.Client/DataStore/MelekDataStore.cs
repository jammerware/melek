using System.Collections.Generic;
using Melek.Domain;

namespace Melek.Client.DataStore
{
    // this is the object that gets directly deserialized from the melek-data-store.json file from the melek API
    public class MelekDataStore
    {
        public IList<ICard> Cards { get; set; }
        public string ReleaseNotes { get; set; }
        public IList<Set> Sets { get; set; }
        public string Version { get; set; }
    }
}