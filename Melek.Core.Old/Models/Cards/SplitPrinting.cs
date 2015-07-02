namespace Melek.Models
{
    public class SplitPrinting : PrintingBase, IPrinting
    {
        public string LeftArtist { get; set; }
        public string RightArtist { get; set; }
    }
}