using System.Linq;
using Melek.Models;

namespace Melek.Vendors
{
    public class GathererClient : InfoClient
    {
        public override string GetLink(Card card, Set set)
        {
            PrintingBase printing = card.Printings.Where(p => p.Set.Code == set.Code).FirstOrDefault();
            if (printing != null) {
                return "http://gatherer.wizards.com/Pages/Card/Details.aspx?multiverseid=" + printing.MultiverseID;
            }
            return string.Empty;
        }

        public override string GetName()
        {
            return "Gatherer";
        }
    }
}