using System;
using System.Text.RegularExpressions;
using Bazam.Modules;

namespace Melek.Models
{
    public class CardCost
    {
        public int Quantity { get; set; }
        public CardCostType Type { get; set; }

        public CardCost(string cost)
        {
            int quantity = 0;
            string numberString = string.Empty;
            string actionString = string.Empty;

            foreach (char c in cost) {
                if (Char.IsNumber(c)) {
                    numberString += c;
                }
            }

            if (numberString != string.Empty) {
                Int32.TryParse(numberString, out quantity);
                cost = cost.Replace(numberString, string.Empty);
            }

            CardCostType type = CardCostType._;
            if (cost != string.Empty) {
                try {
                    type = EnuMaster.Parse<CardCostType>(cost);
                }
                catch {
                    type = CardCostType.OTHER;
                }
            }

            Quantity = quantity;
            Type = type;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return (Quantity > 1 || Type == CardCostType._ ? Quantity.ToString() : string.Empty) + Type.ToString().Replace("_", string.Empty);
        }
    }
}