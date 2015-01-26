using System.Collections.Generic;
using BazamWPF.ViewModels;
using Nivix.Infrastructure;
using Nivix.Models;

namespace Nivix.ViewModels
{
    public class CardsViewModel : ViewModelBase
    {
        [RelatedProperty("CardsWithNicknames")]
        private IList<CardNickname> _CardsWithNicknames;
        [RelatedProperty("SelectedCard")]
        private CardNickname _SelectedCard;
        [RelatedProperty("SelectedCardName")]
        private string _SelectedCardName;
        [RelatedProperty("SelectedNicknames")]
        private string[] _SelectedNicknames;

        public IList<CardNickname> CardsWithNicknames
        {
            get { return _CardsWithNicknames; }
            set { ChangeProperty<CardsViewModel>(c => c.CardsWithNicknames, value); }
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
            LoadCardsWithNicknames();
        }

        private void LoadCardsWithNicknames()
        {
            CardsWithNicknames = DataBeast.GetCardNicknames();
        }
    }
}