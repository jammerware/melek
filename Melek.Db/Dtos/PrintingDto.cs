using Melek.Models;

namespace Melek.Db.Dtos
{
    public class PrintingDto
    {
        // database biz
        public int Id { get; set; }

        // base properties
        public string Artist { get; set; }
        public string FlavorText { get; set; }
        public string MultiverseId { get; set; }
        public CardRarity Rarity { get; set; }
        public SetDto Set { get; set; }
        public string Watermark { get; set; }

        // "second" for flipped/split/transform
        public string SecondArtist { get; set; }
        public string SecondFlavorText { get; set; }
        public string SecondMultiverseId { get; set; }
    }
}