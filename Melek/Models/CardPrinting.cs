namespace Melek.Models
{
    public class CardPrinting
    {
        public string Artist { get; set; }
        public string FlavorText { get; set; }
        public string MultiverseID { get; set; }
        public CardRarity Rarity { get; set; }
        public Set Set { get; set; }
        public string TransformsToMultiverseID { get; set; }
    }
}