using System.Collections.Generic;

namespace Melek.Models
{
    public class Card : CardBase<Printing>, ICard<Printing>
    {
        public IReadOnlyList<CardType> CardTypes { get; set; }
        public CardCostCollection Cost { get; set; }
        public int? Power { get; set; }
        public string Text { get; set; }
        public int? Toughness { get; set; }
        public IReadOnlyList<string> Tribes { get; set; }

        public Card() : base() 
        {
            Tribes = new List<string>();
        }

        #region enforced by CardBase<T>
        public override IList<Printing> Printings { get; set; }
        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}