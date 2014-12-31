using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Melek.Models
{
    public class CardCostCollection : List<CardCost>
    {
        private CardCostCollection()
        {
        }

        // {3}{B}
        public CardCostCollection(string cost)
        {
            IEnumerable<string> splits = Regex.Split(cost, "\\{(.+?)\\}").Where(d => d != string.Empty);

            foreach (string piece in splits) {
                this.Add(new CardCost(piece));
            }
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