using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melek.Api.Repositories.Interfaces;
using Melek.Client.DataStore;
using Melek.Domain;
using Melek.Domain.Json;
using Newtonsoft.Json;

namespace Melek.Api.Repositories.Implementations
{
    public class MelekRepository : IMelekRepository
    {
        // TODO: figure out a better plan than setting this from Startup.cs via SetDataSource
        private MelekDataStore MelekDataStore { get; set; }

        public string GetAllData()
        {
            return JsonConvert.SerializeObject(MelekDataStore, MelekSerializationSettings.Get());
        }

        public ICard GetCardByMultiverseId(string multiverseId)
        {
            throw new NotImplementedException();
        }

        public ICard GetCardByName(string name)
        {
            return MelekDataStore.Cards.Where(c => c.Name.ToLower().Contains(name.ToLower())).FirstOrDefault();
        }

        public string GetVersion()
        {
            return MelekDataStore.Version;
        }

        public IReadOnlyList<Card> Search(string search)
        {
            throw new NotImplementedException();
        }

        public async Task SetDataSource(string path)
        {
            await Task.Factory.StartNew(() => {
                this.MelekDataStore = JsonConvert.DeserializeObject<MelekDataStore>(File.ReadAllText(path), MelekSerializationSettings.Get());
            });
        }
    }
}