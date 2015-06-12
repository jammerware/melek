namespace Melek.Models
{
    public interface IPrinting
    {
        string MultiverseId { get; set; }
        CardRarity Rarity { get; set; }
        Set Set { get; set; }
        string Watermark { get; set; }
    }
}