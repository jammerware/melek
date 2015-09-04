using System;
using System.Collections.Generic;

namespace Melek.Domain
{
    public class SplitCard : CardBase
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

        #region enforced by ICard<T>
        public override IReadOnlyList<CardCostCollection> AllCosts
        {
            get
            {
                List<CardCostCollection> costs = null;

                if (LeftCost != null || RightCost != null) {
                    costs = new List<CardCostCollection>();

                    if (LeftCost != null) costs.Add(LeftCost);
                    if (RightCost != null) costs.Add(RightCost);
                }

                return costs;
            }
        }

        public override IReadOnlyList<string> AllTribes
        {
            get { return null; }
        }

        public override IReadOnlyList<CardType> AllTypes
        {
            get { return new CardType[] { Type }; }
        }

        public override bool IsColor(MagicColor color)
        {
            return LeftCost.IsColor(color) || RightCost.IsColor(color);
        }
        #endregion
    }
}