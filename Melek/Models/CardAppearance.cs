using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melek.Models
{
    public class CardAppearance
    {
        public string Artist { get; set; }
        public string FlavorText { get; set; }
        public string MultiverseID { get; set; }
        public CardRarity Rarity { get; set; }
        public Set Set { get; set; }
    }
}
