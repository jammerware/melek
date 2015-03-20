using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Bazam.APIs.SharpZipLib;
using Bazam.Modules;
using Bazam.Slugging;
using Melek.Models;
using Melek.Models.Helpers;
using Melek.Utilities;

namespace Melek.DataStore
{
    public class MelekDataStore
    {
        #region Events
        public event EventHandler DataLoaded;
        public event PackagesUpdatedEventHandler PackagesUpdated;
        #endregion

        #region Fields
        private Card[] _Cards;
        private bool _DevMode = false;
        private bool _IsLoaded = false;
        private LoggingNinja _LoggingNinja;
        private List<Package> _Packages;
        private Set[] _Sets;
        private bool _SaveCardImages;
        private string _StorageDirectory;
        #endregion

        #region Constructors
        // general use constructors
        public MelekDataStore(string storageDirectory, bool storeCardImages, LoggingNinja loggingNinja) : this(storageDirectory, storeCardImages, loggingNinja, false, string.Empty) { }
        public MelekDataStore(string storageDirectory, bool storeCardImages, LoggingNinja loggingNinja, bool lazyLoad) : this(storageDirectory, storeCardImages, loggingNinja, lazyLoad, string.Empty) { }

        // for original dev only
        public MelekDataStore(string storageDirectory, bool storeCardImages, LoggingNinja loggingNinja, bool lazyLoad, string devAuthkey)
        {
            // flip on dev mode if appropriate
            if (!string.IsNullOrEmpty(devAuthkey) && Cryptonite.Encrypt(devAuthkey, Constants.DEV_AUTH_KEY_PRIVATE_KEY) == Cryptonite.Encrypt(Constants.DEV_AUTH_KEY_PUBLIC_KEY, Constants.DEV_AUTH_KEY_PRIVATE_KEY)) {
                _DevMode = true;
            }

            _LoggingNinja = loggingNinja;
            _SaveCardImages = storeCardImages;
            _StorageDirectory = storageDirectory;

            if (!Directory.Exists(storageDirectory)) {
                Directory.CreateDirectory(storageDirectory);
            }

            if (!Directory.Exists(CardImagesDirectory) && storeCardImages) {
                Directory.CreateDirectory(CardImagesDirectory);
            }

            if (!Directory.Exists(PackagesDirectory)) {
                Directory.CreateDirectory(PackagesDirectory);
            }

            string packagesManifestFileName = Path.Combine(PackagesDirectory, "packages.xml");
            if (!File.Exists(packagesManifestFileName)) {
                XDocument doc = new XDocument(new XElement("packages"));
                doc.Save(packagesManifestFileName);
            }

            _Packages = new List<Package>();

            if (!lazyLoad) {
                BackgroundBuddy.RunAsync(async() => {
                    await ForceLoad();
                    await CheckForPackageUpdates();
                });
            }
        }
        #endregion

        #region internal utility properties
        public string CardImagesDirectory
        {
            get { return Path.Combine(_StorageDirectory, "cards"); }
        }

        public string PackagesDirectory
        {
            get { return Path.Combine(_StorageDirectory, "packages"); }
        }

        public string StorageDirectory
        {
            get { return _StorageDirectory; }
        }
        #endregion

