namespace Melek.Domain
{
    public class TransformPrinting : PrintingBase
    {
        public string NormalArtist { get; set; }
        public string NormalFlavorText { get; set; }

        public string TransformedArtist { get; set; }
        public string TransformedFlavorText { get; set; }
        public string TransformedMultiverseId { get; set; }
    }
}