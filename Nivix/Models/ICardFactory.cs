using System.Collections.Generic;
using System.Xml.Linq;
using Bazam.Modules;
using Melek.Models;

namespace Nivix.Models
{
    public class ICardFactory
    {
        public List<Set> Sets { get; set; }

        public ICard GetCard(XElement cardData)
        {
            // declare some generally useful things
            ICard retVal = null;
            string name = XMLPal.GetString(cardData.Element("name"));
            string text = XMLPal.GetString(cardData.Element("ability"));

            // flip cards (like Erayo, Soratami Ascendant) have ——— in their text
            // split cards (like Beck // Call) have // in their name
            // transforming cards (like Huntmaster of the Fells) have a back_id property in the source db

            if(cardData.Element("back_id") != null) {
                // TRANSFORMERS - MORE THAN MEETS THE EYE
                TransformCard card = new TransformCard();
                retVal = card as ICard;
            }
            else if (name.Contains(" // ")) {
                // split card

            }
            else if (text.Contains("———")) {
                // flip card
            }
            else {
                // reg'lar card, y'all
                Card card = new Card();
                retVal = card as ICard;
            }

            return retVal;
        }
    }
}