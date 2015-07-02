﻿namespace Melek.Client.Models
{
    public class TransformPrinting : PrintingBase, IPrinting
    {
        public string NormalArtist { get; set; }
        public string NormalFlavorText { get; set; }

        public string TransformedArtist { get; set; }
        public string TransformedFlavorText { get; set; }
        public string TransformedMultiverseId { get; set; }
    }
}