using System.Threading.Tasks;
using Melek.Domain;

namespace Melek.Client.Vendors
{
    public interface IInfoClient
    {
        Task<string> GetLink(ICard<IPrinting> card, Set set);
        string GetName();
    }
}