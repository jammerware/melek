using System;
using Melek.Domain;

namespace Melek.Api.Repositories
{
    public class CardRepository : ICardRepository
    {
        public ICard GetCardFromSlug(string slug)
        {
            //MelekDbContext ctx = new MelekDbContext();
            //return ctx.Cards.Where(c => Slugger.Slugify(c.Name).Equals(slug, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            throw new NotImplementedException();
        }
    }
}