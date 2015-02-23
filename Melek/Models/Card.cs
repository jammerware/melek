using System;
using System.Collections.Generic;
using System.Linq;
using Bazam.Slugging;

namespace Melek.Models
{
    public class Card : Slugger
    {
        private static readonly List<CardCostType> COLORLESS_COSTS = new List<CardCostType>() {
            CardCostType._,
            CardCostType.X
        };

        public CardType[] CardTypes { get; set; }
        public CardCostCollection Cost { get; set; }
        public string Name { get; set; }
        public string[] Nicknames { get; set; }
        public int? Power { get; set; }
        public List<CardPrinting> Printings { get; set; }
        public string Text { get; set; }
        public int? Toughness { get; set; }
        public string Tribe { get; set; }
        public string Watermark { get; set; }

        public Card()
        {
            Printings = new List<CardPrinting>();
        }

        protected override string SlugBase()
        {
            return Name;
        }

        public Card Copy()
        {
            return this.MemberwiseClone() as Card;
        }

        public bool IsColor(MagicColor color)
        {
            if(color == MagicColor.COLORLESS) {
                return Cost.Count == 0 || Cost.All(cost => COLORLESS_COSTS.Contains(cost.Type));
            }
            return Cost.ToString().Contains(color.ToString());
        }

        public bool IsColors(IEnumerable<MagicColor> colors)
        {
            if (colors.Contains(MagicColor.COLORLESS) && colors.Any(c => c != MagicColor.COLORLESS)) {
                // this is logically impossible, sorry bud
                return false;
            }

            foreach (MagicColor color in colors) {
                if (!IsColor(color)) {
                    return false;
                }
            }

            return true;
        }
    }
}