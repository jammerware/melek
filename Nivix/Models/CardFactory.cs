using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<TransformCard> _UnresolvedTransformers = new List<TransformCard>();

        public IDictionary<string, ICard> Cards { get; private set; }
        public IDictionary<string, string[]> CardNicknames { get; set; }
        public IDictionary<string, SetData> SetMetaData { get; set; }
        public IDictionary<string, Set> Sets { get; set; }

        public CardFactory()
        {
            Cards = new Dictionary<string, ICard>();
        }

        public ICard AddCardData(XElement cardData)
        {
            // declare some generally useful things
            ICard retVal = null;
            string name = XMLPal.GetString(cardData.Element("name")).Trim();
            string text = XMLPal.GetString(cardData.Element("ability")).Trim();

            // flip cards (like Erayo, Soratami Ascendant) have ——— in their text
            // split cards (like Beck // Call) have // in their name
            // transforming cards (like Huntmaster of the Fells) have a back_id property in the source db

            if(cardData.Element("back_id") != null) {
                // TRANSFORMERS - MORE THAN MEETS THE EYE
                TransformCard card = new TransformCard();
                SetICardProperties(card, cardData);
                retVal = card;
            }
            else if (name.Contains(" // ")) {
                // split card

            }
            else if (text.Contains("———")) {
                // flip card
            }
            else {
                // reg'lar card, y'all
                Card card = Cards.ContainsKey(name) ? Cards[name] as Card : null;
                if (card == null) {
                    card = new Card();
                    SetICardProperties(card, cardData);

                    string costData = XMLPal.GetString(cardData.Element("manacost"));
                    string typeData = XMLPal.GetString(cardData.Element("type"));

                    card.CardTypes = GetTypesFromTypeData(typeData);
                    card.Cost = string.IsNullOrEmpty(costData) ? null : new CardCostCollection(costData);
                    card.Power = XMLPal.GetInt(cardData.Element("power"));
                    card.Text = XMLPal.GetString(cardData.Element("ability"));
                    card.Toughness = XMLPal.GetInt(cardData.Element("toughness"));
                    card.Tribes = GetTribesFromTypeData(typeData);

                    Cards.Add(name, card);
                }

                // printing
                Printing printing = new Printing();
                SetIPrintingProperties(printing, cardData);

                printing.Artist = XMLPal.GetString(cardData.Element("artist"));
                printing.FlavorText = XMLPal.GetString(cardData.Element("flavor"));

                card.Printings.Add(printing);
            }

            return retVal;
        }

        private void SetICardProperties(ICard card, XElement cardData)
        {
            // legal formats
            List<Format> legalFormats = new List<Format>();
            if (XMLPal.GetString(cardData.Element("legality_Standard")) == "v") {
                legalFormats.Add(Format.Standard);
            }
            if (XMLPal.GetString(cardData.Element("legality_Modern")) == "v") {
                legalFormats.Add(Format.Modern);
            }
            if (XMLPal.GetString(cardData.Element("legality_Legacy")) == "v") {
                legalFormats.Add(Format.Legacy);
            }
            if (XMLPal.GetString(cardData.Element("legality_Vintage")) == "v") {
                legalFormats.Add(Format.Vintage);
            }

            string commanderLegalityValue = XMLPal.GetString(cardData.Element("legality_Commander"));
            if (commanderLegalityValue == "g" || commanderLegalityValue == "v") {
                legalFormats.Add(Format.Commander);
                if (commanderLegalityValue == "v") {
                    legalFormats.Add(Format.CommanderGeneral);
                }
            }

            // rulings
            string rulingsData = XMLPal.GetString(cardData.Element("ruling"));
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
            card.Name = XMLPal.GetString(cardData.Element("name")).Trim();
            card.Rulings = rulings;

            if (CardNicknames.Keys.Contains(card.Name)) {
                card.Nicknames = CardNicknames[card.Name].ToList();
            }
        }

        private void SetIPrintingProperties(IPrinting printing, XElement cardData)
        {
            string rarityData = XMLPal.GetString(cardData.Element("rarity"));
            if (string.IsNullOrEmpty(rarityData)) { rarityData = "C"; }

            string setData = cardData.Element("set").Value;
            SetData match = SetMetaData.Values.Where(s => s.GathererCode == setData).FirstOrDefault();
            if (match != null) { setData = match.Code; }

            printing.MultiverseId = XMLPal.GetString(cardData.Element("id"));
            printing.Rarity = StringToCardRarityConverter.GetRarity(rarityData);
            printing.Set = Sets[setData];
            printing.Watermark = XMLPal.GetString(cardData.Element("watermark"));
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