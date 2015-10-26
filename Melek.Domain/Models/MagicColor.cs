using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Melek.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MagicColor
    {
        B,
        Colorless,
        G,
        R,
        U,
        W
    }
}