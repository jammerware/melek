using System.Collections.Generic;

namespace Melek.Domain
{
    public class Card : CardBase
    {
        public CardCostCollection Cost { get; set; }
        public int? Power { get; set; }
        public string Text { get; set; }
        public int? Toughness { get; set; }
        public IReadOnlyList<string> Tribes { get; set; }
        public IReadOnlyList<CardType> Types { get; set; }

        public Card() : base() 
        {
            Tribes = new List<string>();
            Types = new List<CardType>();
        }

        #region enforced by CardBase
        protected override IReadOnlyList<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { Cost }; }
        }

        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}