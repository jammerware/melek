using Melek.Models;
using Melek.Models.Cards;

namespace Melek.Vendors
{
    public abstract class VendorClient : InfoClient
    {
        public abstract string GetPrice(Card card, Set set);
    }
}