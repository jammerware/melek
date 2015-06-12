using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Bazam.Modules;
using Bazam.Modules.Enumerations;
using Bazam.SharpZipLibHelpers;
using Bazam.Slugging;
using Bazam.WPF.ViewModels;
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

        #region Constructor
        public PackageBuilderViewModel()
        {
            OutputPath = @"E:\Dev\Melek\Project\Live";
            SourceDatabasePath = @"E:\Dev\Melek\Nivix\Data\core.xml";
        }
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
                        IsPromo = (XMLPal.GetBool(set.Element("is_promo")) ?? false),
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
                                (string.IsNullOrEmpty(set.TCGPlayerName) ? null : new XAttribute("tcgPlayerName", set.TCGPlayerName)),
                                (set.Date == null ? null : new XAttribute("date", set.Date))
                            )
                        );
                    }
                }

                Dictionary<string, Card> cards = new Dictionary<string, Card>();

                // pass to load card data
                // flip cards (like Erayo, Soratami Ascendant) have ——— in their text
                // split cards (like Beck // Call) have // in their text
                // transforming cards (like Huntmaster of the Fells) have a back_id property in the source db
                CardFactory cardFactory = new CardFactory() {
                    CardNicknames = cardNicknames,
                    Sets = sets,
                    SetMetaData = setData
                };
                foreach (XElement cardData in doc.Root.Element("cards").Elements("card")) {
                    cardFactory.AddCardData(cardData);
                }

                // TODO: generate sql and update db omg

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
                return input.Trim();
            }
            return string.Empty;
        }

        private List<CardType> GetCardTypes(string input)
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

            return retVal;
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

        private Card GetCard(PrintingBase printing, string cardTypesData, string cost, string name, string power, string text, string toughness, string tribeData, string watermark, Dictionary<string, Set> setDictionary, List<Format> legalFormats, List<Ruling> rulings)
        {
            int dummyForOutParams = 0;

            // With the gatherer update in 2014, Wizards stopped using hard hyphens (the subtraction sign on every keyboard) and instead
            // replaced them with ASCII character 8722, which http://www.ascii.cl/htmlcodes.htm describes as a "soft hyphen." BECAUSE
            // THAT FUCKING MAKES SENSE. How do they even enter the data in the DB with a character that isn't on a keyboard?
            // ... GOD. Just replace them with the normal hyphen sign, which was good enough for our parents.
            text = text.Replace((char)SOFT_HYPHEN_CODE, '-');

            // if the card isn't a legendary creature, it's not legal as a general in commander
            List<CardType> cardTypes = GetCardTypes(cardTypesData);
            if(legalFormats.Contains(Format.CommanderGeneral) && !(cardTypes.Contains(CardType.LEGENDARY) && cardTypes.Contains(CardType.CREATURE))) {
                legalFormats.Remove(Format.CommanderGeneral);
            }

            return new Card() {
                CardTypes = cardTypes.ToArray(),
                Cost = (!string.IsNullOrEmpty(cost) ? new CardCostCollection(cost) : null),
                LegalFormats = legalFormats.ToArray(),
                Name = name,
                Power = (Int32.TryParse(power, out dummyForOutParams) ? (int?)Int32.Parse(power) : null),
                Printings = new List<Printing>() { 
                    //TODO: FIX
                    //printing
                },
                Rulings = rulings.ToArray(),
                Text = text,
                Toughness = (Int32.TryParse(toughness, out dummyForOutParams) ? (int?)Int32.Parse(toughness) : null),
                //TODO: FIX
                //Tribe = GetCardTribe(tribeData),
                //Watermark = watermark
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