using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Melek.Models;

namespace Melek.Vendors
{
    public class AmazonClient : VendorClient
    {
        public override string GetLink(Card card, Set set)
        {
            string cardName = card.Name.Replace("/", string.Empty).ToLower();
            return "http://www.amazon.com/s/field-keywords=mtg+" + HttpUtility.UrlEncode(set.Name).ToLower() + "+" + HttpUtility.UrlEncode(cardName);
        }

        public override string GetName()
        {
            return "Amazon.com";
        }

        public override string GetPrice(Card card, Set set)
        {
            string url = GetLink(card, set);
            string html = new WebClient().DownloadString(url);

            Match match = Regex.Match(html, "<div id=\"atfResults\"[\\s\\S]+?(\\$[0-9]+\\.[0-9]{2})");
            if (match != null && match.Groups.Count == 2) {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
    }
}
