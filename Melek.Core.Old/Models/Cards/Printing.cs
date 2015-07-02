namespace Melek.Models
{
    public class Printing : PrintingBase, IPrinting
    {
        public string Artist { get; set; }
        public string FlavorText { get; set; }
    }
}