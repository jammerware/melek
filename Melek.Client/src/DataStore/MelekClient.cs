﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bazam;
using Bazam.Extensions;
using Bazam.Http;
using Bazam.Modules;
using Bazam.Slugging;
using Bazam.Zipster;
using Melek.Client.Utilities;
using Melek.Json;
using Newtonsoft.Json;

namespace Melek.Client.DataStore
{
    public class MelekClient
    {
        #region static
        private const string API_ROOT = "http://melekapi.azurewebsites.net/";
        #endregion

        #region events
        public event DumbEventHandler DataLoaded;
        public event DumbEventHandler NewDataLoaded;
        public event DumbEventHandler UpdateCheckOccurred;
        #endregion

        #region fields
        private bool _IsLoaded = false;
        private MelekDataStore _MelekDataStore;
        private Timer _UpdateCheckTimer;
        #endregion

        #region private properties
        private string LocalDataPath
        {
            get { return Path.Combine(StorageDirectory, "melek-data-store.json"); }
        }
        #endregion

        #region private utility methods
        private async Task Load()
        {
            if (!File.Exists(LocalDataPath)) {
                await DownloadRemoteData();
            }

            await LoadLocalData();
            await UpdateFromRemote();
        }

        private async Task DownloadRemoteData()
        {
            // local path to which the zip will be saved
            string zipPath = Path.Combine(StorageDirectory, "melek-data-store.zip");

            // download the zip
            await new NoobWebClient().DownloadFile(API_ROOT + "api/melek/alldata", zipPath);

            // unzip
            var zipClient = new BazamZipClient();
            zipClient.Unzip(zipPath, StorageDirectory, true);
        }
        
        private async Task LoadLocalData()
        {
            _IsLoaded = false;

            _MelekDataStore = await Task.Factory.StartNew(() => { return JsonConvert.DeserializeObject<MelekDataStore>(File.ReadAllText(LocalDataPath), MelekSerializationSettings.Get()); });
            _IsLoaded = true;

            DataLoaded?.Invoke();
        }

        private async Task UpdateFromRemote()
        {
            string localVersionData = _MelekDataStore?.Version;
            string remoteVersionData = await new NoobWebClient().DownloadString(API_ROOT + "api/melek/version");
            Version localVersion = null;
            Version remoteVersion = null;

            Version.TryParse(localVersionData, out localVersion);
            Version.TryParse(remoteVersionData, out remoteVersion);
            
            if(_MelekDataStore == null || remoteVersion > localVersion) {
                await DownloadRemoteData();
                await LoadLocalData();

                NewDataLoaded?.Invoke();
            }

            UpdateCheckOccurred?.Invoke();
        }
        #endregion

        #region image management utility methods
        private async Task<Uri> ResolveCardImage(string multiverseId, Uri webUri)
        {
            return await Task.Run<Uri>(async () => {
                if (StoreCardImagesLocally) {
                    Uri localUri = new Uri(Path.Combine(CardImagesDirectory, Slugger.Slugify(multiverseId) + ".jpg"));
                    if (!File.Exists(localUri.LocalPath) || new FileInfo(localUri.LocalPath).Length == 0) {
                        NoobWebClient client = new NoobWebClient();
                        await client.DownloadFile(webUri.AbsoluteUri, localUri.LocalPath);
                    }
                    return localUri;
                }
                return webUri;
            });
        }
        #endregion

        #region update timer management
        private void StartUpdateTimer(TimeSpan interval)
        {
            if (interval == null) {
                _UpdateCheckTimer.Dispose();
            }
            else {
                int totalMilliseconds = (int)interval.TotalMilliseconds;

                if (_UpdateCheckTimer == null) {
                    _UpdateCheckTimer = new Timer(
                        async (object state) => {
                            await UpdateFromRemote();
                            _UpdateCheckTimer.Change(totalMilliseconds, Timeout.Infinite);
                        },
                        null,
                        totalMilliseconds,
                        Timeout.Infinite
                    );
                }
                else {
                    _UpdateCheckTimer.Change(totalMilliseconds, Timeout.Infinite);
                }
            }
        }
        #endregion

        #region public properties
        public string CardImagesDirectory
        {
            get { return Path.Combine(_StorageDirectory, "cards"); }
        }

        public string DataStoreVersion
        {
            get { return IsLoaded ? _MelekDataStore.Version : null; }
        }

        public bool IsLoaded
        {
            get { return _IsLoaded; }
        }
        
