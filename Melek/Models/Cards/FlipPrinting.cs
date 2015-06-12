namespace Melek.Models
{
    public class FlipPrinting : PrintingBase, IPrinting
    {
        public string Artist { get; set; }
        public string FlavorText { get; set; }
    }
}