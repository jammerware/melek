using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Melek.Domain.Json
{
    public class PrintingJsonConverter : JsonCreationConverter<IPrinting>
    {
        // see http://stackoverflow.com/questions/12314438/self-referencing-loop-in-json-net-jsonserializer-from-custom-jsonconverter-web
        // for why this is necessary. 
        //
        // basically, json.net has a bug in it (at least, it seems like a bug) that prevents you from indicating that your converter can 
        // perform the serialization of an object if your custom serialization falls back on the default serializer in any cases. we create
        // our own serializer here (independent of the greater serialization process) to fall back on in the WriteJson method if we need it.
        private readonly JsonSerializer _IndependentSerializer = new JsonSerializer();

        protected override Type GetType(Type objectType, JObject jObject)
        {
            if (jObject.Property("TransformArtist") != null) {
                return typeof(TransformPrinting);
            }
            else if (jObject.Property("LeftArtist") != null) {
                return typeof(SplitPrinting);
            }
            else if (jObject.Property("IsFlipPrinting") != null) {
                return typeof(FlipPrinting);
            }
            
            return typeof(Printing);
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value as FlipPrinting != null) {
                FlipPrinting flipPrinting = value as FlipPrinting;

                writer.WriteStartObject();

                // write each property individually so we can add an additional one
                // TODO: maybe reflection so we don't miss any new properties?
                writer.WritePropertyName("MultiverseId");
                _IndependentSerializer.Serialize(writer, flipPrinting.MultiverseId);

                writer.WritePropertyName("Rarity");
                _IndependentSerializer.Serialize(writer, flipPrinting.Rarity);

                writer.WritePropertyName("Set");
                _IndependentSerializer.Serialize(writer, flipPrinting.Set);

                writer.WritePropertyName("Watermark");
                _IndependentSerializer.Serialize(writer, flipPrinting.Watermark);

                writer.WritePropertyName("Artist");
                _IndependentSerializer.Serialize(writer, flipPrinting.Artist);

                // now add a "ghost" property that isn't on the flipprinting object but will let us identify objects of this type
                // on deserialization
                writer.WritePropertyName("IsFlipPrinting");
                _IndependentSerializer.Serialize(writer, "true");

                writer.WriteEndObject();
            }
            else {
                _IndependentSerializer.Serialize(writer, value);
            }
        }
    }
}