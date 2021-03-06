﻿using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Http;
using Melek;

namespace Melek.Client.Vendors
{
    public class ChannelFireballClient : IVendorClient
    {
        private string GetCFSetName(Set set)
        {
            return (set.CFName == null ? set.Name : set.CFName);
        }

        private string GetSearchLink(ICard card, Set set)
        {
            return "http://store.channelfireball.com/products/search?query=" + WebUtility.UrlEncode(card.Name + " " + GetCFSetName(set));
        }

        public async Task<string> GetLink(ICard card, Set set)
        {
            string searchLink = GetSearchLink(card, set);

            string searchHtml = await new NoobWebClient().DownloadString(searchLink);
            string searchPattern = string.Format("<a href=\"(\\S+?)\">\\s+<h3 class=\"hover-title\">{0}: {1}</h3>", GetCFSetName(set), card.Name);
            Match match = Regex.Match(searchHtml, searchPattern);

            if (match != null && match.Groups.Count == 2) {
                return "http://store.channelfireball.com" + match.Groups[1].Value;
            }

            return searchLink;
        }

        public string GetName()
        {
            return "ChannelFireball.com";
        }

        public async Task<string> GetPrice(ICard card, Set set)
        {
            string url = GetSearchLink(card, set);
            string html = string.Empty;
            string pattern = string.Format("<h3 class=\"grid-item-price\">(.+?)</h3>", (string.IsNullOrEmpty(set.CFName) ? set.Name : set.CFName), card.Name);
            
            html = await new NoobWebClient().DownloadString(url);
            Match match = Regex.Match(html, pattern);
            if (match != null && match.Groups.Count == 2) { return match.Groups[1].Value; }
            return null;
        }
    }
}