using System;
using System.Collections.Generic;

namespace Melek
{
    public class FlipCard : CardBase
    {
        public CardCostCollection Cost { get; set; }

        // names, types, & tribes
        public string FlippedName { get; set; }
        public IReadOnlyList<string> NormalTribes { get; set; } = new List<string>();
        public IReadOnlyList<CardType> NormalTypes { get; set; } = new List<CardType>();
        public IReadOnlyList<string> FlippedTribes { get; set; } = new List<string>();
        public IReadOnlyList<CardType> FlippedTypes { get; set; } = new List<CardType>();

        // p/t
        public int? FlippedPower { get; set; }
        public int? FlippedToughness { get; set; }
        public int? NormalPower { get; set; }
        public int? NormalToughness { get; set; }

        // text
        public string NormalText { get; set; }
        public string FlippedText { get; set; }
        
        #region enforced by CardBase
        public override IReadOnlyList<CardCostCollection> AllCosts
        {
            get { return (Cost == null ? null : new CardCostCollection[] { Cost }); }
        }

        public override IReadOnlyList<string> AllTribes
        {
            get
            {
                List<string> tribes = null;

                if(FlippedTribes != null || NormalTribes != null) {
                    tribes = new List<string>();

                    tribes.AddRange(FlippedTribes);
                    tribes.AddRange(NormalTribes);
                }
                
                return tribes;
            }
        }

        public override IReadOnlyList<CardType> AllTypes
        {
            get
            {
                List<CardType> cardTypes = new List<CardType>(FlippedTypes);
                cardTypes.AddRange(NormalTypes);
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