using Melek.Models;

namespace Melek.Vendors
{
    public interface IVendorClient : IInfoClient
    {
        string GetPrice(Card card, Set set);
    }
}