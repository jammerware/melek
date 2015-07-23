using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Http;
using Bazam.Modules;
using Bazam.Slugging;
using Melek.Domain;

namespace Melek.Client.DataStore
{
    public class MelekClient
    {
        #region Events
        public event EventHandler DataLoaded;
        #endregion

        #region Fields
        private ICard<IPrinting>[] _Cards;
        private bool _IsLoaded = false;
        private Set[] _Sets;
        private bool _SaveCardImages;
        private string _StorageDirectory;
        #endregion

        #region Constructors
        // general use constructors
        public MelekClient(string storageDirectory, bool storeCardImages) : this(storageDirectory, storeCardImages, false) { }
        
        // for original dev only
        public MelekClient(string storageDirectory, bool storeCardImages, bool lazyLoad)
        {
            _SaveCardImages = storeCardImages;
            _StorageDirectory = storageDirectory;

            if (!Directory.Exists(storageDirectory)) {
                Directory.CreateDirectory(storageDirectory);
            }

            if (!Directory.Exists(CardImagesDirectory) && storeCardImages) {
                Directory.CreateDirectory(CardImagesDirectory);
            }
            
            if (!lazyLoad) {
                Action lolz = async () => {
                    await ForceLoad();
                };

                lolz.BeginInvoke(null, null);
            }
        }
        #endregion

        #region internal utility properties
        public string CardImagesDirectory
        {
            get { return Path.Combine(_StorageDirectory, "cards"); }
        }
        
        public string StorageDirectory
        {
            get { return _StorageDirectory; }
        }
        #endregion

        #region data management utility methods
        private void DoLoad()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Forces this MelekDataStore to reload its data from the data available on the file system. Note that unless specifically argued
        /// in the constructor, the data store will do this asynchronously upon instantiation - the only reason to call this manually is
        /// if you want to control when the data store initially loads its data, and realistically should only be called externally once per
        /// instance.
        /// </summary>
        public async Task ForceLoad()
        {
            await Task.Run(() => { DoLoad(); });
        }
        #endregion

        #region Image management utility methods
        private async Task<Uri> ResolveCardImage(IPrinting printing, Uri webUri)
        {
            Uri retVal = await Task.Run<Uri>(async () => {
                Uri localUri = new Uri(Path.Combine(CardImagesDirectory, Slugger.Slugify(printing.MultiverseId) + ".jpg"));

                if (_SaveCardImages) {
                    if (!File.Exists(localUri.LocalPath) || new FileInfo(localUri.LocalPath).Length == 0) {
                        NoobWebClient client = new NoobWebClient();
                        await client.DownloadFile(webUri.AbsoluteUri, localUri.LocalPath);
                    }
                    return localUri;
                }
                return webUri;
            });

            return retVal;
        }
        #endregion

        #region public properties
        public bool StoreCardImagesLocally { get; set; }
        #endregion

