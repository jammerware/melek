using System.Collections.Generic;

namespace Melek.Models
{
    public class TransformCard : CardBase<TransformPrinting>, ICard<TransformPrinting>
    {
        // general
        public CardCostCollection Cost { get; set; }

        // front
        public int? NormalPower { get; set; }
        public string NormalText { get; set; }
        public IReadOnlyList<CardType> NormalCardTypes { get; set; }
        public int? NormalToughness { get; set; }
        public IReadOnlyList<string> NormalTribes { get; set; }

        // back
        public CardType[] TransformedCardTypes { get; set; }
        public int? TransformedPower { get; set; }
        public string TransformedText { get; set; }
        public int? TransformedToughness { get; set; }
        public IReadOnlyList<string> TransformedTribes { get; set; }

        #region enforced by CardBase<T>
        protected override IEnumerable<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { Cost }; }
        }

        public override IList<TransformPrinting> Printings { get; set; }
        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}