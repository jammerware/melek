using System.Collections.Generic;

namespace Melek.Models
{
    public class FlipCard : CardBase<FlipPrinting>
    {
        public CardCostCollection Cost { get; set; }

        // names, types, & tribes
        public string FlippedName { get; set; }
        public ICollection<string> NormalTribes { get; set; }
        public ICollection<CardType> NormalTypes { get; set; }
        public ICollection<string> FlippedTribes { get; set; }
        public ICollection<CardType> FlippedTypes { get; set; }

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
            NormalTypes = new List<CardType>();
            NormalTribes = new List<string>();
            FlippedTypes = new List<CardType>();
            FlippedTribes = new List<string>();

            Printings = new List<FlipPrinting>();
        }

        #region enforced by CardBase<T>
        protected override ICollection<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { Cost }; }
        }

        public override ICollection<FlipPrinting> Printings { get; set; }
        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}