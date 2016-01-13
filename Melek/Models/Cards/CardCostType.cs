using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Melek
{
    // P[SOME LETTER] is phyrexian
    // _ is colorless, we'll see how that goes
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardCostType
    {
        _,
        B,
        BR,
        BG,
        C,
        CHAOS,
        G,
        GU,
        GW,
        HW,
        OTHER,
        PB,
        PG,
        PR,
        PU,
        PW,
        Q,
        R,
        RG,
        RW,
        S,
        T,
        U,
        UB,
        UR,
        W,
        WB,
        WU,
        X,
    }
}