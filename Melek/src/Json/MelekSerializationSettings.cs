using Newtonsoft.Json;

namespace Melek.Json
{
    public static class MelekSerializationSettings
    {
        public static JsonSerializerSettings Get()
        {
            // We're adding the PrintingJsonConverter here (used for de/serialization in Melek.Client and Melek.Api) for
            // very sad reasons.
            //
            // PrintingJsonConverter serializes all descendants of IPrinting in a default fashion except for FlipPrintings, which need
            // to have a special JSON property injected into them so that we can check for its existence on deserialization. If your custom
            // converter tries to serialize an object using the default serializer, a crazy bug happens (see the PrintingJsonConverter class
            // for details), so you have to create a new JsonSerializer inside your converter that is completely independent of the 
            // serialization process so false circular references don't get flagged.
            //
            // This is bad enough, but the problem is that if we decorate the PrintingBase class to indicate that objects of the type should
            // always be serialized with PrintingJsonConverter, a StackOverflowException occurs (because the serializer tries to call your 
            // converter to serialize the input, which causes the serializer to call your converter, which causes the serializer to call your
            // converter...). Thus, while the serializer in charge of the greater serialization process needs to use this converter to
            // serialize the input, but the lower-level one inside PrintingJsonConverter must not. Which means the use of the attribute is 
            // off the table, which means I'm very sad and I hate this and I'm pretty sure I took a wrong turn at some point.
            //
            // TODO: FIX IT.
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new PrintingJsonConverter());
            settings.Formatting = Formatting.None;
            settings.NullValueHandling = NullValueHandling.Ignore;

            return settings;
        }
    }
}