using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Bazam.APIs.SharpZipLib;
using Bazam.Modules;
using Bazam.Slugging;
using BazamWPF.ViewModels;
using FirstFloor.ModernUI.Presentation;
using Melek;
using Melek.Models;
using Melek.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nivix.Infrastructure;
using Nivix.Models;

namespace Nivix.ViewModels
{
    public class PackageBuilderViewModel : ViewModelBase
    {
        #region Constants
        private const int SOFT_HYPHEN_CODE = 8722;
        #endregion

        #region Fields
        [RelatedProperty("CardReleaseDate")]
        private DateTime? _CardReleaseDate;
        [RelatedProperty("DeployToDev")]
        private bool _DeployToDev = true;
        [RelatedProperty("DeployToProd")]
        private bool _DeployToProd = true;
        [RelatedProperty("ManifestPath")]
        private string _ManifestPath;
        [RelatedProperty("OutputPath")]
        private string _OutputPath;
        [RelatedProperty("PackageID")]
        private string _PackageID;
        [RelatedProperty("PackageName")]
        private string _PackageName;
        [RelatedProperty("SourceDatabasePath")]
        private string _SourceDatabasePath;
        #endregion

        #region Properties
        public DateTime? CardReleaseDate
        {
            get { return _CardReleaseDate; }
            set { ChangeProperty<PackageBuilderViewModel>(vm => vm.CardReleaseDate, value); }
        }

        public bool DeployToDev
        {
            get { return _DeployToDev; }
            set { ChangeProperty<PackageBuilderViewModel>(vm => vm.DeployToDev, value); }
        }

        public bool DeployToProd
        {
            get { return _DeployToProd; }
            set { ChangeProperty<PackageBuilderViewModel>(vm => vm.DeployToProd, value); }
        }

        public ICommand GoCommand
        {
            get { return new RelayCommand((shitIsGoinDown) => { StartTheProcess(); }); }
        }

        public string ManifestPath
        {
            get { return _ManifestPath; }
            set { ChangeProperty<PackageBuilderViewModel>(vm => vm.ManifestPath, value); }
        }

        public string OutputPath
        {
            get { return _OutputPath; }
            set { ChangeProperty<PackageBuilderViewModel>(vm => vm.OutputPath, value); }
        }

        public string PackageID
        {
            get { return _PackageID; }
            set { ChangeProperty<PackageBuilderViewModel>(vm => vm.PackageID, value); }
        }

        public string PackageName
        {
            get { return _PackageName; }
            set { ChangeProperty<PackageBuilderViewModel>(vm => vm.PackageName, value); }
        }

        public string SourceDatabasePath
        {
            get { return _SourceDatabasePath; }
            set { ChangeProperty<PackageBuilderViewModel>(vm => vm.SourceDatabasePath, value); }
        }
        #endregion

        #region Internal utility methods
        private void DeployToEnvironment(XDocument sets, XDocument cards, Manifest manifest, string environment)
        {
            string outputPath = Path.Combine(OutputPath, environment);
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }

            string packagesDirectory = Path.Combine(outputPath, "packages");
            if (!Directory.Exists(packagesDirectory)) {
                Directory.CreateDirectory(packagesDirectory);
            }
            outputPath = packagesDirectory;

            string packageDirectory = Path.Combine(outputPath, PackageID);
            if (!Directory.Exists(packageDirectory)) {
                Directory.CreateDirectory(packageDirectory);
            }
            outputPath = packageDirectory;

            string setsFileName = Path.Combine(outputPath, "sets.xml");
            string cardsFileName = Path.Combine(outputPath, "cards.xml");

            sets.Save(setsFileName);
            cards.Save(cardsFileName);

            // zip up and delete the component files
            BazamZip zip = new BazamZip() {
                ZipFileName = Path.Combine(packagesDirectory, PackageID.ToLower() + ".gbd"),
                Password = Constants.ZIP_PASSWORD
            };
            zip.Files = new string[] { setsFileName, cardsFileName };
            zip.FilesRelativeRootForZip = packagesDirectory;
            zip.PreserveFilePaths = true;
            SharpZipLibHelper.Zip(zip, true);

