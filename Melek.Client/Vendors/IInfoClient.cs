using System.Threading.Tasks;
using Melek;

namespace Melek.Client.Vendors
{
    public interface IInfoClient
    {
        Task<string> GetLink(ICard card, Set set);
        string GetName();
    }
}