        private string _StorageDirectory;
        public string StorageDirectory
        {
            get { return _StorageDirectory; }
        }

        private bool _StoreCardImagesLocally = false;
        public bool StoreCardImagesLocally
        {
            get { return _StoreCardImagesLocally; }
            set
            {
                _StoreCardImagesLocally = value;
                if(!value) {
                    // assigned to a variable to suppress the whole "this is async" thing
                    // ... it'll probably be fine
                    Task task = ClearCardImageCache();
                }
            }
        }

        private TimeSpan _UpdateCheckInterval;
        public TimeSpan UpdateCheckInterval
        {
            get { return _UpdateCheckInterval; }
            set
            {
                _UpdateCheckInterval = value;
                StartUpdateTimer(value);
            }
        }
        #endregion

        #region public data access
        public async Task ClearCardImageCache()
        {
            if(!string.IsNullOrEmpty(StorageDirectory)) {
                await Task.Factory.StartNew(() => {
                    foreach (string fileName in Directory.GetFiles(CardImagesDirectory)) {
                        try {
                            File.Delete(fileName);
                        }
                        catch (Exception) {
                            // double hmm
                        }
                    }
                });
            }
        }
        
        public ICard GetCardByMultiverseId(string multiverseID)
        {
            return _MelekDataStore.Cards.Where(c => c.Printings.Where(p => p.MultiverseId == multiverseID).FirstOrDefault() != null).FirstOrDefault();
        }

        public ICard GetCardByPrinting(IPrinting printing)
        {
            return _MelekDataStore.Cards.Where(c => c.Printings.Contains(printing)).FirstOrDefault();
        }
        
        public Task<Uri> GetImageUri(IPrinting printing)
        {
            return GetImageUri(printing.MultiverseId);
        }

        public Task<Uri> GetImageUri(TransformPrinting printing, bool getFront)
        {
            return GetImageUri(getFront ? printing.MultiverseId : printing.TransformedMultiverseId);
        }

        private async Task<Uri> GetImageUri(string multiverseId)
        {
            if (Regex.IsMatch(multiverseId, "^[0-9]+$")) {
                // these are typical non-promo cards
                return await ResolveCardImage(multiverseId, new Uri(string.Format("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card", multiverseId)));
            }
            else {
                // promo cards we have to get from magiccards.info
                Match match = Regex.Match(multiverseId, @"([a-zA-Z0-9]+)_([a-zA-Z0-9]+)");
                if (match != null && match.Groups.Count == 3) {
                    return await ResolveCardImage(
                        multiverseId,
                        new Uri(
                            string.Format(
                                "http://magiccards.info/scans/en/{0}/{1}.jpg",
                                match.Groups[1].Value.ToLower(),
                                match.Groups[2].Value
                            )
                        )
                    );
                }
            }

            return null;
        }

        public async Task<string> GetCardImageCacheSize(bool estimate = false)
        {
            return await Task.Factory.StartNew(() => {
                double cardsDirectorySize = 0;

                if (estimate) {
                    cardsDirectorySize = (Directory.GetFiles(CardImagesDirectory).Count() * 34816); // a card is ABOUT 34kb on average
                    cardsDirectorySize = Math.Round(cardsDirectorySize);
                }
                else {
                    try {
                        foreach (string file in Directory.GetFiles(CardImagesDirectory)) {
                            cardsDirectorySize += new FileInfo(file).Length;
                        }
                    }
                    catch (Exception) {
                        // it's fine i guess, it's just an estimate for now
                    }
                }

                return (estimate ? "about " : string.Empty) + Math.Round(cardsDirectorySize / 1048576, 1).ToString() + " MB";
            });
        }

        public IReadOnlyList<ICard> GetCards()
        {
            return _MelekDataStore.Cards.ToArray();
        }

        public string GetRandomCardName()
        {
            if (_IsLoaded) {
                return _MelekDataStore.Cards.Random().Name;
            }
            return "Melek, Izzet Paragon"; // LOL
        }
        
        public IReadOnlyList<Set> GetSets()
        {
            return _MelekDataStore.Sets.ToArray();
        }

