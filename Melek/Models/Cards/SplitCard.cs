using System.Collections.Generic;
namespace Melek.Models.Cards
{
    public class SplitCard : CardBase<SplitPrinting>, ICard<SplitPrinting>
    {
        // general
        public bool HasFuse { get; set; }
        // we gon' try this - currently all split cards have the same types on each half
        // if not we'll change it later
        public CardType Type { get; set; }

        // left
        public CardCostCollection LeftCost { get; set; }
        public string LeftText { get; set; }

        // right
        public CardCostCollection RightCost { get; set; }
        public string RightText { get; set; }

        #region enforced by CardBase<T>
        public override IList<SplitPrinting> Printings { get; set; }

        public override bool IsColor(MagicColor color)
        {
            return LeftCost.IsColor(color) || RightCost.IsColor(color);
        }
        #endregion
    }
}