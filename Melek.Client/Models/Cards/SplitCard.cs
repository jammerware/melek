﻿using System.Collections.Generic;

namespace Melek.Client.Models
{
    public class SplitCard : CardBase<SplitPrinting>
    {
        // general
        public bool HasFuse { get; set; }
        // we gon' try this - currently all split cards have the same types on each half
        // if that stops being true we'll change it later
        public CardType Type { get; set; }

        // left
        public CardCostCollection LeftCost { get; set; }
        public string LeftText { get; set; }

        // right
        public CardCostCollection RightCost { get; set; }
        public string RightText { get; set; }

        #region enforced by ICollection<T>
        protected override ICollection<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { LeftCost, RightCost }; }
        }

        public override ICollection<SplitPrinting> Printings { get; set; }

        public override bool IsColor(MagicColor color)
        {
            return LeftCost.IsColor(color) || RightCost.IsColor(color);
        }
        #endregion
    }
}