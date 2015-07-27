using System.Collections.Generic;
using System.Linq;
using Bazam.Slugging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Melek.Domain
{
    public abstract class CardBase : ICard, ISluggable
    {
        // stock properties
        public IReadOnlyList<Format> LegalFormats { get; set; }
        public string Name { get; set; }
        public IReadOnlyList<string> Nicknames { get; set; }
        public IReadOnlyList<Ruling> Rulings { get; set; }

        // abstract properties
        protected abstract IReadOnlyList<CardCostCollection> AllCosts { get; }
        public IList<IPrinting> Printings { get; set; }

        protected CardBase()
        {
            LegalFormats = new List<Format>();
            Printings = new List<IPrinting>();
            Rulings = new List<Ruling>();
        }

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