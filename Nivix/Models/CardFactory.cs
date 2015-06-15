using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bazam.Modules;
using Bazam.Modules.Enumerations;
using Melek.Models;
using Melek.Utilities;

namespace Nivix.Models
{
    public class CardFactory
    {
        private const int SOFT_HYPHEN_CODE = 8722;
        private List<TransformCard> _UnresolvedTransformers = new List<TransformCard>();

        public IDictionary<string, ICard> Cards { get; private set; }
        public IDictionary<string, string[]> CardNicknames { get; set; }
        public IDictionary<string, SetData> SetMetaData { get; set; }
        public IDictionary<string, Set> Sets { get; set; }

        public CardFactory()
        {
            Cards = new Dictionary<string, ICard>();
        }

        public void AddCardData(XElement cardData)
        {
            // declare some generally useful things
            string name = XmlPal.GetString(cardData.Element("name")).Trim();
            string text = XmlPal.GetString(cardData.Element("ability")).Trim();

            Console.WriteLine(name);

            // With the gatherer update in 2014, Wizards stopped using hard hyphens (the subtraction sign on every keyboard) and instead
            // replaced them with ASCII character 8722, which http://www.ascii.cl/htmlcodes.htm describes as a "soft hyphen." BECAUSE
            // THAT FUCKING MAKES SENSE. How do they even enter the data in the DB with a character that isn't on a keyboard?
            // ... GOD. Just replace them with the normal hyphen sign, which was good enough for our parents.
            text = text.Replace((char)SOFT_HYPHEN_CODE, '-');

            // flip cards (like Erayo, Soratami Ascendant) have ——— in their text
            // split cards (like Beck // Call) have // in their name
            // transforming cards (like Huntmaster of the Fells) have a back_id property in the source db
            if(cardData.Element("back_id") != null && !string.IsNullOrEmpty(XmlPal.GetString(cardData.Element("back_id")))) {
                // TRANSFORMERS - MORE THAN MEETS THE EYE
                TransformCard card = new TransformCard();
                SetICardProperties(card, cardData);
            }
            else if (name.Contains(" // ")) {
                // split card

            }
            else if (text.Contains("———")) {
                // flip card
                FlipCard card = Cards.ContainsKey(name) ? Cards[name] as FlipCard : null;
                if (card == null) {
                    card = new FlipCard();
                    SetICardProperties(card, cardData);

                    string costData = XmlPal.GetString(cardData.Element("manacost"));
                    string typeData = XmlPal.GetString(cardData.Element("type"));

                    // cost
                    card.Cost = string.IsNullOrEmpty(costData) ? null : new CardCostCollection(costData);

                    // normal (unflipped) 
                    // resolve text
                    string cardText = XmlPal.GetString(cardData.Element("ability"));

                    card.NormalCardTypes = GetTypesFromTypeData(typeData);
                    card.NormalPower = XmlPal.GetInt(cardData.Element("power"));
                    card.NormalText = cardText.Substring(0, cardText.IndexOf("———")).Trim();
                    card.NormalToughness = XmlPal.GetInt(cardData.Element("toughness"));
                    card.NormalTribes = GetTribesFromTypeData(typeData);

                    // flipped is trickier
                    string rawFlippedText = cardText.Substring(cardText.IndexOf("———") + 3).Trim();
                    string[] lines = rawFlippedText.Split(new char[] { '\\', 'n' }, StringSplitOptions.RemoveEmptyEntries);
                    string flippedTypeData = lines[1];
                    Match ptMatch = Regex.Match(lines[lines.Length - 1], @"(?<power>[\d+])/(?<toughness>[\d+])");
                    StringBuilder textBuilder = new StringBuilder();


                    card.FlippedName = lines[0];
                    card.FlippedCardTypes = GetTypesFromTypeData(flippedTypeData);
                    card.FlippedTribes = GetTribesFromTypeData(flippedTypeData);

                    if (ptMatch.Success) {
                        card.FlippedPower = int.Parse(ptMatch.Groups["power"].Value);
                        card.FlippedToughness = int.Parse(ptMatch.Groups["toughness"].Value);

                        for (int i = 2; i < lines.Length - 1; i++) {
                            textBuilder.AppendLine(lines[i]);
                        }
                    }
                    else {
                        for (int i = 2; i < lines.Length; i++) {
                            textBuilder.AppendLine(lines[i]);
            }
                    }

                    card.FlippedText = textBuilder.ToString();
                }

                FlipPrinting printing = new FlipPrinting();
                SetIPrintingProperties(printing, cardData);
                printing.Artist = XmlPal.GetString(cardData.Element("artist"));

                card.Printings.Add(printing);
            }
            else {
                // reg'lar card, y'all
                Card card = Cards.ContainsKey(name) ? Cards[name] as Card : null;
                if (card == null) {
                    card = new Card();
                    SetICardProperties(card, cardData);

                    string costData = XmlPal.GetString(cardData.Element("manacost"));
                    string typeData = XmlPal.GetString(cardData.Element("type"));

                    if (cardData.Element("power") != null) {
                        // p/t can be weird things like *
                        int? power = null;
                        int? toughness = null;
                        int powerParse = 0;
                        int toughnessParse = 0;
                        string sPower = XmlPal.GetString(cardData.Element("power"));
                        string sToughness = XmlPal.GetString(cardData.Element("toughness"));

                        if (int.TryParse(sPower, out powerParse)) {
                            power = powerParse;
                        }
                        if (int.TryParse(sToughness, out toughnessParse)) {
                            toughness = toughnessParse;
                        }

                        card.Power = power;
                        card.Toughness = toughness;
                    }

                    card.CardTypes = GetTypesFromTypeData(typeData);
                    card.Cost = string.IsNullOrEmpty(costData) ? null : new CardCostCollection(costData);
                    card.Text = XmlPal.GetString(cardData.Element("ability"));
                    card.Tribes = GetTribesFromTypeData(typeData);

                    Cards.Add(name, card);
                }

                // printing
                Printing printing = new Printing();
                SetIPrintingProperties(printing, cardData);

                printing.Artist = XmlPal.GetString(cardData.Element("artist"));
                printing.FlavorText = XmlPal.GetString(cardData.Element("flavor"));

                card.Printings.Add(printing);
                retVal = card;
            }
            // need to work this out somewhere along the way
            //List<CardType> cardTypes = GetCardTypes(cardTypesData);
            //if (legalFormats.Contains(Format.CommanderGeneral) && !(cardTypes.Contains(CardType.LEGENDARY) && cardTypes.Contains(CardType.CREATURE))) {
            //    legalFormats.Remove(Format.CommanderGeneral);
            //}
        }

