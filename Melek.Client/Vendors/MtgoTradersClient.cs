using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Http;
using Melek.Domain;

namespace Melek.Client.Vendors
{
    public class MtgoTradersClient : IVendorClient
    {
        public Task<string> GetLink(ICard<IPrinting> card, Set set)
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

        public async Task<string> GetPrice(ICard<IPrinting> card, Set set)
        {
            string pageHtml = await new NoobWebClient().DownloadString(await GetLink(card, set));
            Match match = Regex.Match(pageHtml, "<span class=\"price\">(\\S+)</span>");

            if (match != null) return match.Groups[1].Value;
            return null;
        }
    }
}