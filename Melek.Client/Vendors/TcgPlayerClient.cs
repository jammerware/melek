using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Bazam.Slugging;
using Melek.Models;

namespace Melek.Client.Vendors
{
    public class TcgPlayerClient : IVendorClient
    {
        private string GetAPIData(Card card, Set set)
        {
            //http://partner.tcgplayer.com/x3/pv.asmx/p?pk=MTGBAR&p=Sword+of+War+and+Peace&s=New+Phyrexia&v=3
            // resolve their set name - sometimes it's special
            string setName = (set.TCGPlayerName == string.Empty ? set.Name : set.TCGPlayerName);
            using (WebClient client = new WebClient()) {
                return client.DownloadString(string.Format("http://partner.tcgplayer.com/x3/pv.asmx/p?pk=MTGBAR&p={0}&s={1}&v=3", HttpUtility.UrlEncode(card.Name), HttpUtility.UrlEncode(setName)));
            }
        }

        public string GetLink(Card card, Set set)
        {
            string apiData = GetAPIData(card, set);
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

        public string GetPrice(Card card, Set set)
        {
            string apiData = GetAPIData(card, set);
            MatchCollection matches = Regex.Matches(apiData, "<price>([0-9]*?\\.[0-9]{2})</price>");
            if (matches.Count > 0) {
                return "$" + matches[0].Groups[1].Value.Trim();
            }
            return string.Empty;
        }
    }
}
