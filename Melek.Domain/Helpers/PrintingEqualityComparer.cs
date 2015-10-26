using System.Collections.Generic;

namespace Melek.Domain
{
    public class PrintingEqualityComparer : IEqualityComparer<IPrinting>
    {
        public bool Equals(IPrinting x, IPrinting y)
        {
            return x.Set.Code == y.Set.Code;
        }

        public int GetHashCode(IPrinting obj)
        {
            return obj.Set.Code.GetHashCode();
        }
    }
}