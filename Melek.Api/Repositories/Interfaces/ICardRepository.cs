using Melek.Domain;

namespace Melek.Api.Repositories
{
    public interface ICardRepository
    {
        ICard GetCardFromSlug(string slug);
    }
}