using System.Threading.Tasks;
using Melek;

namespace Melek.Client.Vendors
{
    public interface IVendorClient : IInfoClient
    {
        Task<string> GetPrice(ICard card, Set set);
    }
}