        #region public data access
        public async Task ClearCardImageCache()
        {
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
        
        public ICard<IPrinting> GetCardByMultiverseId(string multiverseID)
        {
            return _Cards.Where(c => c.Printings.Where(p => p.MultiverseId == multiverseID).FirstOrDefault() != null).FirstOrDefault();
        }

        public ICard<IPrinting> GetCardByPrinting(IPrinting printing)
        {
            return _Cards.Where(c => c.Printings.Contains(printing)).FirstOrDefault();
        }

        public async Task<Uri> GetCardImageUri(IPrinting printing)
        {
            if (Regex.IsMatch(printing.MultiverseId, "^[0-9]+$")) {
                // these are typical non-promo cards
                return await ResolveCardImage(printing, new Uri(string.Format("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card", printing.MultiverseId)));
            }
            else {
                // promo cards we have to get from magiccards.info
                Match match = Regex.Match(printing.MultiverseId, @"([a-zA-Z0-9]+)_([a-zA-Z0-9]+)");
                if (match != null && match.Groups.Count == 3) {
                    return await ResolveCardImage(
                        printing,
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

        public string GetCardImageCacheSize(bool estimate = false)
        {
            double cardsDirectorySize = 0;

            if (estimate) {
                cardsDirectorySize = (Directory.GetFiles(CardImagesDirectory).Count() * 34816); // a card is ABOUT 34kb
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

            return (estimate ? "about " : string.Empty) + Math.Round(cardsDirectorySize / 1024 / 1024, 1).ToString() + " MB";
        }

        public ICard[] GetCards()
        {
            return _Cards;
        }

        public string GetRandomCardName()
        {
            if (_IsLoaded) {
                return _Cards[new Random().Next(_Cards.Length - 1)].Name;
            }
            return "Melek, Izzet Paragon"; // LOL
        }
        
        public Set[] GetSets()
        {
            return _Sets;
        }

        public ICard[] Search(string searchTerm)
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
                                criteria.Add((ICard<IPrinting> card) => { return card.IsColors(colors) && (!requireMulticolored || card.IsMulticolored()); });
                            else if (criteriaOperator == ":")
                                criteria.Add((ICard<IPrinting> card) => { return colors.Any(color => card.IsColor(color) || (requireMulticolored && card.IsMulticolored())); });
                            break;
                        case "mid":
                            criteria.Add((ICard<IPrinting> card) => { return card.Printings.Any(p => p.MultiverseId.Equals(criteriaValue, StringComparison.CurrentCultureIgnoreCase)); });
                            break;
                        case "r":
                            CardRarity? rarity = null;
                            switch (criteriaValue) {
                                case "common":
                                case "c":
                                    rarity = CardRarity.Common;
                                    break;
                                case "uncommon":
                                case "u":
                                    rarity = CardRarity.Uncommon;
                                    break;
                                case "rare":
                                case "r":
                                    rarity = CardRarity.Rare;
                                    break;
                                case "mythic":
                                case "m":
                                    rarity = CardRarity.MythicRare;
                                    break;
                            }

                            if (rarity != null) {
                                criteria.Add((ICard<IPrinting> card) => { return card.Printings.Any(p => p.Rarity == rarity.Value); });
                            }

                            break;
                        default:
                            break;
                    }
                }

                return Search(null, criteria);
            }
        }

        private ICard[] Search(string nameTerm, List<CardSearchDelegate> filters)
        {
            if (_IsLoaded && (!string.IsNullOrEmpty(nameTerm) || (filters != null && filters.Count > 0))) {
                IEnumerable<ICard> cards = _Cards;
                string nameTermPattern = string.Empty;

                if (!string.IsNullOrEmpty(nameTerm)) {
                    Dictionary<string, string> replacementSequences = new Dictionary<string, string>() {
                        { "ae", "æ" },
                        { "u", "û" }
                    };
                    nameTermPattern = nameTerm.ToLower().Replace(" ", @"\s");
                    foreach (string replacementSequence in replacementSequences.Keys) {
                        nameTermPattern = nameTermPattern.Replace(replacementSequence, "[" + replacementSequence + replacementSequences[replacementSequence] + "]");
                    }

                    if (filters == null) filters = new List<CardSearchDelegate>();
                    filters.Add((ICard<IPrinting> card) => { 
                        return 
                            Regex.IsMatch(card.Name, nameTermPattern, RegexOptions.IgnoreCase) ||
                            card.Nicknames.Any(n => Regex.IsMatch(n, nameTermPattern, RegexOptions.IgnoreCase)); 
                    });
                }

                foreach (CardSearchDelegate filter in filters) {
                    cards = cards.Where(c => filter(c as ICard<IPrinting>));
                }

                return cards
                    .OrderBy(c => string.IsNullOrEmpty(nameTerm) || c.Name.StartsWith(nameTerm, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
                    .ThenBy(c => string.IsNullOrEmpty(nameTerm) || Regex.IsMatch(c.Name, nameTermPattern, RegexOptions.IgnoreCase) ? 0 : 1)
                    .ThenBy(c => string.IsNullOrEmpty(nameTerm) || c.Nicknames.Count() > 0 && c.Nicknames.Any(n => Regex.IsMatch(n, nameTermPattern, RegexOptions.IgnoreCase)) ? 0 : 1)
                    .ThenBy(c => c.Name)
                    .ToArray();
            }

            return new Card[] { };
        }
        #endregion
    }
}