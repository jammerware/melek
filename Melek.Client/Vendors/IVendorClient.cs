﻿using System.Threading.Tasks;
using Melek.Domain;

namespace Melek.Client.Vendors
{
    public interface IVendorClient : IInfoClient
    {
        Task<string> GetPrice(ICard<IPrinting> card, Set set);
    }
}