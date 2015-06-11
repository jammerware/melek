using System.Collections.Generic;
namespace Melek.Models
{
    public class FlipCard : CardBase<FlipPrinting>
    {
        public CardCostCollection Cost { get; set; }

        public FlipCard() : base()
        {
            Printings = new List<FlipPrinting>();
        }

        #region enforced by CardBase<T>
        public override IReadOnlyList<FlipPrinting> Printings { get; set; }
        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}