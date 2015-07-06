using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Http;
using Melek.Client.Models;

namespace Melek.Client.Vendors
{
    public class AmazonClient : IVendorClient
    {
        public Task<string> GetLink(Card card, Set set)
        {
            string cardName = card.Name.Replace("/", string.Empty).ToLower();
            return Task.Run(() => { return "http://www.amazon.com/s/field-keywords=mtg+" + WebUtility.UrlEncode(set.Name).ToLower() + "+" + WebUtility.UrlEncode(cardName); });
        }

        public string GetName()
        {
            return "Amazon.com";
        }

        public async Task<string> GetPrice(Card card, Set set)
        {
            string url = await GetLink(card, set);
            string html = await new NoobWebClient().DownloadString(url);

            Match match = Regex.Match(html, "<div id=\"atfResults\"[\\s\\S]+?(\\$[0-9]+\\.[0-9]{2})");
            if (match != null && match.Groups.Count == 2) {
                return match.Groups[1].Value;
            }

            return null;
        }
    }
}