        public IReadOnlyList<ICard> Search(string searchTerm)
        {
            MatchCollection matches = Regex.Matches(searchTerm.Trim(), "([a-zA-Z0-9]{0,3})([:!])(\\S+)");
            
            if (matches.Count == 0) {
                return Search(searchTerm, null);
            }
            else {
                List<CardSearchDelegate> criteria = new List<CardSearchDelegate>();

                foreach (Match match in matches) {
                    string criteriaType = match.Groups[1].Value.ToLower();
                    string criteriaOperator = match.Groups[2].Value;
                    string criteriaValue = match.Groups[3].Value.ToLower();

                    switch (criteriaType) {
                        case "c":
                            bool requireMulticolored = false;
                            List<MagicColor> colors = new List<MagicColor>();
                            MagicColor tempColor = MagicColor.B;

                            foreach (char c in criteriaValue) {
                                if (c == 'm') requireMulticolored = true;
                                else if (EnuMaster.TryParse<MagicColor>(c.ToString(), out tempColor, true)) {
                                    colors.Add(tempColor);
                                }
                            }                            

                            if (criteriaOperator == "!")
                                criteria.Add((ICard card) => { return card.IsColors(colors) && (!requireMulticolored || card.IsMulticolored()); });
                            else if (criteriaOperator == ":")
                                criteria.Add((ICard card) => { return colors.Any(color => card.IsColor(color) || (requireMulticolored && card.IsMulticolored())); });
                            break;
                        case "mid":
                            criteria.Add((ICard card) => { return card.Printings.Any(p => p.MultiverseId.Equals(criteriaValue, StringComparison.CurrentCultureIgnoreCase)); });
                            break;
                        case "r":
                            CardRarity? rarity = StringToCardRarityConverter.GetRarity(criteriaValue);
                            
                            if (rarity != null) {
                                criteria.Add((ICard card) => { return card.Printings.Any(p => p.Rarity == rarity.Value); });
                            }

                            break;
                    }
                }

                return Search(null, criteria);
            }
        }

        private IReadOnlyList<ICard> Search(string nameTerm, List<CardSearchDelegate> filters)
        {
            if (_IsLoaded && (!string.IsNullOrEmpty(nameTerm) || (filters != null && filters.Count > 0))) {
                IEnumerable<ICard> cards = _MelekDataStore.Cards;
                string nameTermPattern = string.Empty;

                if (!string.IsNullOrEmpty(nameTerm)) {
                    Dictionary<string, string> replacementSequences = new Dictionary<string, string>() {
                        { "ae", "æ" },
                        { "u", "û" },
                        { "o", "ö" }
                    };
                    nameTermPattern = nameTerm.ToLower().Replace(" ", @"\s");
                    foreach (string replacementSequence in replacementSequences.Keys) {
                        nameTermPattern = nameTermPattern.Replace(replacementSequence, "[" + replacementSequence + "|" + replacementSequences[replacementSequence] + "]");
                    }

                    if (filters == null) filters = new List<CardSearchDelegate>();
                    filters.Add((ICard card) => { 
                        return 
                            Regex.IsMatch(card.Name, nameTermPattern, RegexOptions.IgnoreCase) ||
                            card.Nicknames.Any(n => Regex.IsMatch(n, nameTermPattern, RegexOptions.IgnoreCase)); 
                    });
                }

                foreach (CardSearchDelegate filter in filters) {
                    cards = cards.Where(c => filter(c as ICard));
                }

                return cards
                    .OrderBy(c => string.IsNullOrEmpty(nameTerm) || c.Name.StartsWith(nameTerm, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
                    .ThenBy(c => string.IsNullOrEmpty(nameTerm) || c.Nicknames.Count() > 0 && c.Nicknames.Any(n => n.StartsWith(nameTerm, StringComparison.CurrentCultureIgnoreCase)))
                    .ThenBy(c => string.IsNullOrEmpty(nameTerm) || Regex.IsMatch(c.Name, nameTermPattern, RegexOptions.IgnoreCase) ? 0 : 1)
                    .ThenBy(c => string.IsNullOrEmpty(nameTerm) || c.Nicknames.Count() > 0 && c.Nicknames.Any(n => Regex.IsMatch(n, nameTermPattern, RegexOptions.IgnoreCase)) ? 0 : 1)
                    .ThenBy(c => c.Name)
                    .ToArray();
            }

            return new ICard[] { };
        }
        #endregion

        #region public setup
        public async Task LoadFromDirectory(string directoryPath)
        {
            bool newValue = (_StorageDirectory != directoryPath);
            _StorageDirectory = directoryPath;

            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            if (!Directory.Exists(CardImagesDirectory)) {
                Directory.CreateDirectory(CardImagesDirectory);
            }

            if (newValue) {
                await Load();
            }
        }
        #endregion
    }
}