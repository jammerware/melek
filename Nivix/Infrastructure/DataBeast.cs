using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nivix.Models;

namespace Nivix.Infrastructure
{
    public static class DataBeast
    {
        public static IList<CardNickname> GetCardNicknames()
        {
            string jsonData = File.ReadAllText("Data/nicknames.json");
            JObject jObject = JObject.Parse(jsonData);
            return JsonConvert.DeserializeObject<IList<CardNickname>>(jObject["Cards"].ToString());
        }

        public static IList<SetData> GetSetData()
        {
            string jsonData = File.ReadAllText("Data/sets.json");
            JObject jObject = JObject.Parse(jsonData);
            return JsonConvert.DeserializeObject<IList<SetData>>(jObject["Sets"].ToString());
        }
    }
}