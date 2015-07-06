using System.Threading.Tasks;
using Melek.Client.Models;

namespace Melek.Client.Vendors
{
    public interface IInfoClient
    {
        Task<string> GetLink(Card card, Set set);
        string GetName();
    }
}