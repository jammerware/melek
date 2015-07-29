using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Bazam.Modules;
using Bazam.Wpf.ViewModels;
using FirstFloor.ModernUI.Presentation;
using Melek.Client.DataStore;
using Melek.Domain;
using Newtonsoft.Json;
using Nivix.Infrastructure;
using Nivix.Models;

namespace Nivix.ViewModels
{
    public class PackageBuilderViewModel : ViewModelBase<PackageBuilderViewModel>
    {
        #region Fields
        private string _CurrentVersion;
        private bool _DeployToDev = true;
        private bool _DeployToProd = true;
        private DateTime _ReleaseDate;
        private string _ReleaseNotes;
        private string _SourceDatabasePath;
        private string _VersionNo;
        #endregion

        #region Constructor
        public PackageBuilderViewModel()
        {
            SourceDatabasePath = Path.Combine(Assembly.GetExecutingAssembly().Location, @"melek-data-store.json");
        }
        #endregion

        #region Properties
        public string CurrentVersion
        {
            get 
            {
                if (_CurrentVersion == null) {
                    //using (MelekDbContext context = new MelekDbContext()) {
                    //    _CurrentVersion = context.Version.First().Version;
                    //}
                }

                return _CurrentVersion;
            }
        }

        public bool DeployToDev
        {
            get { return _DeployToDev; }
            set { ChangeProperty(vm => vm.DeployToDev, value); }
        }

        public bool DeployToProd
        {
            get { return _DeployToProd; }
            set { ChangeProperty(vm => vm.DeployToProd, value); }
        }

        public ICommand GoCommand
        {
            get { return new RelayCommand((shitIsGoinDown) => { StartTheProcess(); }); }
        }

        public DateTime ReleaseDate
        {
            get 
            {
                if (_ReleaseDate == DateTime.MinValue) {
                    _ReleaseDate = DateTime.Now;
                }

                return _ReleaseDate; 
            }
            set { ChangeProperty(vm => vm.ReleaseDate, value); }
        }

        public string ReleaseNotes
        {
            get { return _ReleaseNotes; }
            set { ChangeProperty(vm => vm.ReleaseNotes, value); }
        }

        public string SourceDatabasePath
        {
            get { return _SourceDatabasePath; }
            set { ChangeProperty(vm => vm.SourceDatabasePath, value); }
        }
        
        public string VersionNo
        {
            get { return _VersionNo; }
            set { ChangeProperty(vm => vm.VersionNo, value); }
        }
        #endregion

        #region Internal utility methods
        private async void StartTheProcess()
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

                        set.CFName = (string.IsNullOrEmpty(thisSetData.CfName) ? null : thisSetData.CfName);
                        set.TCGPlayerName = (string.IsNullOrEmpty(thisSetData.TcgPlayerName) ? null : thisSetData.TcgPlayerName);
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

                MelekDataStore store = new MelekDataStore() {
                    Cards = cardFactory.Cards.Values.ToList(),
                    ReleaseNotes = ReleaseNotes,
                    Sets = cardFactory.Sets.Values.ToList(),
                    Version = VersionNo
                };

                //string data = await Task.Factory.StartNew<string>(() => { return JsonConvert.SerializeObject(store, MelekDataStore.GetRequiredConverters()); });
                string data = await Task.Factory.StartNew<string>(() => { return JsonConvert.SerializeObject(store); });
                File.WriteAllText("melek-data-store.json", data);

                //// update the DB oh gurl
                //DtoFactory dtoFactory = new DtoFactory();
                //MelekDbContext database = new MelekDbContext();

                //// sets
                //IEnumerable<SetDto> setDtos = sets.Values.Select(s => dtoFactory.GetSetDto(s));
                //database.Sets.AddRange(setDtos);

                //// cardz
                //foreach (ICard card in cardFactory.Cards.Values) {
                //    database.Cards.Add(dtoFactory.GetCardDto(card));
                //}
                
                //// version
                //database.Version.RemoveRange(database.Version);
                //database.Version.Add(new ApiVersionDto() {
                //    Notes = this.ReleaseNotes,
                //    ReleaseDate = this.ReleaseDate,
                //    Version = this.VersionNo
                //});

                //// savez
                //CancellationToken cancelToken = new CancellationToken();
                //database.SaveChangesAsync(cancelToken);
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
            return null;
        }
        #endregion
    }
}