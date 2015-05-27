using System.Web;
using Melek.Models;
using Melek.Models.Cards;

namespace Melek.Vendors
{
    public class MagicCardsInfoClient : InfoClient
    {
        public override string GetLink(Card card, Set set)
        {
            return string.Format("http://magiccards.info/query?q={0}&v=card&s=cname", HttpUtility.UrlEncode(card.Name));
        }

        public override string GetName()
        {
            return "MagicCards.info";
        }
    }
}