﻿using System.Collections.Generic;

namespace Melek.Domain
{
    public class FlipCard : CardBase<FlipPrinting>
    {
        public CardCostCollection Cost { get; set; }

        // names, types, & tribes
        public string FlippedName { get; set; }
        public IReadOnlyList<string> NormalTribes { get; set; }
        public IReadOnlyList<CardType> NormalTypes { get; set; }
        public IReadOnlyList<string> FlippedTribes { get; set; }
        public IReadOnlyList<CardType> FlippedTypes { get; set; }

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
        protected override IReadOnlyList<CardCostCollection> AllCosts
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