namespace Melek.Models
{
    public abstract class PrintingBase
    {
        public string MultiverseID { get; set; }
        public CardRarity Rarity { get; set; }
        public Set Set { get; set; }
        public string Watermark { get; set; }
    }
}