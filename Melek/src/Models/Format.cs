using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Melek
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Format
    {
        Archenemy,
        Block,
        Commander,
        CommanderGeneral,
        Conspiracy,
        Legacy,
        Modern,
        Planechase,
        Standard,
        Vintage
    }
}