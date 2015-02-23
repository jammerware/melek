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
                throw new InvalidOperationException("The storage directory specified for the MelekDataStore doesn't exist. And it probably should.");
            }

            // as long as the base directory exists, we can assume you know what you're doing... MAYBE
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
        public void DoLoad()
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
                                Rarity = EnuMaster.Parse<CardRarity>(XMLPal.GetString(printing.Attribute("rarity"))),
                                Set = setDictionary[XMLPal.GetString(printing.Attribute("setCode"))],
                                TransformsToMultiverseID = XMLPal.GetString(printing.Attribute("transformsInto"))
                            }
                        ).Distinct(new CardPrintingEqualityComparer()).OrderByDescending(a => a.Set.Date).ToList(),
                        Text = XMLPal.GetString(cardElement.Attribute("text")),
                        Toughness = XMLPal.GetInt(cardElement.Attribute("toughness")),
                        Tribe = XMLPal.GetString(cardElement.Attribute("tribe")),
                        Watermark = XMLPal.GetString(cardElement.Attribute("watermark"))
                    };

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
            try {
                await Task.Run(() => { DoLoad(); });
            }
            catch (AggregateException) {
                throw;
            }
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
            return await ResolveCardImage(printing, new Uri(string.Format("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card", printing.MultiverseID)));
        }

        public async Task<Uri> GetCardImageUri(Set set, Card card)
        {
            CardPrinting activePrinting = null;
            foreach(CardPrinting printing in card.Printings) {
                if (printing.Set.Code == set.Code) {
                    activePrinting = printing;
                    break;
                }
            }

            if (activePrinting != null) {
                return await ResolveCardImage(
                    activePrinting,
                    new Uri(
                        string.Format(
                            "http://mtgimage.com/set/{0}/{1}.jpg",
                            (!string.IsNullOrEmpty(activePrinting.Set.MtgImageName) ? activePrinting.Set.MtgImageName : activePrinting.Set.Code),
                            card.Name
                        )
                    )
                );
            }

            throw new InvalidOperationException("Looks like you tried to get an image for a set/card combination that doesn't exist. Try again.");
        }

        // overload that allows the end developer to request images by card and set. this has to be available
        // because Melek provides images for promo cards from a mtgimage.com, and the site doesn't seem to have
        // matching multiverseIDs for promo cards.
        public async Task<BitmapImage> GetCardImage(Set set, Card card)
        {
            return await ImageFromUri(await GetCardImageUri(set, card));
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
            MatchCollection matches = Regex.Matches(searchTerm.Trim(), "([a-zA-Z0-9]{0,3}):(\\S+)");

            if (matches.Count == 0) {
                return Search(new DataStoreSearchArgs() {
                    Name = searchTerm
                });
            }

            DataStoreSearchArgs args = new DataStoreSearchArgs();
            foreach (Match match in matches) {
                if (match.Groups[1].Value == "c") {
                    List<MagicColor> colors = new List<MagicColor>();
                    MagicColor tempColor = MagicColor.B;
                    foreach (char c in match.Groups[2].Value) {
                        if (EnuMaster.TryParse<MagicColor>(c.ToString(), out tempColor, true)) {
                            colors.Add(tempColor);
                        }
                    }
                    args.Colors = colors;
                }
            }

            return Search(args);
        }

        public Card[] Search(DataStoreSearchArgs args)
        {
            if (_IsLoaded) {
                string searchTerm = string.Empty;
                string setCode = string.Empty;

                if (!string.IsNullOrEmpty(args.Name)) searchTerm = args.Name.Trim().ToLower();
                if (!string.IsNullOrEmpty(args.SetCode)) setCode = args.SetCode.Trim().ToLower();

                if (args.HasArguments()) {
                    // clean this up at some point - add support for a collection of search terms to support possible other "replacement" exceptions
                    // character codes are utf8 by default (utf32)?
                    string searchTermAlt = searchTerm;
                    if (searchTermAlt.Contains("ae")) {
                        searchTermAlt = searchTermAlt.Replace("ae", ((Char)230).ToString());
                    }
                    if (searchTermAlt.Contains("u")) {
                        searchTermAlt = searchTermAlt.Replace("u", ((Char)251).ToString());
                    }

                    return _Cards
                        .Where(c =>
                            c.Name.ToLower().Contains(searchTermAlt) ||
                            c.Name.ToLower().Contains(searchTerm) || (
                                c.Nicknames.Count() > 0 &&
                                c.Nicknames.FirstOrDefault(n => n.ToLower() == searchTerm || n.ToLower().Contains(searchTerm)) != null
                            )
                        )
                        .Where(c => c.Printings.Any(a => a.Set.Code.ToLower() == setCode) || setCode == string.Empty)
                        .Where(c => c.IsColors(args.Colors) || args.Colors.Count() == 0)
                        //.Where(c => c.Printings.Any(p => args.Rarities.Contains(p.Rarity)) || args.Rarities == null || args.Rarities.Count() == 0)
                        .OrderBy(c => c.Name.ToLower() == searchTerm || c.Name.ToLower() == searchTermAlt ? 0 : 1)
                        .ThenBy(c => c.Nicknames.Count() > 0 && c.Nicknames.FirstOrDefault(n => n.ToLower() == searchTerm) != null ? 0 : 1)
                        .ThenBy(c => c.Name.ToLower().StartsWith(searchTermAlt) || c.Name.ToLower().StartsWith(searchTerm) ? 0 : 1)
                        .ThenBy(c => c.Name)
                        .ToArray();
                }
            }

            return new Card[] { };
        }
        #endregion
    }
}