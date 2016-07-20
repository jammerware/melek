using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Http;
using Bazam.Slugging;
using Melek;

namespace Melek.Client.Vendors
{
    public class TcgPlayerClient : IVendorClient
    {
        private async Task<string> GetAPIData(ICard card, Set set)
        {
            //http://partner.tcgplayer.com/x3/pv.asmx/p?pk=MTGBAR&p=Sword+of+War+and+Peace&s=New+Phyrexia&v=3
            // resolve their set name - sometimes it's special
            string setName = (set.TCGPlayerName == null ? set.Name : set.TCGPlayerName);
            return await new NoobWebClient().DownloadString(string.Format("http://partner.tcgplayer.com/x3/pv.asmx/p?pk=MTGBAR&p={0}&s={1}&v=3", WebUtility.UrlEncode(card.Name), WebUtility.UrlEncode(setName)));
        }

        public async Task<string> GetLink(ICard card, Set set)
        {
            string apiData = await GetAPIData(card, set);
            MatchCollection matches = Regex.Matches(apiData, "<link>([\\s\\S]+?)</link>");
            if (matches.Count > 0) {
                return matches[0].Groups[1].Value.Trim();
            }
            return string.Format("http://store.tcgplayer.com/magic/{0}/{1}", Slugger.Slugify(string.IsNullOrEmpty(set.TCGPlayerName) ? set.Name : set.TCGPlayerName), Slugger.Slugify(card.Name));
        }

        public string GetName()
        {
            return "TCGPlayer.com";
        }

        public async Task<string> GetPrice(ICard card, Set set)
        {
            string apiData = await GetAPIData(card, set);
            MatchCollection matches = Regex.Matches(apiData, "<price>([0-9]*?\\.[0-9]{2})</price>");
            if (matches.Count > 0) {
                return "$" + matches[0].Groups[1].Value.Trim();
            }
            return string.Empty;
        }
    }
}