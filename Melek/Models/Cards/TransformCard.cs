using System.Collections.Generic;

namespace Melek.Models
{
    public class TransformCard : CardBase<TransformPrinting>
    {
        // general
        public CardCostCollection Cost { get; set; }

        // front
        public int? NormalPower { get; set; }
        public string NormalText { get; set; }
        public int? NormalToughness { get; set; }
        public ICollection<string> NormalTribes { get; set; }
        public ICollection<CardType> NormalTypes { get; set; }

        // back
        public string TransformedName { get; set; }
        public int? TransformedPower { get; set; }
        public string TransformedText { get; set; }
        public int? TransformedToughness { get; set; }
        public ICollection<string> TransformedTribes { get; set; }
        public ICollection<CardType> TransformedTypes { get; set; }

        #region enforced by CardBase<T>
        protected override ICollection<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { Cost }; }
        }

        public override ICollection<TransformPrinting> Printings { get; set; }
        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}