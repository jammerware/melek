using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Bazam.WPF.UIHelpers;
using Bazam.WPF.ViewModels;
using Nivix.Infrastructure;
using Nivix.Models;
using Nivix.Views;
using Nivix.Views.Dialogs;

namespace Nivix.ViewModels
{
    public class CardsViewModel : ViewModelBase
    {
        [RelatedProperty("CardNicknames")]
        private IList<CardNickname> _CardNicknames;
        [RelatedProperty("DataIsDirty")]
        private bool _DataIsDirty;
        [RelatedProperty("NewCardName")]
        private string _NewCardName;
        [RelatedProperty("NewCardNicknames")]
        private IList<string> _NewCardNicknames;
        [RelatedProperty("SelectedCard")]
        private CardNickname _SelectedCard;
        [RelatedProperty("SelectedCardName")]
        private string _SelectedCardName;
        [RelatedProperty("SelectedNicknames")]
        private string[] _SelectedNicknames;

        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand((letsFrigginPartyBaby) => {
                    NewCardNicknameView dialog = new NewCardNicknameView();
                    dialog.DataContext = this;
                    bool? dialogResult = dialog.ShowDialog();

                    if (dialogResult != null && dialogResult.Value) {
                        CardNicknames.Add(new CardNickname() {
                            Name = NewCardName,
                            Nicknames = NewCardNicknames.ToArray()
                        });

                        CardNicknames = CardNicknames.OrderBy(c => c.Name).ToList();
                    }

                    NewCardName = string.Empty;
                    NewCardNicknames = null;
                });
            }
        }

        public IList<CardNickname> CardNicknames
        {
            get { return _CardNicknames; }
            set { ChangeProperty<CardsViewModel>(c => c.CardNicknames, value); }
        }

        public bool DataIsDirty
        {
            get { return _DataIsDirty; }
            set { ChangeProperty<CardsViewModel>(c => c.DataIsDirty, value); }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(
                    (timeToDeleteLiterallyEverything) => {
                        DeleteCardNicknameView dialog = new DeleteCardNicknameView();
                        dialog.DataContext = this;
                        bool? dialogResult = dialog.ShowDialog();

                        if (dialogResult != null && dialogResult.Value) {
                            CardNicknames.Remove(SelectedCard);
                            SelectedCard = null;
                        }
                    },
                    (vm) => { return SelectedCard != null; }
                );
            }
        }

        public string NewCardName
        { 
            get { return _NewCardName; }
            set { ChangeProperty<CardsViewModel>(c => c.NewCardName, value); }
        }

        public IList<string> NewCardNicknames
        {
            get { return _NewCardNicknames; }
            set { ChangeProperty<CardsViewModel>(c => c.NewCardNicknames, value); }
        }

        public ICommand SaveCommand
        {
            get { 
                return new RelayCommand((itsTimeToThings) => { 
                    DataBeast.SaveCardNicknames(CardNicknames);
                    SetDataIsDirty();
                }); 
            }
        }

        public CardNickname SelectedCard
        {
            get { return _SelectedCard; }
            set { 
                ChangeProperty<CardsViewModel>(c => c.SelectedCard, value);

                if (value != null) {
                    SelectedCardName = value.Name;
                    SelectedNicknames = value.Nicknames;
                }
                else {
                    SelectedCardName = string.Empty;
                    SelectedNicknames = null;
                }
            }
        }

        public string SelectedCardName
        {
            get { return _SelectedCardName; }
            set { ChangeProperty<CardsViewModel>(c => c.SelectedCardName, value); }
        }

        public string[] SelectedNicknames
        {
            get { return _SelectedNicknames; }
            set { 
                ChangeProperty<CardsViewModel>(c => c.SelectedNicknames, value);
                if (SelectedCard != null) {
                    SelectedCard.Nicknames = value;
                    SetDataIsDirty();
                }
            }
        }

        public CardsViewModel()
        {
            DataIsDirty = false;
            LoadData();
        }

        private CardNickname[] GetNicknameData()
        {
            List<CardNickname> list = new List<CardNickname>();
            foreach(CardNickname nick in DataBeast.GetCardNicknames()) {
                list.Add(nick);
            }

            return list.ToArray();
        }

        private void LoadData()
        {
            BindingList<CardNickname> data = new BindingList<CardNickname>();
            foreach (CardNickname nick in GetNicknameData()) {
                data.Add(nick);
            }

            data.ListChanged += (theList, somethingHappenedWithIt) => {
                SetDataIsDirty();
            };

            CardNicknames = data;
        }

        private void SetDataIsDirty()
        {
            CardNickname[] rawData = GetNicknameData();
            CardNickname[] modifiedData = new CardNickname[CardNicknames.Count];
            CardNicknames.CopyTo(modifiedData, 0);

            DataIsDirty = !(rawData.SequenceEqual(modifiedData));
        }
    }
}