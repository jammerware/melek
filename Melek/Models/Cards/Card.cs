using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bazam.Modules.Enumerations;
using Bazam.Slugging;

namespace Melek.Models.Cards
{
    public class Card : ISluggable
    {
        private static readonly List<CardCostType> COLORLESS_COSTS = new List<CardCostType>() {
            CardCostType._,
            CardCostType.X
        };

        public bool IsColorless { get; private set; }
        public bool IsMulticolored { get; private set; }
        public CardType[] CardTypes { get; set; }
        public MagicColor[] Colors { get; set; }
        public Format[] LegalFormats { get; set; }
        public string Name { get; set; }
        public string[] Nicknames { get; set; }
        public int? Power { get; set; }
        public List<CardPrinting> Printings { get; set; }
        public Ruling[] Rulings { get; set; }
        public string Text { get; set; }
        public int? Toughness { get; set; }
        public string Tribe { get; set; }
        public string Watermark { get; set; }

        // need to listen for Cost getting set to compute color identity
        private CardCostCollection _Cost = null;
        public CardCostCollection Cost 
        {
            get { return _Cost; }
            set
            {
                _Cost = value;
                ComputeColorProperties();
            }
        }

        public Card()
        {
            LegalFormats = new Format[] { };
            Printings = new List<CardPrinting>();
            Rulings = new Ruling[] { };
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
            return Colors.Contains(color);
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

        /// <summary>
        /// Gets the last non-promo printing of this card (or the last printing if there aren't any non-promo ones).
        /// </summary>
        /// <returns></returns>
        public CardPrinting GetLastPrinting()
        {
            CardPrinting lastPrinting = Printings.OrderByDescending(p => p.Set.Date).FirstOrDefault(p => !p.Set.IsPromo);
            if (lastPrinting == null) lastPrinting = Printings.FirstOrDefault();

            return lastPrinting;
        }

        #region internal utility
        private void ComputeColorProperties()
        {
            if (Cost != null) {
                string cost = Cost.ToString();
                List<MagicColor> colors = new List<MagicColor>();

                foreach(Match match in Regex.Matches(cost, @"\{(?<Color>[WUBRG])\}")) {
                    MagicColor color = (EnuMaster.Parse<MagicColor>(match.Groups["Color"].Value));
                    if (!colors.Contains(color)) {
                        colors.Add(color);
                    }
                }

                if (colors.Count == 0) { this.Colors = new MagicColor[] { MagicColor.COLORLESS }; }
                else { this.Colors = colors.ToArray(); }

                IsColorless = (Colors.Length == 1 && Colors[0] == MagicColor.COLORLESS);
                IsMulticolored = Colors.Length > 1;
            }
            else {
                IsColorless = false;
                IsMulticolored = false;
                Colors = new MagicColor[] { };
            }
        }
        #endregion

        #region ISluggable
        public string GetSlugBase()
        {
            return Name;
        }
        #endregion
    }
}