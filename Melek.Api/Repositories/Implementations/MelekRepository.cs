using System;
using System.Collections.Generic;
using System.Web;
using Melek.Api.Repositories.Interfaces;
using Melek.Client.DataStore;
using Melek.Domain;
using Newtonsoft.Json;

namespace Melek.Api.Repositories.Implementations
{
    public class MelekRepository : IMelekRepository
    {
        public static readonly MelekDataStore _MelekDataStore = JsonConvert.DeserializeObject<MelekDataStore>(HttpRuntime.AppDomainAppVirtualPath + "/Content/Data/melek-data-store.json");

        public string GetAllData()
        {
            return JsonConvert.SerializeObject(_MelekDataStore);
        }

        public ICard GetCardByMultiverseId(string multiverseId)
        {
            throw new NotImplementedException();
        }

        public ICard GetCardByName(string name)
        {
            throw new NotImplementedException();
        }

        public string GetVersion()
        {
            return _MelekDataStore.Version;
        }

        public IReadOnlyList<Card> Search(string search)
        {
            throw new NotImplementedException();
        }
    }
}