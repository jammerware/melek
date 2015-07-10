using Melek.Client.Models;

namespace Melek.Api.Repositories
{
    public interface ICardRepository
    {
        public Card GetCardFromSlug(string slug);
    }
}