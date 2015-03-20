using Melek.Models;

namespace Melek.Vendors
{
    public abstract class InfoClient
    {
        public abstract string GetLink(Card card, Set set);
        public abstract string GetName();
    }
}
