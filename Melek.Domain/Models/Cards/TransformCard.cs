using System;
using System.Collections.Generic;

namespace Melek.Domain
{
    public class TransformCard : CardBase
    {
        // general
        public CardCostCollection Cost { get; set; }

        // front
        public int? NormalPower { get; set; }
        public string NormalText { get; set; }
        public int? NormalToughness { get; set; }
        public IReadOnlyList<string> NormalTribes { get; set; }
        public IReadOnlyList<CardType> NormalTypes { get; set; }

        // back
        public string TransformedName { get; set; }
        public int? TransformedPower { get; set; }
        public string TransformedText { get; set; }
        public int? TransformedToughness { get; set; }
        public IReadOnlyList<string> TransformedTribes { get; set; }
        public IReadOnlyList<CardType> TransformedTypes { get; set; }

        #region enforced by CardBase<T>
        public override IReadOnlyList<CardCostCollection> AllCosts
        {
            get { return new CardCostCollection[] { Cost }; }
        }

        public override IReadOnlyList<string> AllTribes
        {
            get
            {
                List<string> tribes = new List<string>(NormalTribes);
                tribes.AddRange(TransformedTribes);
                return tribes;   
            }
        }

        public override IReadOnlyList<CardType> AllTypes
        {
            get
            {
                List<CardType> cardTypes = new List<CardType>(NormalTypes);
                cardTypes.AddRange(TransformedTypes);
                return cardTypes;
            }
        }

        public override bool IsColor(MagicColor color)
        {
            return Cost.IsColor(color);
        }
        #endregion
    }
}