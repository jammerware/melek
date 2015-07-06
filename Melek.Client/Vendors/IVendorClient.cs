using Melek.Client.Models;
using System.Threading.Tasks;

namespace Melek.Client.Vendors
{
    public interface IVendorClient : IInfoClient
    {
        Task<string> GetPrice(Card card, Set set);
    }
}