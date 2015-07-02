using System.Collections.Generic;
using Melek;

namespace Melek.Client.Models.Helpers
{
    public class CardPrintingEqualityComparer : IEqualityComparer<Printing>
    {
        public bool Equals(Printing x, Printing y)
        {
            return x.Set.Code == y.Set.Code;
        }

        public int GetHashCode(Printing obj)
        {
            return obj.Set.Code.GetHashCode();
        }
    }
}
