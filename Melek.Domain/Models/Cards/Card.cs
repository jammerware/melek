using System.Collections.Generic;

namespace Melek.Domain
{
    public class Card : CardBase<Printing>
    {
        public CardCostCollection Cost { get; set; }
        public int? Power { get; set; }
        public string Text { get; set; }
        public int? Toughness { get; set; }
        public ICollection<string> Tribes { get; set; }
        public ICollection<CardType> Types { get; set; }

        public Card() : base() 
        {
            Tribes = new List<string>();
        }

        #region enforced by CardBase<T>
        protected override ICollection<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { Cost }; }
        }

        public override ICollection<Printing> Printings { get; set; }
        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}