        private void SetICardProperties(ICard card, XElement cardData)
        {
            // legal formats
            List<Format> legalFormats = new List<Format>();
            if (XmlPal.GetString(cardData.Element("legality_Standard")) == "v") {
                legalFormats.Add(Format.Standard);
            }
            if (XmlPal.GetString(cardData.Element("legality_Modern")) == "v") {
                legalFormats.Add(Format.Modern);
            }
            if (XmlPal.GetString(cardData.Element("legality_Legacy")) == "v") {
                legalFormats.Add(Format.Legacy);
            }
            if (XmlPal.GetString(cardData.Element("legality_Vintage")) == "v") {
                legalFormats.Add(Format.Vintage);
            }

            string commanderLegalityValue = XmlPal.GetString(cardData.Element("legality_Commander"));
            if (commanderLegalityValue == "g" || commanderLegalityValue == "v") {
                legalFormats.Add(Format.Commander);
                if (commanderLegalityValue == "v") {
                    legalFormats.Add(Format.CommanderGeneral);
                }
            }

            // rulings
            string rulingsData = XmlPal.GetString(cardData.Element("ruling"));
            List<Ruling> rulings = new List<Ruling>();
            if (!string.IsNullOrEmpty(rulingsData)) {
                foreach (string rulingData in rulingsData.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries)) {
                    Match match = Regex.Match(rulingData, @"(?<date>[0-9/]+)\s?:\s?(?<text>.+)$");

                    string dateText = match.Groups["date"].Value;
                    DateTime parseResult = DateTime.MinValue;
                    DateTime? finalDate = null;

                    if (DateTime.TryParse(dateText, out parseResult)) {
                        finalDate = parseResult;
                    }

                    rulings.Add(new Ruling() {
                        Date = finalDate,
                        Text = match.Groups["text"].Value
                    });
                }
            }

            card.LegalFormats = legalFormats;
            card.Name = XmlPal.GetString(cardData.Element("name")).Trim();
            card.Rulings = rulings;

            if (CardNicknames.Keys.Contains(card.Name)) {
                card.Nicknames = CardNicknames[card.Name].ToList();
            }
        }

        private void SetIPrintingProperties(IPrinting printing, XElement cardData)
        {
            string rarityData = XmlPal.GetString(cardData.Element("rarity"));
            if (string.IsNullOrEmpty(rarityData)) { rarityData = "C"; }

            string setData = cardData.Element("set").Value;
            SetData match = SetMetaData.Values.Where(s => s.GathererCode == setData).FirstOrDefault();
            if (match != null) { setData = match.Code; }

            printing.MultiverseId = XmlPal.GetString(cardData.Element("id"));
            printing.Rarity = StringToCardRarityConverter.GetRarity(rarityData);
            printing.Set = Sets[setData];
            printing.Watermark = XmlPal.GetString(cardData.Element("watermark"));
        }

        private IReadOnlyList<string> GetTribesFromTypeData(string typeData)
        {
            List<string> retVal = new List<string>();
            typeData = typeData.ToUpper();

            if (typeData.IndexOf('—') >= 0) {
                typeData = typeData.Substring(typeData.IndexOf('—') + 1).Trim();
                retVal.AddRange(Regex.Split(typeData, @"\s+"));
            }

            return retVal;
        }

        private IReadOnlyList<CardType> GetTypesFromTypeData(string typeData)
        {
            List<CardType> retVal = new List<CardType>();
            typeData = typeData.ToUpper();

            if (typeData.IndexOf('—') >= 0) {
                typeData = typeData.Substring(0, typeData.IndexOf('—'));
            }
            string[] splitInput = typeData.Split(new Char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (typeData.Contains("ENCHANT ")) {
                retVal.Add(CardType.ENCHANTMENT);
            }
            else if (typeData.Contains("SCHEME")) {
                retVal.Add(CardType.SCHEME);
            }
            else {
                foreach (string inputPiece in splitInput) {
                    if (inputPiece == "EATURECRAY") {
                        retVal.Add(CardType.CREATURE);
                    }
                    else if (inputPiece == "INTERRUPT") {
                        retVal.Add(CardType.INSTANT);
                    }
                    else {
                        retVal.Add(EnuMaster.Parse<CardType>(inputPiece.ToUpper().Trim()));
                    }
                }
            }

            return retVal;
        }
    }
}