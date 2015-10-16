using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Melek.Domain.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                    _CurrentVersion = new WebClient().DownloadString(new Uri("http://melekapi.azurewebsites.net/api/Version"));
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
                
                string data = await Task.Factory.StartNew<string>(() => {
                    // We're adding the PrintingJsonConverter here (and again when we deserialize in the MelekClient class of Melek.Client) for
                    // very sad reasons.
                    //
                    // PrintingJsonConverter serializes all descendants of IPrinting in a default fashion except for FlipPrintings, which need
                    // to have a special JSON property injected into them so that we can check for its existence on deserialization. If your custom
                    // converter tries to serialize an object using the default serializer, a crazy bug happens (see the PrintingJsonConverter class
                    // for details), so you have to create a new JsonSerializer inside your converter that is completely independent of the 
                    // serialization process so false circular references don't get flagged.
                    //
                    // This is bad enough, but the problem is that if we decorate the PrintingBase class to indicate that objects of the type should
                    // always be serialized with PrintingJsonConverter, a StackOverflowException occurs (because the serializer tries to call your 
                    // converter to serialize the input, which causes the serializer to call your converter, which causes the serializer to call your
                    // converter...). Thus, while the serializer in charge of the greater serialization process needs to use this converter to
                    // serialize the input, but the lower-level one inside PrintingJsonConverter must not. Which means the use of the attribute is 
                    // off the table, which means I'm very sad and I hate this and I'm pretty sure I took a wrong turn at some point.
                    //
                    // TODO: FIX IT.
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.Converters.Add(new PrintingJsonConverter());
                    settings.Formatting = Formatting.None;
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    settings.TraceWriter = new MemoryTraceWriter();
                    
                    return JsonConvert.SerializeObject(store, settings);
                });
                File.WriteAllText("melek-data-store.json", data);
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