namespace Melek.Client.Models
{
    public abstract class PrintingBase : IPrinting
    {
        public string MultiverseId { get; set; }
        public CardRarity Rarity { get; set; }
        public Set Set { get; set; }
        public string Watermark { get; set; }
    }
}