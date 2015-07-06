﻿using System.Net;
using System.Threading.Tasks;
using Melek.Client.Models;

namespace Melek.Client.Vendors
{
    public class MagicCardsInfoClient : IInfoClient
    {
        public Task<string> GetLink(Card card, Set set)
        {
            return Task.Run(() => { return string.Format("http://magiccards.info/query?q={0}&v=card&s=cname", WebUtility.UrlEncode(card.Name)); });
        }

        public string GetName()
        {
            return "MagicCards.info";
        }
    }
}