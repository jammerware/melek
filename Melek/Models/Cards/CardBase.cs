using System.Collections.Generic;
using System.Linq;
using Bazam.Slugging;
using Newtonsoft.Json;

namespace Melek
{
    public abstract class CardBase : ICard, ISluggable
    {
        // stock properties
        public IReadOnlyList<Format> LegalFormats { get; set; } = new List<Format>();
        public string Name { get; set; }
        public IList<string> Nicknames { get; } = new List<string>();
        public IReadOnlyList<Ruling> Rulings { get; set; } = new List<Ruling>();

        // abstract properties
        [JsonIgnore]
        public abstract IReadOnlyList<CardCostCollection> AllCosts { get; }
        [JsonIgnore]
        public abstract IReadOnlyList<string> AllTribes { get; }
        [JsonIgnore]
        public abstract IReadOnlyList<CardType> AllTypes { get; }
        public IList<IPrinting> Printings { get; set; } = new List<IPrinting>();

        public int GetConvertedManaCost()
        {
            return AllCosts.Sum(c => c.GetConvertedManaCost());
        }

        public abstract bool IsColor(MagicColor color);
        public bool IsColors(IEnumerable<MagicColor> colors)
        {
            if (colors.Contains(MagicColor.Colorless) && colors.Any(c => c != MagicColor.Colorless)) {
                // this is logically impossible, sorry friend
                return false;
            }

            foreach (MagicColor color in colors) {
                if (!IsColor(color)) {
                    return false;
                }
            }

            return true;
        }

        public bool IsMulticolored()
        {
            List<MagicColor> colors = new List<MagicColor>();
            foreach (CardCostCollection cost in AllCosts) {
                colors.AddRange(cost.GetColors());
            }

            return colors.Where(c => c == MagicColor.Colorless).Count() > 1;
        }

        /// <summary>
        /// Gets the last non-promo printing of this card (or the last printing if there aren't any non-promo ones).
        /// </summary>
        /// <returns></returns>
        public IPrinting GetLastPrinting()
        {
            IPrinting lastPrinting = Printings.OrderByDescending(p => p.Set.Date).FirstOrDefault(p => !p.Set.IsPromo);
            if (lastPrinting == null) lastPrinting = Printings.FirstOrDefault();

            return lastPrinting;
        }

        #region ISluggable
        public string GetSlugBase()
        {
            return Name;
        }
        #endregion
    }
}