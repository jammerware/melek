namespace Melek.Models.Cards
{
    public class TransformingCard : Card
    {
        public CardType[] TransformedCardTypes { get; set; }
        public string TransformedFlavorText { get; set; }
        public int? TransformedPower { get; set; }
        public string TransformedText { get; set; }
        public int? TransformedToughness { get; set; }
        public string TransformedTribe { get; set; }
    }
}