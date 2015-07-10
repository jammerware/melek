using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Melek.Client.Models;
using Melek.Db;

namespace Melek.Api.Repositories
{
    public class CardRepository : ICardRepository
    {
        public Card GetCardFromSlug(string slug)
        {
            MelekDbContext ctx = new MelekDbContext();
            return ctx.Cards.Where(c => c.)
        }
    }
}