using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bazam.Modules;
using Melek.Client.Utilities;
using Melek.Domain;

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
            string name = XmlPal.GetString(cardData.Element("name"));
            string text = XmlPal.GetString(cardData.Element("ability"));

#if DEBUG
            Console.WriteLine(name);
#endif

            // With the gatherer update in 2014, Wizards stopped using hard hyphens (the subtraction sign on every keyboard) and instead
            // replaced them with ASCII character 8722, which http://www.ascii.cl/htmlcodes.htm describes as a "soft hyphen." BECAUSE
            // THAT FUCKING MAKES SENSE. How do they even enter the data in the DB with a character that isn't on a keyboard?
            // ... GOD. Just replace them with the normal hyphen sign, which was good enough for our parents.
            text = text.Replace((char)SOFT_HYPHEN_CODE, '-');

            // flip cards (like Erayo, Soratami Ascendant) have ——— in their text
            // split cards (like Beck // Call) have // in their name
            // transforming cards (like Huntmaster of the Fells) have a back_id property in the source db
            // yes, this is gross
            if(cardData.Element("back_id") != null && !string.IsNullOrEmpty(XmlPal.GetString(cardData.Element("back_id")))) {
                // TRANSFORMERS - MORE THAN MEETS THE EYE
                TransformCard card = Cards.ContainsKey(name) ? Cards[name] as TransformCard : null;
                // some logic branches need this and i forget which, also i'm tired
                string costData = XmlPal.GetString(cardData.Element("manacost"));

                if (card == null) {
                    // this can either be a totally new transformer, or it can be the other half of one we've already 
                    // started but haven't finished
                    string transformsInto = XmlPal.GetString(cardData.Element("back_id"));
                    TransformCard existingTransformer = _UnresolvedTransformers.Where(t => t.Printings.Any(p => p.MultiverseId == transformsInto)).FirstOrDefault();

                    if (existingTransformer != null) {
                        // we've started in this card, we just need to know whether we found the front or back last time
                        bool frontDone = (existingTransformer.Cost != null);

                        if (!frontDone) {
                            SetTransformerProperties(existingTransformer, cardData);
                        }
                        else {
                            SetTransformerProperties(existingTransformer, cardData, false);
                        }

                        _UnresolvedTransformers.Remove(existingTransformer);
                        Cards.Add(name, existingTransformer);
                        card = existingTransformer;
                    }
                    else {
                        // this is a totally new transformer
                        card = new TransformCard();
                        SetICardProperties(card, cardData, name);
                        
                        // you can't cast a transforming card transformed, so the transformed side has no cost.
                        if (!string.IsNullOrEmpty(costData)) {
                            // this is the "normal" side
                            SetTransformerProperties(card, cardData);
                        }
                        else {
                            // this is the transformed side
                            SetTransformerProperties(card, cardData, false);
                        }

                        _UnresolvedTransformers.Add(card);
                    }
                }

                // this is a printing for an existing transformer
                TransformPrinting printing = null;
                // EXCEPT we have to make sure we haven't already created it 
                string setCode = GetSetFromGathererCode(XmlPal.GetString(cardData.Element("set"))).Code;
                printing = card.Printings.Where(p => p.Set.Code == setCode).FirstOrDefault();

                if (printing == null) {
                    printing = new TransformPrinting();
                    SetIPrintingProperties(printing, cardData);
                    card.Printings.Add(printing);
                }

                if (!string.IsNullOrEmpty(costData)) {
                    printing.NormalArtist = XmlPal.GetString(cardData.Element("artist"));
                    printing.NormalFlavorText = XmlPal.GetString(cardData.Element("flavor"));
                }
                else {
                    printing.TransformedArtist = XmlPal.GetString(cardData.Element("artist"));
                    printing.TransformedFlavorText = XmlPal.GetString(cardData.Element("flavor"));
                    printing.TransformedMultiverseId = XmlPal.GetString(cardData.Element("id"));
                }
            }
            else if (name.Contains(" // ")) {
                // split card
                // unlike other types, most fields in a split card have two values divided by " // ".
                string divider = " // ";
                SplitCard card = Cards.ContainsKey(name) ? Cards[name] as SplitCard : null;

                if (card == null) {
                    card = new SplitCard();
                    SetICardProperties(card, cardData, name);

                    string costData = XmlPal.GetString(cardData.Element("manacost"));
                    string typeData = XmlPal.GetString(cardData.Element("type"));
                    string textData = XmlPal.GetString(cardData.Element("ability"));

                    // right now we're assuming the type of each half is the same (because they all are as of now).
                    typeData = typeData.Substring(0, typeData.IndexOf(divider));
                    string leftCostData = costData.Substring(0, costData.IndexOf(divider)).Trim();
                    string rightCostData = costData.Substring(costData.IndexOf(divider) + divider.Length).Trim();

                    card.HasFuse = Regex.IsMatch(textData, @"\bFuse\b");
                    card.LeftCost = new CardCostCollection(leftCostData);
                    card.RightCost = new CardCostCollection(rightCostData);
                    card.LeftText = textData.Substring(0, textData.IndexOf(divider));
                    card.RightText = textData.Substring(textData.IndexOf(divider) + divider.Length);
                    card.Name = XmlPal.GetString(cardData.Element("name"));
                    card.Type = GetTypesFromTypeData(typeData).First();

                    Cards.Add(name, card);
                }

                SplitPrinting printing = new SplitPrinting();
                SetIPrintingProperties(printing, cardData);

                string artistData = XmlPal.GetString(cardData.Element("artist"));
                // some promotional printings seem to have regular artist data - no one knows why
                if (artistData.IndexOf(divider) >= 0) {
                    printing.LeftArtist = artistData.Substring(0, artistData.IndexOf(divider)).Trim();
                    printing.RightArtist = artistData.Substring(artistData.IndexOf(divider) + divider.Length).Trim();
                }
                else {
                    printing.LeftArtist = artistData;
                    printing.RightArtist = artistData;
                }

                card.Printings.Add(printing);
            }
            // curse of the fire penguin is the only flip card that doesn't have a name for each side. WOT. come back to this.
            else if (text.Contains("———") && name != "Curse of the Fire Penguin") {
                // flip card
                FlipCard card = Cards.ContainsKey(name) ? Cards[name] as FlipCard : null;
                if (card == null) {
                    card = new FlipCard();
                    SetICardProperties(card, cardData, name);

                    string costData = XmlPal.GetString(cardData.Element("manacost"));
                    string typeData = XmlPal.GetString(cardData.Element("type"));

                    // cost
                    card.Cost = string.IsNullOrEmpty(costData) ? null : new CardCostCollection(costData);

                    // normal (unflipped) 
                    // resolve text
                    string cardText = XmlPal.GetString(cardData.Element("ability"));

                    card.Name = XmlPal.GetString(cardData.Element("name"));
                    card.NormalTypes = GetTypesFromTypeData(typeData);
                    card.NormalPower = XmlPal.GetInt(cardData.Element("power"));
                    card.NormalText = cardText.Substring(0, cardText.IndexOf("———")).Trim();
                    card.NormalToughness = XmlPal.GetInt(cardData.Element("toughness"));
                    card.NormalTribes = GetTribesFromTypeData(typeData);

                    // flipped is trickier
                    string rawFlippedText = cardText.Substring(cardText.IndexOf("———") + 3).Trim('\\', 'n');
                    string[] lines = rawFlippedText.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string flippedTypeData = lines[1];
                    Match ptMatch = Regex.Match(lines[lines.Length - 1], @"(?<power>[\d+])/(?<toughness>[\d+])");
                    StringBuilder textBuilder = new StringBuilder();

                    card.FlippedName = lines[0];
                    card.FlippedTypes = GetTypesFromTypeData(flippedTypeData);
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
                    Cards.Add(name, card as ICard<IPrinting>);
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
                    SetICardProperties(card, cardData, name);

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

                    card.Types = GetTypesFromTypeData(typeData);
                    card.Cost = string.IsNullOrEmpty(costData) ? null : new CardCostCollection(costData);
                    card.Name = XmlPal.GetString(cardData.Element("name"));
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
            }
            // need to work this out somewhere along the way
            //List<CardType> cardTypes = GetCardTypes(cardTypesData);
            //if (legalFormats.Contains(Format.CommanderGeneral) && !(cardTypes.Contains(CardType.LEGENDARY) && cardTypes.Contains(CardType.CREATURE))) {
            //    legalFormats.Remove(Format.CommanderGeneral);
            //}
        }

        private Set GetSetFromGathererCode(string gathererCode)
        {
            string setData = gathererCode;
            SetData match = SetMetaData.Values.Where(s => s.GathererCode == setData).FirstOrDefault();
            if (match != null) { setData = match.Code; }

            return Sets[setData];
        }

        private ICollection<string> GetTribesFromTypeData(string typeData)
        {
            List<string> retVal = new List<string>();
            typeData = typeData.ToUpper();

            if (typeData.IndexOf('—') >= 0) {
                typeData = typeData.Substring(typeData.IndexOf('—') + 1).Trim();
                retVal.AddRange(Regex.Split(typeData, @"\s+"));
            }

            return retVal;
        }

        private ICollection<CardType> GetTypesFromTypeData(string typeData)
        {
            List<CardType> retVal = new List<CardType>();
            typeData = typeData.ToUpper();

            if (typeData.IndexOf('—') >= 0) {
                typeData = typeData.Substring(0, typeData.IndexOf('—'));
            }
            string[] splitInput = typeData.Split(new Char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (typeData.Contains("ENCHANT ")) {
                retVal.Add(CardType.Enchantment);
            }
            else if (typeData.Contains("SCHEME")) {
                retVal.Add(CardType.Scheme);
            }
            else {
                foreach (string inputPiece in splitInput) {
                    if (inputPiece == "SUMMON" || inputPiece == "EATURECRAY") {
                        retVal.Add(CardType.Creature);
                    }
                    else if (inputPiece == "INTERRUPT") {
                        retVal.Add(CardType.Instant);
                    }
                    else {
                        retVal.Add(EnuMaster.Parse<CardType>(inputPiece.ToUpper().Trim(), true));
                    }
                }
            }

            return retVal;
        }

        private void SetICardProperties(ICard card, XElement cardData, string name)
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
            card.Rulings = rulings;

            if (CardNicknames.Keys.Contains(name)) {
                card.Nicknames = CardNicknames[name].ToList();
            }
        }

        private void SetIPrintingProperties(IPrinting printing, XElement cardData)
        {
            string rarityData = XmlPal.GetString(cardData.Element("rarity"));
            if (string.IsNullOrEmpty(rarityData)) { rarityData = "C"; }
            // this is a bit of a cheat - the split cards currently come out with rarity like "U // U" and stuff,
            // but since it's impossible for one half of a card to be more rare than the other...
            rarityData = rarityData.Substring(0, 1);

            printing.MultiverseId = XmlPal.GetString(cardData.Element("id"));
            printing.Rarity = StringToCardRarityConverter.GetRarity(rarityData);
            printing.Set = GetSetFromGathererCode(XmlPal.GetString(cardData.Element("set")));
            printing.Watermark = XmlPal.GetString(cardData.Element("watermark"));
        }

        private void SetTransformerProperties(TransformCard card, XElement cardData, bool setFront = true)
        {
            string name = XmlPal.GetString(cardData.Element("name"));
            string costData = XmlPal.GetString(cardData.Element("manacost"));
            string typeData = XmlPal.GetString(cardData.Element("type"));

            // p/t can be weird things like *
            int? power = null;
            int? toughness = null;

            if (cardData.Element("power") != null) {
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
            }

            if (setFront) {
                card.Cost = string.IsNullOrEmpty(costData) ? null : new CardCostCollection(costData);
                card.NormalTypes = GetTypesFromTypeData(typeData);
                card.NormalPower = power;
                card.NormalToughness = toughness;
                card.NormalText = XmlPal.GetString(cardData.Element("ability"));
                card.NormalTribes = GetTribesFromTypeData(typeData);
                card.Name = name;
            }
            else {
                card.TransformedTypes = GetTypesFromTypeData(typeData);
                card.TransformedName = name;
                card.TransformedPower = power;
                card.TransformedToughness = toughness;
                card.TransformedText = XmlPal.GetString(cardData.Element("ability"));
                card.TransformedTribes = GetTribesFromTypeData(typeData);
            }
        }
    }
}