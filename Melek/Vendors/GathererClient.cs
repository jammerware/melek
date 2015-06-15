using System.Linq;
using Melek.Models;

namespace Melek.Vendors
{
    public class GathererClient : IInfoClient
    {
        public string GetLink(Card card, Set set)
        {
            PrintingBase printing = card.Printings.Where(p => p.Set.Code == set.Code).FirstOrDefault();
            if (printing != null) {
                return "http://gatherer.wizards.com/Pages/Card/Details.aspx?multiverseid=" + printing.MultiverseId;
            }
            return string.Empty;
        }

        public string GetName()
        {
            return "Gatherer";
        }
    }
}