using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bazam.Modules.Enumerations;

namespace Melek.Models
{
    public class CardCostCollection : List<CardCost>
    {
        private static readonly List<CardCostType> COLORLESS_COSTS = new List<CardCostType>() {
            CardCostType._,
            CardCostType.X
        };

        // {3}{B}
        public CardCostCollection(string cost)
        {
            IEnumerable<string> splits = Regex.Split(cost, "\\{(.+?)\\}").Where(d => d != string.Empty);

            foreach (string piece in splits) {
                this.Add(new CardCost(piece));
            }
        }

        public bool IsColor(MagicColor color)
        {
            if(color == MagicColor.COLORLESS) {
                return !GetColors().Any(c => c != MagicColor.COLORLESS);
            }

            return GetColors().Contains(color);
        }

        public bool IsMulticolored()
        {
            return GetColors().Where(c => c != MagicColor.COLORLESS).Count() > 1;
        }

        public IEnumerable<MagicColor> GetColors()
        {
            string cost = this.ToString();
            List<MagicColor> colors = new List<MagicColor>();

            foreach (Match match in Regex.Matches(cost, @"\{(?<Color>[WUBRG])\}")) {
                MagicColor costColor = (EnuMaster.Parse<MagicColor>(match.Groups["Color"].Value));
                if (!colors.Contains(costColor)) {
                    colors.Add(costColor);
                }
            }

            return colors;
        }

        public int GetConvertedManaCost()
        {
            int total = 0;
            foreach (CardCost cost in this) {
                total += cost.Quantity;
            }

            return total;
        }

        public override string ToString()
        {
            StringBuilder retVal = new StringBuilder();
            foreach (CardCost cost in this) {
                retVal.Append("{" + cost.ToString() + "}");
            }

            return retVal.ToString();
        }
    }
}