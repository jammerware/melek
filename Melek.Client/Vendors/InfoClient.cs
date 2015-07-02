using Melek.Models;

namespace Melek.Client.Vendors
{
    public interface IInfoClient
    {
        string GetLink(Card card, Set set);
        string GetName();
    }
}