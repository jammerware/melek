using System.Collections.Generic;
using Melek;

namespace Melek.Models.Helpers
{
    public class CardPrintingEqualityComparer : IEqualityComparer<CardPrinting>
    {
        public bool Equals(CardPrinting x, CardPrinting y)
        {
            return x.Set.Code == y.Set.Code;
        }

        public int GetHashCode(CardPrinting obj)
        {
            return obj.Set.Code.GetHashCode();
        }
    }
}
