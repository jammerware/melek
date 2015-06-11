using Melek.Models;

namespace Nivix.Models
{
    public class CardBaseFactory
    {
        public static ICard<PrintingBase> GetCard(string cardXml)
        {
            return new FlipCard();
        }
    }
}