        #region data management utility methods
        private void DoLoad()
        {
            XDocument packagesManifest = XDocument.Load(Path.Combine(PackagesDirectory, "packages.xml"));
            _Packages = new List<Package>();
            _Packages.AddRange(GetPackagesFromManifest(packagesManifest));

            List<Card> cards = new List<Card>();
            List<Set> sets = new List<Set>();

            foreach (Package package in _Packages) {
                XDocument setDoc = null;
                using (var reader = XmlReader.Create(Path.Combine(PackagesDirectory, package.ID, "sets.xml"))) {
                    setDoc = XDocument.Load(reader);
                }

                // iterate through each set, adding it if it hasn't appeared in another package or updating it if it has
                foreach (XElement setElement in setDoc.Element("sets").Elements("set")) {
                    Set set = new Set() {
                        Code = XMLPal.GetString(setElement.Attribute("code")),
                        CFName = XMLPal.GetString(setElement.Attribute("cfName")),
                        Date = XMLPal.GetDate(setElement.Attribute("date")),
                        IsPromo = (setElement.Attribute("isPromo") != null ? XMLPal.GetBool(setElement.Attribute("isPromo")) : false),
                        MtgImageName = (setElement.Attribute("mtgImageName") != null ? XMLPal.GetString(setElement.Attribute("mtgImageName")) : string.Empty),
                        Name = XMLPal.GetString(setElement.Attribute("name")),
                        TCGPlayerName = XMLPal.GetString(setElement.Attribute("tcgPlayerName"))
                    };

                    Set existingSet = sets.Where(s => s.Code == set.Code).FirstOrDefault();

                    if (existingSet == null) {
                        sets.Add(set);
                    }
                    else {
                        existingSet.CFName = set.CFName;
                        existingSet.Date = set.Date;
                        existingSet.Name = set.Name;
                        existingSet.TCGPlayerName = set.TCGPlayerName;
                    }
                }

                Dictionary<string, Set> setDictionary = new Dictionary<string, Set>();
                foreach (Set set in sets) {
                    setDictionary.Add(set.Code, set);
                }

                string cardsDocPath = Path.Combine(PackagesDirectory, package.ID, "cards.xml");
                XDocument cardsDoc = XDocument.Load(cardsDocPath);

                foreach (XElement cardElement in cardsDoc.Element("cards").Elements("card")) {
                    Card card = new Card() {
                        Name = XMLPal.GetString(cardElement.Attribute("name")),
                        Nicknames = (
                            cardElement.Element("nicknames") == null ? new List<string>() :
                            from nickname in cardElement.Element("nicknames").Elements("nickname")
                            select nickname.Value
                        ).ToArray(),
                        CardTypes = (
                            from cardType in cardElement.Elements("types").Elements("type")
                            select EnuMaster.Parse<CardType>(XMLPal.GetString(cardType.Attribute("name")))
                        ).ToArray(),
                        Cost = new CardCostCollection(XMLPal.GetString(cardElement.Attribute("cost"))),
                        Power = XMLPal.GetInt(cardElement.Attribute("power")),
                        Printings = (
                            from printing in cardElement.Element("appearances").Elements("appearance")
                            select new CardPrinting() {
                                Artist = XMLPal.GetString(printing.Attribute("artist")),
                                FlavorText = XMLPal.GetString(printing.Attribute("flavor")),
                                MultiverseID = XMLPal.GetString(printing.Attribute("multiverseID")),
                                Rarity = StringToCardRarityConverter.GetRarity(XMLPal.GetString(printing.Attribute("rarity"))),
                                Set = setDictionary[XMLPal.GetString(printing.Attribute("setCode"))],
                                TransformsToMultiverseID = XMLPal.GetString(printing.Attribute("transformsInto"))
                            }
                        ).Distinct(new CardPrintingEqualityComparer()).OrderByDescending(a => a.Set.Date).ToList(),
                        Text = XMLPal.GetString(cardElement.Attribute("text")),
                        Toughness = XMLPal.GetInt(cardElement.Attribute("toughness")),
                        Tribe = XMLPal.GetString(cardElement.Attribute("tribe")),
                        Watermark = XMLPal.GetString(cardElement.Attribute("watermark"))
                    };

                    if (cardElement.Element("legalFormats") != null) {
                        List<Format> legalFormats = new List<Format>();
                        foreach (XElement formatElement in cardElement.Element("legalFormats").Elements("format")) {
                            legalFormats.Add(EnuMaster.Parse<Format>(formatElement.Attribute("name").Value));
                        }
                        card.LegalFormats = legalFormats.ToArray();
                    }

                    Card existingCard = cards.Where(c => c.Name == card.Name).FirstOrDefault();
                    if (existingCard == null) {
                        cards.Add(card);
                    }
                    else {
                        foreach (CardPrinting printing in card.Printings) {
                            CardPrinting existingPrinting = existingCard.Printings.Where(a => a.Set.Code == printing.Set.Code).FirstOrDefault();
                            if (existingPrinting == null) {
                                existingCard.Printings.Add(printing);
                            }
                            else {
                                existingPrinting.Artist = printing.Artist;
                                existingPrinting.FlavorText = printing.FlavorText;
                                existingPrinting.MultiverseID = printing.MultiverseID;
                                existingPrinting.Rarity = printing.Rarity;
                                existingPrinting.Set = printing.Set;
                            }

                            existingCard.CardTypes = card.CardTypes;
                            existingCard.Cost = card.Cost;
                            existingCard.Power = card.Power;
                            existingCard.Text = card.Text;
                            existingCard.Toughness = card.Toughness;
                            existingCard.Tribe = card.Tribe;
                            existingCard.Watermark = card.Watermark;
                        }
                    }
                }
            }

            _Cards = cards.ToArray();
            _Sets = sets.ToArray();

            if (_Packages.Count() > 0) {
                _IsLoaded = true;
                _LoggingNinja.LogMessage("Data loaded.");
                if (DataLoaded != null) {
                    DataLoaded(this, EventArgs.Empty);
                }
            }
            else {
                _LoggingNinja.LogMessage("Data loaded, but no packages were present.");
            }
        }

