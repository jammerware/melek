using Melek.Models;

namespace Melek.Vendors
{
    public interface IInfoClient
    {
        string GetLink(Card card, Set set);
        string GetName();
    }
}