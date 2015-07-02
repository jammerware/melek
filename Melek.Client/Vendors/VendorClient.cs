using Melek.Models;

namespace Melek.Client.Vendors
{
    public interface IVendorClient : IInfoClient
    {
        string GetPrice(Card card, Set set);
    }
}