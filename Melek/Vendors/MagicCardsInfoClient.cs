using System.Web;
using Melek.Models;

namespace Melek.Vendors
{
    public class MagicCardsInfoClient : IInfoClient
    {
        public string GetLink(Card card, Set set)
        {
            return string.Format("http://magiccards.info/query?q={0}&v=card&s=cname", HttpUtility.UrlEncode(card.Name));
        }

        public string GetName()
        {
            return "MagicCards.info";
        }
    }
}