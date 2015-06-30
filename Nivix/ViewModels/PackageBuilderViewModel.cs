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
using Bazam.SharpZipLibHelpers;
using Bazam.Slugging;
using Bazam.WPF.ViewModels;
using FirstFloor.ModernUI.Presentation;
using Melek;
using Melek.Db;
using Melek.Db.Factories;
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

        #region Fields
        private DateTime? _CardReleaseDate;
        private bool _DeployToDev = true;
        private bool _DeployToProd = true;
        private string _ManifestPath;
        private string _OutputPath;
        private string _PackageID;
        private string _PackageName;
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

                foreach (XElement setElement in doc.Root.Element("sets").Elements("set")) {
                    Set set = new Set() {
                        IsPromo = (XmlPal.GetBool(setElement.Element("is_promo")) ?? false)
                    };
                }

                Set[] rawSets = (
                    from set in doc.Root.Element("sets").Elements("set")
                    select new Set() {
                        Code = XmlPal.GetString(set.Element("code")),
                        Date = GetSetDate(XmlPal.GetString(set.Element("date"))),
                        IsPromo = (XmlPal.GetBool(set.Element("is_promo")) ?? false),
                        Name = XmlPal.GetString(set.Element("name"))
                    }
                ).ToArray();

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
                    }
                }

                // pass to load card data
                CardFactory cardFactory = new CardFactory() {
                    CardNicknames = cardNicknames,
                    Sets = sets,
                    SetMetaData = setData
                };
                foreach (XElement cardData in doc.Root.Element("cards").Elements("card")) {
                    cardFactory.AddCardData(cardData);
                }

                // TODO: generate sql and update db omg
                DtoFactory dtoFactory = new DtoFactory();
                MelekDbContext database = new MelekDbContext();

                foreach (ICard card in cardFactory.Cards.Values) {
                    database.Cards.Add(dtoFactory.GetCardDto(card));
                }
                database.SaveChanges();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }
        #endregion

        #region Parsing helpers
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