using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nivix.Models;

namespace Nivix.Infrastructure
{
    public static class DataBeast
    {
        private static string NICKNAMES_FILE = "Data/nicknames.json";
        private static string SETS_FILE = "Data/sets.json";

        public static IList<CardNickname> GetCardNicknames()
        {
            string jsonData = File.ReadAllText(NICKNAMES_FILE);
            JObject jObject = JObject.Parse(jsonData);
            return JsonConvert.DeserializeObject<IList<CardNickname>>(jObject["Cards"].ToString());
        }

        public static IList<SetData> GetSetData()
        {
            string jsonData = File.ReadAllText(SETS_FILE);
            JObject jObject = JObject.Parse(jsonData);
            return JsonConvert.DeserializeObject<IList<SetData>>(jObject["Sets"].ToString()).OrderBy(s => s.Name).ToList();
        }

        public static void SaveCardNicknames(IList<CardNickname> data)
        {
            dynamic dataWithRoot = new { Cards = data };
            string jsonData = JsonConvert.SerializeObject(dataWithRoot, Formatting.Indented);
            File.WriteAllText(NICKNAMES_FILE ,jsonData);
        }
        
        public static void SaveSetData(IList<SetData> data)
        {
            dynamic dataWithRoot = new { Sets = data };
            string jsonData = JsonConvert.SerializeObject(dataWithRoot, Formatting.Indented);
            File.WriteAllText(SETS_FILE, jsonData);
        }
    }
}