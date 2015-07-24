using Melek.Domain;

namespace Melek.Client.Utilities
{
    public static class StringToCardRarityConverter
    {
        public static CardRarity? GetRarity(string input)
        {
            if (input?.Length >= 1) {
                switch (input.ToLower()) {
                    case "c":
                    case "common":
                        return CardRarity.Common;
                    case "u":
                    case "uncommon":
                        return CardRarity.Uncommon;
                    case "r":
                    case "rare":
                        return CardRarity.Rare;
                    case "m":
                    case "mythic":
                    case "mythic rare":
                        return CardRarity.MythicRare;
                }
            }

            return null;
        }
    }
}