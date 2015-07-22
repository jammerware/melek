using System.Collections.Generic;

namespace Melek.Domain
{
    public class Card : CardBase<Printing>
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
        }

        #region enforced by CardBase<T>
        protected override IReadOnlyList<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { Cost }; }
        }

        public override IReadOnlyList<Printing> Printings { get; set; }
        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}