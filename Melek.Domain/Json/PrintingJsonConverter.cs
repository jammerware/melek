using System;
using Melek.Domain;
using Newtonsoft.Json.Linq;

namespace Melek.Domain.Json
{
    public class PrintingJsonConverter : JsonCreationConverter<IPrinting>
    {
        protected override Type GetType(Type objectType, JObject jObject)
        {
            // Flippers don't have flavor text, it's the only way we can tell them apart from 
            if (jObject.Property("FlavorText") == null) {
                return typeof(FlipPrinting);
            }
            else if (jObject.Property("LeftArtist") != null) {
                return typeof(SplitPrinting);
            }
            else if (jObject.Property("TransformedArtist") != null) {
                return typeof(TransformPrinting);
            }
            return typeof(Printing);
        }
    }
}
