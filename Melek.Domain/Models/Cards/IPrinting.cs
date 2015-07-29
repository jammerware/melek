using Melek.Domain.Json;
using Newtonsoft.Json;

namespace Melek.Domain
{
    [JsonConverter(typeof(PrintingJsonConverter))]
    public interface IPrinting
    {
        string MultiverseId { get; set; }
        CardRarity Rarity { get; set; }
        Set Set { get; set; }
        string Watermark { get; set; }
    }
}