            // deploy the manifest
            XDocument manifestDoc = manifest.ToXML();
            manifestDoc.Save(Path.Combine(packagesDirectory, "packages.xml"));

            // clean up
            Directory.Delete(packageDirectory);
        }

        private Manifest GetManifest()
        {
            Manifest manifest = new Manifest();

            if (!string.IsNullOrEmpty(ManifestPath) && File.Exists(ManifestPath)) {
                return Manifest.FromXML(XDocument.Load(ManifestPath));
            }
            return manifest;
        }

        private void StartTheProcess()
        {
            try {
                // load the database
                XDocument doc = XDocument.Load(SourceDatabasePath);

                // init some things
                Dictionary<string, string[]> cardNicknames = new Dictionary<string, string[]>();
                Dictionary<string, SetData> setData = new Dictionary<string, SetData>();
                Dictionary<string, Set> sets = new Dictionary<string, Set>();

                // load local data - card nicknames, set code overrides, things like that
                IList<CardNickname> cardNicknamesDeserialized = DataBeast.GetCardNicknames();
                IList<SetData> setDataDeserialized = DataBeast.GetSetData();

                foreach (CardNickname card in cardNicknamesDeserialized) {
                    cardNicknames.Add(card.Name, card.Nicknames);
                }

                foreach (SetData set in setDataDeserialized) {
                    setData.Add(set.Code, set);
                }

                List<XElement> setElements = new List<XElement>();
                List<XElement> cardElements = new List<XElement>();

                IEnumerable<Set> rawSets = (
                    from set in doc.Root.Element("sets").Elements("set")
                    select new Set() {
                        Code = XMLPal.GetString(set.Element("code")),
                        Date = GetSetDate(XMLPal.GetString(set.Element("date"))),
                        IsPromo = XMLPal.GetBool(set.Element("is_promo")),
                        Name = XMLPal.GetString(set.Element("name"))
                    }
                );

                foreach (Set set in rawSets) {
                    string replacementCode = GetRealSetCodeFromFakeSetCode(setDataDeserialized, set.Code);
                    if (!string.IsNullOrEmpty(replacementCode)) {
                        set.Code = replacementCode;
                    }

                    if (setData.Keys.Contains(set.Code)) {
                        SetData thisSetData = setData[set.Code];

                        set.CFName = thisSetData.CfName;
                        set.MtgImageName = thisSetData.MtgImageCode;
                        set.TCGPlayerName = thisSetData.TcgPlayerName;
                    }

                    if(!sets.ContainsKey(set.Code)) {
                        sets.Add(set.Code, set);
                        setElements.Add(
                            new XElement(
                                "set",
                                new XAttribute("name", setData.Keys.Contains(set.Code) ? setData[set.Code].Name : set.Name),
                                new XAttribute("code", set.Code),
                                (string.IsNullOrEmpty(set.CFName) ? null : new XAttribute("cfName", set.CFName)),
                                new XAttribute("isPromo", set.IsPromo),
                                (string.IsNullOrEmpty(set.MtgImageName) ? null : new XAttribute("mtgImageName", set.MtgImageName)),
                                (string.IsNullOrEmpty(set.TCGPlayerName) ? null : new XAttribute("tcgPlayerName", set.TCGPlayerName)),
                                (set.Date == null ? null : new XAttribute("date", set.Date))
                            )
                        );
                    }
                }

                Dictionary<string, Card> cards = new Dictionary<string, Card>();

                // pass to load card data
                foreach (XElement cardData in doc.Root.Element("cards").Elements("card")) {
                    // read some generally useful things
                    string name = XMLPal.GetString(cardData.Element("name"));
                    string sluggedName = Slugger.Slugify(name);
                    string setCode = cardData.Element("set").Value;

                    string realCode = GetRealSetCodeFromFakeSetCode(setDataDeserialized, setCode);
                    if (!string.IsNullOrEmpty(realCode)) {
                        setCode = realCode;
                    }

                    // we used the sluggedName variable to check if this card has been loaded before. if this is a split card, we can just check one half to see if it's been loaded before.
                    if (name.Contains("//")) {
                        sluggedName = Slugger.Slugify(GetSplitCardName(name, true));
                    }

                    // create the printing, we'll need it no matter what
                    CardPrinting printing = new CardPrinting() {
                        Artist = XMLPal.GetString(cardData.Element("artist")),
                        FlavorText = XMLPal.GetString(cardData.Element("flavor")),
                        MultiverseID = XMLPal.GetString(cardData.Element("id")),
                        Rarity = StringToCardRarityConverter.GetRarity(XMLPal.GetString(cardData.Element("rarity"))),
                        Set = sets[setCode],
                        TransformsToMultiverseID = XMLPal.GetString(cardData.Element("back_id"))
                    };

                    // first we need to know if this card has already been loaded (because it's in another set), in which case we need to 
                    // add a printing to the existing card for the current set. we can do this by checking its name, except the split
                    // cards need to be checked twice (once for each half).
                    if (cards.Keys.Contains(sluggedName)) {
                        if (!name.Contains("//")) {
                            // normal cards
                            cards[sluggedName].Printings.Add(printing);
                        }
                        else {
                            // split cards
                            string turnsSluggedName = sluggedName;
                            string burnsSluggedName = Slugger.Slugify(GetSplitCardName(name, false));

                            cards[turnsSluggedName].Printings.Add(printing);
                            cards[burnsSluggedName].Printings.Add(printing);
                        }
                    }
                    else {
                        // either it's a regular card or a split card
                        // parse data into variables for manipulation in the case of a split card
                        string id = XMLPal.GetString(cardData.Element("id"));
                        string rarity = XMLPal.GetString(cardData.Element("rarity"));
                        string artist = XMLPal.GetString(cardData.Element("artist"));
                        string types = XMLPal.GetString(cardData.Element("type"));
                        string cost = XMLPal.GetString(cardData.Element("manacost"));
                        string flavor = XMLPal.GetString(cardData.Element("flavor"));
                        string power = XMLPal.GetString(cardData.Element("power"));
                        string text = XMLPal.GetString(cardData.Element("ability"));
                        string toughness = XMLPal.GetString(cardData.Element("toughness"));
                        string tribe = types;
                        string watermark = XMLPal.GetString(cardData.Element("watermark"));

                        // check for nicknames
                        string[] nicknames = new string[] { };
                        if (cardNicknames.Keys.Contains(name)) {
                            nicknames = cardNicknames[name];
                        }

                        if (!name.Contains("//")) {
                            Card card = GetCard(printing, types, cost, name, power, text, toughness, tribe, watermark, sets);
                            card.Nicknames = nicknames;
                            cards.Add(Slugger.Slugify(name), card);
                        }
                        else {
                            string turnsArtist = GetSplitCardValue(artist, true);
                            string burnsArtist = GetSplitCardValue(artist, false);
                            string turnsTypes = GetSplitCardValue(types, true);
                            string burnsTypes = GetSplitCardValue(types, false);
                            string turnsCost = GetSplitCardValue(cost, true);
                            string burnsCost = GetSplitCardValue(cost, false);
                            string turnsFlavor = GetSplitCardValue(flavor, true);
                            string burnsFlavor = GetSplitCardValue(flavor, false);
                            string turnsName = GetSplitCardName(name, true);
                            string burnsName = GetSplitCardName(name, false);
                            string turnsPower = GetSplitCardValue(power, true);
                            string burnsPower = GetSplitCardValue(power, false);
                            string turnsText = GetSplitCardValue(text, true);
                            string burnsText = GetSplitCardValue(text, false);
                            string turnsToughness = GetSplitCardValue(toughness, true);
                            string burnsToughness = GetSplitCardValue(toughness, false);
                            string turnsTribe = GetSplitCardValue(tribe, true);
                            string burnsTribe = GetSplitCardValue(tribe, false);
                            string turnsWatermark = GetSplitCardValue(watermark, true);
                            string burnsWatermark = GetSplitCardValue(watermark, false);

                            Card turn = GetCard(printing, turnsTypes, turnsCost, turnsName, turnsPower, turnsText, turnsToughness, turnsTribe, turnsWatermark, sets);
                            Card burn = GetCard(printing, burnsTypes, burnsCost, burnsName, burnsPower, burnsText, burnsToughness, burnsTribe, burnsWatermark, sets);

                            turn.Nicknames = nicknames;
                            burn.Nicknames = nicknames;

                            cards.Add(Slugger.Slugify(turnsName), turn);
                            cards.Add(Slugger.Slugify(burnsName), burn);
                        }
                    }
                }

                // pass to generate xml
                foreach (Card card in cards.Values) {
                    List<XElement> cardTypes = new List<XElement>();
                    foreach (CardType cardType in card.CardTypes) {
                        cardTypes.Add(new XElement("type", new XAttribute("name", cardType.ToString())));
                    }

                    List<XElement> cardPrintings = new List<XElement>();
                    foreach (CardPrinting printing in card.Printings) {
                        cardPrintings.Add(
                            new XElement(
                                "appearance",
                                new XAttribute("multiverseID", printing.MultiverseID),
                                new XAttribute("rarity", printing.Rarity.ToString()),
                                new XAttribute("setCode", printing.Set.Code),
                                (!string.IsNullOrEmpty(printing.FlavorText) ? new XAttribute("flavor", printing.FlavorText) : null),
                                new XAttribute("artist", printing.Artist),
                                (!string.IsNullOrEmpty(printing.TransformsToMultiverseID) ? new XAttribute("transformsInto", printing.TransformsToMultiverseID) : null)
                            )
                        );
                    }

                    List<XElement> cardNicknamesElement = new List<XElement>();
                    foreach (string nickname in card.Nicknames) {
                        cardNicknamesElement.Add(new XElement("nickname", nickname));
                    }

                    string power = (card.Power == null ? card.Power.ToString() : string.Empty);
                    string toughness = (card.Toughness == null ? card.Toughness.ToString() : string.Empty);

                    XElement cardElement = new XElement(
                        "card",
                        new XAttribute("name", card.Name),
                        (card.Cost != null && card.Cost.ToString() != string.Empty ? new XAttribute("cost", card.Cost.ToString()) : null),
                        (!string.IsNullOrEmpty(card.Text) ? new XAttribute("text", card.Text) : null),
                        (!string.IsNullOrEmpty(card.Tribe) ? new XAttribute("tribe", card.Tribe) : null),
                        (!string.IsNullOrEmpty(card.Watermark) ? new XAttribute("watermark", card.Watermark) : null),
                        new XElement("types", cardTypes),
                        new XElement("appearances", cardPrintings),
                        (cardNicknamesElement.Count() > 0 ? new XElement("nicknames", cardNicknamesElement) : null)
                    );

                    if (!string.IsNullOrEmpty(power)) {
                        cardElement.Attribute("power").Value = power;
                    }

                    if (!string.IsNullOrEmpty(toughness)) {
                        cardElement.Attribute("toughness").Value = toughness;
                    }

                    cardElements.Add(cardElement);
                }

                XDocument outputSets = new XDocument(new XElement("sets"));
                outputSets.Root.Add(setElements);

                XDocument outputCards = new XDocument(new XElement("cards"));
                outputCards.Root.Add(cardElements);

                // make the manifest
                Manifest manifest = GetManifest();

                if (manifest.Packages.FirstOrDefault(p => p.ID == PackageID) == null) {
                    manifest.Packages.Add(new Package() {
                        CardsReleased = CardReleaseDate,
                        DataUpdated = DateTime.Now,
                        ID = PackageID,
                        Name = PackageName
                    });
                }

                // deploy
                if (DeployToDev) {
                    DeployToEnvironment(outputSets, outputCards, manifest, "dev");
                }

                if (DeployToProd) {
                    DeployToEnvironment(outputSets, outputCards, manifest, "prod");
                }

                // open 'er up
                Process.Start("explorer.exe", OutputPath);

                Console.WriteLine(cardElements.Count.ToString() + " cards in " + setElements.Count().ToString() + " sets generated.");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }
        #endregion

        #region Parsing helpers
        private string GetCardTribe(string input)
        {
            if (input.IndexOf('—') >= 0) {
                input = input.Substring(input.IndexOf('—') + 1);
            }
            return input.Trim();
        }

        private CardType[] GetCardTypes(string input)
        {
            List<CardType> retVal = new List<CardType>();
            input = input.ToUpper();

            if (input.IndexOf('—') >= 0) {
                input = input.Substring(0, input.IndexOf('—'));
            }
            string[] splitInput = input.Split(new Char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (input.Contains("ENCHANT ")) {
                retVal.Add(CardType.ENCHANTMENT);
            }
            else if (input.Contains("SCHEME")) {
                retVal.Add(CardType.SCHEME);
            }
            else {
                foreach (string inputPiece in splitInput) {
                    if (inputPiece == "EATURECRAY") {
                        retVal.Add(CardType.CREATURE);
                    }
                    else if (inputPiece == "INTERRUPT") {
                        retVal.Add(CardType.INSTANT);
                    }
                    else {
                        retVal.Add(EnuMaster.Parse<CardType>(inputPiece.ToUpper().Trim()));
                    }
                }
            }

            return retVal.ToArray();
        }

        private DateTime? GetSetDate(string input)
        {
            DateTime? retVal = null;
            DateTime parseTarget;
            if (DateTime.TryParse(input, out parseTarget)) {
                retVal = new DateTime?(parseTarget);
            }
            else {
                Match match = Regex.Match(input, "([0-9]{2})/([0-9]{4})");
                int month, year;
                Int32.TryParse(match.Groups[1].Value, out month);
                Int32.TryParse(match.Groups[2].Value, out year);

                if (month > 0 && year > 0) {
                    retVal = new DateTime?(new DateTime(year, month, 1));
                }
            }

            return retVal;
        }

        private Card GetCard(CardPrinting printing, string cardTypes, string cost, string name, string power, string text, string toughness, string tribeData, string watermark, Dictionary<string, Set> setDictionary)
        {
            int dummyForOutParams = 0;

            // With the gatherer update in 2014, Wizards stopped using hard hyphens (the subtraction sign on every keyboard) and instead
            // replaced them with ASCII character 8722, which http://www.ascii.cl/htmlcodes.htm describes as a "soft hyphen." BECAUSE
            // THAT FUCKING MAKES SENSE. How do they even enter the data in the DB with a character that isn't on a keyboard?
            // ... GOD. Just replace them with the normal hyphen sign, which was good enough for our parents.
            text = text.Replace((char)SOFT_HYPHEN_CODE, '-');

            return new Card() {
                CardTypes = GetCardTypes(cardTypes),
                Cost = (!string.IsNullOrEmpty(cost) ? new CardCostCollection(cost) : null),
                Name = name,
                Power = (Int32.TryParse(power, out dummyForOutParams) ? (int?)Int32.Parse(power) : null),
                Printings = new List<CardPrinting>() { 
                    printing
                },
                Text = text,
                Toughness = (Int32.TryParse(toughness, out dummyForOutParams) ? (int?)Int32.Parse(toughness) : null),
                Tribe = GetCardTribe(tribeData),
                Watermark = watermark
            };
        }

        private string GetRealSetCodeFromFakeSetCode(IList<SetData> setData, string fakeCode)
        {
            SetData match = setData.Where(s => s.GathererCode == fakeCode).FirstOrDefault();
            if (match != null) return match.Code;
            return string.Empty;
        }

        private string GetSplitCardName(string name, bool firstHalf)
        {
            return name + " (" + name.Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[firstHalf ? 0 : 1].Trim() + ")";
        }

        private string GetSplitCardValue(string input, bool firstHalf)
        {
            Match match = Regex.Match(input, "([\\s\\S]+)//([\\s\\S]+)");
            if (match.Value != string.Empty) {
                return (firstHalf ? match.Groups[1].Value : match.Groups[2].Value).Trim();
            }
            return input;
        }
        #endregion
    }
}