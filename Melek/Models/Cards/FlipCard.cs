using System.Collections.Generic;

namespace Melek.Models
{
    public class FlipCard : CardBase<FlipPrinting>
    {
        public CardCostCollection Cost { get; set; }

        // names, types, & tribes
        public string FlippedName { get; set; }
        public IReadOnlyList<CardType> NormalCardTypes { get; set; }
        public IReadOnlyList<string> NormalTribes { get; set; }
        public IReadOnlyList<CardType> FlippedCardTypes { get; set; }
        public IReadOnlyList<string> FlippedTribes { get; set; }

        // p/t
        public int? NormalPower { get; set; }
        public int? NormalToughness { get; set; }
        public int? FlippedPower { get; set; }
        public int? FlippedToughness { get; set; }

        // text
        public string NormalText { get; set; }
        public string FlippedText { get; set; }

        public FlipCard() : base()
        {
            NormalCardTypes = new List<CardType>();
            NormalTribes = new List<string>();
            FlippedCardTypes = new List<CardType>();
            FlippedTribes = new List<string>();

            Printings = new List<FlipPrinting>();
        }

        #region enforced by CardBase<T>
        protected override IEnumerable<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { Cost }; }
        }

        public override IList<FlipPrinting> Printings { get; set; }
        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}