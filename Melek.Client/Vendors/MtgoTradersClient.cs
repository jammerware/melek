using System.Net;
using System.Text.RegularExpressions;
using Melek.Client.Models;
using Bazam.Http;
using System.Threading.Tasks;

namespace Melek.Client.Vendors
{
    public class MtgoTradersClient : IVendorClient
    {
        public Task<string> GetLink(Card card, Set set)
        {
            string sterilizedCardName = Regex.Replace(card.Name, "[',]", string.Empty);
            sterilizedCardName = sterilizedCardName.Replace(' ', '_');
            string setCode = set.Code;

            if (set.IsPromo) {
                setCode = "PRM";
            }

            return Task.Run(() => {
                return string.Format("http://www.mtgotraders.com/store/{0}_{1}.html", set.Code, sterilizedCardName);
            });
        }

        public string GetName()
        {
            return "MtgoTraders.com";
        }

        public async Task<string> GetPrice(Card card, Set set)
        {
            string pageHtml = await new NoobWebClient().DownloadString(await GetLink(card, set));
            Match match = Regex.Match(pageHtml, "<span class=\"price\">(\\S+)</span>");

            if (match != null) return match.Groups[1].Value;
            return null;
        }
    }
}