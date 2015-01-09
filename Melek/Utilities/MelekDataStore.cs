using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using Bazam.Modules;
using Bazam.Slugging;
using Melek.Models;
using Melek.Models.Helpers;

namespace Melek.Utilities
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
        // general use constructor
        public MelekDataStore(string storageDirectory, bool storeCardImages, LoggingNinja loggingNinja) : this(storageDirectory, storeCardImages, loggingNinja, string.Empty) {}

        // for original dev only
        public MelekDataStore(string storageDirectory, bool storeCardImages, LoggingNinja loggingNinja, string devAuthkey)
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

            BackgroundBuddy.RunAsync(() => {
                ForceLoad();
                CheckForPackageUpdates();
            });
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

        #region internal utility methods
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

        private void ForceLoad()
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

                XDocument cardsDoc = null;
                try {
                    using (var reader = XmlReader.Create(Path.Combine(PackagesDirectory, package.ID, "cards.xml"))) {
                        cardsDoc = XDocument.Load(reader);
                    }
                }
                catch (Exception ex) {
                    _LoggingNinja.LogMessage("Locking error on the cards thing: " + ex.Message);
                }

                foreach (XElement cardElement in cardsDoc.Element("cards").Elements("card")) {
                    Card card = new Card() {
                        Appearances = (
                            from appearance in cardElement.Element("appearances").Elements("appearance")
                            select new CardAppearance() {
                                Artist = XMLPal.GetString(appearance.Attribute("artist")),
                                FlavorText = XMLPal.GetString(appearance.Attribute("flavor")),
                                MultiverseID = XMLPal.GetString(appearance.Attribute("multiverseID")),
                                Rarity = EnuMaster.Parse<CardRarity>(XMLPal.GetString(appearance.Attribute("rarity"))),
                                Set = setDictionary[XMLPal.GetString(appearance.Attribute("setCode"))]
                            }
                        ).Distinct(new CardAppearanceEqualityComparer()).OrderByDescending(a => a.Set.Date).ToList(),
                        Name = XMLPal.GetString(cardElement.Attribute("name")),
                        CardTypes = (
                            from cardType in cardElement.Elements("types").Elements("type")
                            select EnuMaster.Parse<CardType>(XMLPal.GetString(cardType.Attribute("name")))
                        ).ToArray(),
                        Cost = new CardCostCollection(XMLPal.GetString(cardElement.Attribute("cost"))),
                        FlavorText = XMLPal.GetString(cardElement.Attribute("flavor")),
                        Power = XMLPal.GetInt(cardElement.Attribute("power")),
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
                        foreach (CardAppearance appearance in card.Appearances) {
                            CardAppearance existingAppearance = existingCard.Appearances.Where(a => a.Set.Code == appearance.Set.Code).FirstOrDefault();
                            if (existingAppearance == null) {
                                existingCard.Appearances.Add(appearance);
                            }
                            else {
                                existingAppearance.Artist = appearance.Artist;
                                existingAppearance.FlavorText = appearance.FlavorText;
                                existingAppearance.MultiverseID = appearance.MultiverseID;
                                existingAppearance.Rarity = appearance.Rarity;
                                existingAppearance.Set = appearance.Set;
                            }

                            existingCard.CardTypes = card.CardTypes;
                            existingCard.Cost = card.Cost;
                            existingCard.FlavorText = card.FlavorText;
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

        #region exposed so the updater can force a check for updates
        public void CheckForPackageUpdates()
        {
            _LoggingNinja.LogMessage("Checking for package updates...");
            string packagesFolderUrl = Constants.PACKAGES_URL_PROD;
            if (_DevMode) {
                packagesFolderUrl = Constants.PACKAGES_URL_DEV;
            }
            string packagesManifestUrl = packagesFolderUrl + "packages.xml";

            BackgroundBuddy.RunAsync(() => {
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
                        ForceLoad();

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

            });
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

        public Uri GetAppearanceImageUri(CardAppearance appearance)
        {
            Uri localUri = new Uri(Path.Combine(CardImagesDirectory, Slugger.Slugify(appearance.MultiverseID) + ".jpg"));
            Uri webUri = new Uri(string.Format("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card", appearance.MultiverseID));

            if (_SaveCardImages) {
                if (!File.Exists(localUri.LocalPath)) {
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
        }

        public async Task<BitmapImage> GetAppearanceImage(CardAppearance appearance)
        {
            BitmapImage image = await Task.Run<BitmapImage>(() => {
                try {
                    BitmapImage img = new BitmapImage();
                    Uri uri = GetAppearanceImageUri(appearance);
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

        public string GetCardImageCacheSize()
        {
            double cardsDirectorySize = 0;
            foreach (string file in Directory.GetFiles(CardImagesDirectory)) {
                cardsDirectorySize += new FileInfo(file).Length;
            }

            return Math.Round(cardsDirectorySize / 1024 / 1024, 1).ToString() + " MB";
        }

        public string GetRandomCardName()
        {
            if (_IsLoaded) {
                return _Cards[new Random().Next(_Cards.Count() - 1)].Name;
            }
            return "Melek, Izzet Paragon"; // LOL
        }

        public IEnumerable<Package> GetPackages()
        {
            return _Packages;
        }

        public IEnumerable<Set> GetSets()
        {
            return _Sets;
        }

        public Card[] Search(string searchTerm)
        {
            return Search(searchTerm, string.Empty);
        }

        public Card[] Search(string searchTerm, string setCode)
        {
            if (_IsLoaded && !string.IsNullOrEmpty(searchTerm)) {
                searchTerm = searchTerm.Trim().ToLower();

                // clean this up at some point - add support for a collection of search terms to support possible other "replacement" exceptions
                string searchTermAlt = searchTerm;
                if (searchTermAlt.Contains("ae")) {
                    searchTermAlt = searchTermAlt.Replace("ae", ((Char)230).ToString());
                }

                if (searchTerm != string.Empty) {
                    return _Cards
                        .Where(c => c.Name.ToLower().Contains(searchTermAlt) || c.Name.ToLower().Contains(searchTerm))
                        .Where(c => c.Appearances.Any(a => a.Set.Code.ToLower() == setCode) || setCode == string.Empty)
                        .OrderBy(c => c.Name.ToLower().StartsWith(searchTermAlt) || c.Name.ToLower().StartsWith(searchTerm) ? 0 : 1)
                        .ThenBy(c => c.Name)
                        .ToArray();
                }
            }

            return new Card[] { };
        }
        #endregion
    }
}