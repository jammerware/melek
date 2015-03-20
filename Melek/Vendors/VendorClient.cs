using Melek.Models;

namespace Melek.Vendors
{
    public abstract class VendorClient : InfoClient
    {
        public abstract string GetPrice(Card card, Set set);
    }
}