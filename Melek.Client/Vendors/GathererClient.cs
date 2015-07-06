using System.Linq;
using System.Threading.Tasks;
using Melek.Client.Models;

namespace Melek.Client.Vendors
{
    public class GathererClient : IInfoClient
    {
        public Task<string> GetLink(Card card, Set set)
        {
            return Task.Run(() => {
                PrintingBase printing = card.Printings.Where(p => p.Set.Code == set.Code).FirstOrDefault();
                if (printing != null) {
                    return "http://gatherer.wizards.com/Pages/Card/Details.aspx?multiverseid=" + printing.MultiverseId;
                }
                return null;
            });
        }

        public string GetName()
        {
            return "Gatherer";
        }
    }
}