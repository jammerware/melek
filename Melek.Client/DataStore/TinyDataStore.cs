using System.Collections.Generic;
using Melek.Domain;

namespace Melek.Client.DataStore
{
    public class TinyDataStore
    {
        public IList<ICard> Cards { get; set; }
        public IList<Set> Sets { get; set; }
    }
}