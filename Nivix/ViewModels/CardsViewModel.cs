using System.Collections.Generic;
using System.ComponentModel;
using BazamWPF.ViewModels;
using Nivix.Infrastructure;
using Nivix.Models;

namespace Nivix.ViewModels
{
    public class CardsViewModel : ViewModelBase
    {
        [RelatedProperty("CardNicknames")]
        private IList<CardNickname> _CardNicknames;
        [RelatedProperty("DataIsDirty")]
        private bool _DataIsDirty;
        [RelatedProperty("SelectedCard")]
        private CardNickname _SelectedCard;
        [RelatedProperty("SelectedCardName")]
        private string _SelectedCardName;
        [RelatedProperty("SelectedNicknames")]
        private string[] _SelectedNicknames;

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

        public CardNickname SelectedCard
        {
            get { return _SelectedCard; }
            set { 
                ChangeProperty<CardsViewModel>(c => c.SelectedCard, value);
                SelectedCardName = value.Name;
                SelectedNicknames = value.Nicknames;
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
            set { ChangeProperty<CardsViewModel>(c => c.SelectedNicknames, value); }
        }

        public CardsViewModel()
        {
            DataIsDirty = false;
            LoadData();
        }

        private BindingList<CardNickname> GetNicknameData()
        {
            BindingList<CardNickname> bindingList = new BindingList<CardNickname>();
            foreach(CardNickname nick in DataBeast.GetCardNicknames()) {
                bindingList.Add(nick);
            }

            return bindingList;
        }

        private void LoadData()
        {
            BindingList<CardNickname> data = GetNicknameData();
            data.ListChanged += (theList, somethingHappenedWithIt) => {
                DataIsDirty = !(data.Equals(CardNicknames));
            };
            CardNicknames = data;
        }
    }
}