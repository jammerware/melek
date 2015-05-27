using Melek.Models;
using Melek.Models.Cards;

namespace Melek.Vendors
{
    public abstract class InfoClient
    {
        public abstract string GetLink(Card card, Set set);
        public abstract string GetName();
    }
}
