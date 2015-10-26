using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Melek.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardType
    {
        Artifact,
        Basic,
        Conspiracy,
        Creature,
        Enchantment,
        Instant,
        Land,
        Legendary,
        Phenomenon,
        Plane,
        Planeswalker,
        Scheme,
        Snow,
        Sorcery,
        Token,
        Tribal,
        Vanguard,
        World
    }
}