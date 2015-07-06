using Bazam.Modules;
using Melek.Client.Models;

namespace Melek.Client.Utilities
{
    public static class StringToCardRarityConverter
    {
        public static CardRarity GetRarity(string input)
        {
            if (string.IsNullOrEmpty(input)) return CardRarity.Common;
            if (input.Length == 1) {
                switch (input.ToLower()) {
                    case "u":
                        return CardRarity.Uncommon;
                    case "r":
                        return CardRarity.Rare;
                    case "m":
                        return CardRarity.MythicRare;
                    default:
                        return CardRarity.Common;
                }
            }

            return EnuMaster.Parse<CardRarity>(input, true);
        }
    }
}