        private Package[] GetPackagesFromManifest(XDocument manifest)
        {
            return (
                from package in manifest.Element("packages").Elements("package")
                select new Package() {
                    CardsReleased = (package.Attribute("cardsReleased") == null ? null : new Nullable<DateTime>(XMLPal.GetDate(package.Attribute("cardsReleased")))),
                    DataUpdated = XMLPal.GetDate(package.Attribute("dataUpdated")),
                    ID = XMLPal.GetString(package.Attribute("id")),
                    Name = XMLPal.GetString(package.Attribute("name")),
                }
            ).ToArray();
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

        private void SavePackagesManifest()
        {
            IEnumerable<XElement> packageElements = (
                from p in _Packages
                select new XElement(
                    "package",
                    new XAttribute("id", p.ID),
                    new XAttribute("name", p.Name),
                    (p.CardsReleased != null ? new XAttribute("cardsReleased", p.CardsReleased.Value) : null),
                    new XAttribute("dataUpdated", p.DataUpdated)
                )
            );

            XDocument doc = new XDocument(new XElement("packages", packageElements));
            doc.Save(Path.Combine(PackagesDirectory, "packages.xml"));
        }
        #endregion

        #region Image management utility methods
        private async Task<BitmapImage> ImageFromUri(Uri uri)
        {
            BitmapImage image = await Task.Run<BitmapImage>(() => {
                try {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriSource = uri;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
                catch (NotSupportedException ex) {
                    _LoggingNinja.LogError(ex);
                }
                catch (IOException ex) {
                    _LoggingNinja.LogError(ex);
                }
                return null;
            });

            return image;
        }

        private async Task<Uri> ResolveCardImage(CardPrinting printing, Uri webUri)
        {
            Uri retVal = await Task.Run<Uri>(() => {
                Uri localUri = new Uri(Path.Combine(CardImagesDirectory, Slugger.Slugify(printing.MultiverseID) + ".jpg"));

                if (_SaveCardImages) {
                    if (!File.Exists(localUri.LocalPath) || new FileInfo(localUri.LocalPath).Length == 0) {
                        try {
                            new WebClient().DownloadFile(webUri.AbsoluteUri, localUri.LocalPath);
                        }
                        catch (WebException) {
                            // too slow motha fucka
                        }
                    }
                    return localUri;
                }
                return webUri;
            });

            return retVal;
        }
        #endregion

        #region exposed so the updater can force a check for updates
        public async Task CheckForPackageUpdates()
        {
            _LoggingNinja.LogMessage("Checking for package updates...");
            string packagesFolderUrl = Constants.PACKAGES_URL_PROD;
            if (_DevMode) {
                packagesFolderUrl = Constants.PACKAGES_URL_DEV;
            }
            string packagesManifestUrl = packagesFolderUrl + "packages.xml";

            try {
                XDocument manifest = XDocument.Load(packagesManifestUrl);
                Package[] remotePackages = GetPackagesFromManifest(manifest);
                Package[] localPackages = GetPackages().ToArray();
                List<string> packagesToUpdate = new List<string>();

                foreach (Package remotePackage in remotePackages) {
                    Package localPackage = null;
                    foreach (Package package in localPackages) {
                        if (package.ID == remotePackage.ID) {
                            localPackage = package;
                        }
                    }
                    if (localPackage == null || localPackage.DataUpdated < remotePackage.DataUpdated) {
                        packagesToUpdate.Add(remotePackage.ID);
                    }
                }

                List<Package> newPackages = new List<Package>();
                foreach (string packageID in packagesToUpdate) {
                    _LoggingNinja.LogMessage("Updating package data for " + packageID + "...");
                    using (WebClient client = new WebClient()) {
                        // file names n shit
                        string fileName = packageID + ".gbd";
                        string remotePath = packagesFolderUrl + fileName;
                        string localPath = Path.Combine(PackagesDirectory, packageID + ".zip");

                        // download the .gbd file, unzip it into the data directory
                        client.DownloadFile(remotePath, localPath);
                        SharpZipLibHelper.Unzip(localPath, PackagesDirectory, Constants.ZIP_PASSWORD, true);

                        // update the local list of packages with new and improved data from the update packages
                        Package localPackage = _Packages.Where(p => p.ID == packageID).FirstOrDefault();
                        if (localPackage != null) {
                            _Packages.Remove(localPackage);
                        }

                        Package newPackage = remotePackages.Where(p => p.ID == packageID).First();
                        newPackages.Add(newPackage);
                        _Packages.Add(newPackage);
                    }
                    _LoggingNinja.LogMessage("Package " + packageID + " updated.");
                }

                // delete any decomissioned packages
                List<Package> packagesToRemove = new List<Package>();
                foreach (Package package in localPackages) {
                    bool packageFound = false;
                    foreach (Package updatePackage in remotePackages) {
                        if (updatePackage.ID == package.ID) {
                            packageFound = true;
                        }
                    }

                    if (!packageFound) {
                        _LoggingNinja.LogMessage("Package " + package.ID + " wasn't found in the update manifest. It's decomissioned - removing from local the app's search DB.");
                        packagesToRemove.Add(package);
                    }
                }

                if (packagesToRemove.Count() > 0) {
                    foreach (Package package in packagesToRemove) {
                        _Packages.Remove(package);
                    }
                }


                if (packagesToUpdate.Count() > 0 || packagesToRemove.Count() > 0) {
                    // save the local packages manifest to reflect updates
                    SavePackagesManifest();
                    _LoggingNinja.LogMessage("Package update complete.");

                    // reload, bitches
                    DoLoad();

                    // clean up any package folders that need to go
                    foreach (Package package in packagesToRemove) {
                        _LoggingNinja.LogMessage("Deleting the data directory for package " + package.ID + ".");
                        string directory = Path.Combine(PackagesDirectory, package.ID);
                        if (Directory.Exists(directory)) {
                            Directory.Delete(directory, true);
                            _LoggingNinja.LogMessage("Deleted directory for package " + package.ID + ".");
                        }
                    }

                    if (PackagesUpdated != null) {
                        PackagesUpdated(newPackages.ToArray());
                    }
                }
            }
            catch (WebException) {
                _LoggingNinja.LogMessage("Tried to check for package updates, but didn't have internetz.");
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        #endregion

        #region public properties
        public bool StoreCardImagesLocally { get; set; }
        #endregion

        #region public data access
        public void ClearCardImageCache()
        {
            foreach (string fileName in Directory.GetFiles(CardImagesDirectory)) {
                try {
                    File.Delete(fileName);
                }
                catch (Exception ex) {
                    _LoggingNinja.LogError(ex);
                }
            }
        }

        public void ClearDataCache()
        {
            try {
                Directory.Delete(PackagesDirectory, true);
            }
            catch (Exception ex) {
                _LoggingNinja.LogError(ex);
            }
        }

        public Card GetCardByMultiverseID(string multiverseID)
        {
            return _Cards.Where(c => c.Printings.Where(a => a.MultiverseID == multiverseID).FirstOrDefault() != null).FirstOrDefault();
        }

        public Card GetCardByPrinting(CardPrinting printing)
        {
            return _Cards.Where(c => c.Printings.Contains(printing)).FirstOrDefault();
        }

        public async Task<Uri> GetCardImageUri(CardPrinting printing)
        {
            if (Regex.IsMatch(printing.MultiverseID, "^[0-9]+$")) {
                // these are typical non-promo cards
                return await ResolveCardImage(printing, new Uri(string.Format("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card", printing.MultiverseID)));
            }
            else {
                // promo cards we have to get from magiccards.info
                Match match = Regex.Match(printing.MultiverseID, @"([a-zA-Z0-9]+)_([a-zA-Z0-9]+)");
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

        public async Task<BitmapImage> GetCardImage(CardPrinting printing)
        {
            return await ImageFromUri(await GetCardImageUri(printing));
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

        public Card[] GetCards()
        {
            return _Cards;
        }

        public string GetRandomCardName()
        {
            if (_IsLoaded) {
                return _Cards[new Random().Next(_Cards.Count() - 1)].Name;
            }
            return "Melek, Izzet Paragon"; // LOL
        }

        public Package[] GetPackages()
        {
            return _Packages.ToArray();
        }

        public Set[] GetSets()
        {
            return _Sets;
        }

        public Card[] Search(string searchTerm)
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
                                criteria.Add((Card card) => { return card.IsColors(colors) && (!requireMulticolored || card.IsMulticolored()); });
                            else if (criteriaOperator == ":") 
                                criteria.Add((Card card) => { return colors.Any(color => card.IsColor(color) || (requireMulticolored && card.IsMulticolored())); });
                            break;
                        case "mid":
                            criteria.Add((Card card) => { return card.Printings.Any(p => p.MultiverseID.Equals(criteriaValue, StringComparison.InvariantCultureIgnoreCase)); });
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
                                criteria.Add((Card card) => { return card.Printings.Any(p => p.Rarity == rarity.Value); });
                            }

                            break;
                        default:
                            break;
                    }
                }

                return Search(null, criteria);
            }
        }

        private Card[] Search(string nameTerm, List<CardSearchDelegate> filters)
        {
            if (_IsLoaded && (!string.IsNullOrEmpty(nameTerm) || (filters != null && filters.Count > 0))) {
                IEnumerable<Card> cards = _Cards;
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
                    filters.Add((Card card) => { 
                        return 
                            Regex.IsMatch(card.Name, nameTermPattern, RegexOptions.IgnoreCase) ||
                            card.Nicknames.Any(n => Regex.IsMatch(n, nameTermPattern, RegexOptions.IgnoreCase)); 
                    });
                }

                foreach (CardSearchDelegate filter in filters) {
                    cards = cards.Where(c => filter(c));
                }

                return cards
                    .OrderBy(c => string.IsNullOrEmpty(nameTerm) || c.Name.StartsWith(nameTerm, StringComparison.InvariantCultureIgnoreCase) ? 0 : 1)
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