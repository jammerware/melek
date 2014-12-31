using System.Collections.Generic;
using Melek;

namespace Melek.Models.Helpers
{
    public class CardAppearanceEqualityComparer : IEqualityComparer<CardAppearance>
    {
        public bool Equals(CardAppearance x, CardAppearance y)
        {
            return x.Set.Code == y.Set.Code;
        }

        public int GetHashCode(CardAppearance obj)
        {
            return obj.Set.Code.GetHashCode();
        }
    }
}
