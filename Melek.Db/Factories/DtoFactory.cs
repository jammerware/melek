using System;
using System.Collections.Generic;
using System.Linq;
using Melek.Db.Dtos;
using Melek.Client.Models;

namespace Melek.Db.Factories
{
    public class DtoFactory
    {
        // TODO: is this a bad way to make sure the cards get tied to the right setdtos?
        private List<SetDto> _SetDtos = new List<SetDto>();

        #region Card
        public CardDto GetCardDto(Card card)
        {
            // from CardBase
            CardDto dto = FromCardBase<Printing>(card);

            // from Card
            dto.Cost = (card.Cost != null ? card.Cost.ToString() : null);
            dto.Power = card.Power;
            dto.Text = card.Text;
            dto.Toughness = card.Toughness;
            dto.Tribes = card.Tribes;
            dto.Types = card.Types;

            // printings
            foreach (Printing printing in card.Printings) {
                dto.Printings.Add(GetPrintingDto(printing));
            }

            return dto;
        }

        public PrintingDto GetPrintingDto(Printing printing)
        {
            // from PrintingBase
            PrintingDto dto = FromPrintingBase(printing);

            // from Printing
            dto.Artist = printing.Artist;
            dto.FlavorText = printing.FlavorText;

            return dto;
        }
        #endregion

        #region FlipCard
        public CardDto GetCardDto(FlipCard card)
        {
            // from CardBase
            CardDto dto = FromCardBase<FlipPrinting>(card);

            // from FlipCard
            dto.Cost = card.Cost.ToString();
            dto.Power = card.NormalPower;
            dto.Text = card.NormalText;
            dto.Toughness = card.NormalToughness;
            dto.Tribes = card.NormalTribes;
            dto.Types = card.NormalTypes;

            dto.SecondName = card.FlippedName;
            dto.SecondPower = card.FlippedPower;
            dto.SecondText = card.FlippedText;
            dto.SecondToughness = card.FlippedToughness;
            dto.SecondTribes = card.FlippedTribes;
            dto.SecondTypes = card.FlippedTypes;

            // printings
            foreach (FlipPrinting printing in card.Printings) {
                dto.Printings.Add(GetPrintingDto(printing));
            }

            return dto;
        }

        public PrintingDto GetPrintingDto(FlipPrinting printing)
        {
            PrintingDto dto = FromPrintingBase(printing);
            dto.Artist = printing.Artist;
            return dto;
        }
        #endregion

        #region SplitCard
        public CardDto GetCardDto(SplitCard card)
        {
            // from CardBase
            CardDto dto = FromCardBase<SplitPrinting>(card);

            // from SplitCard
            dto.HasFuse = card.HasFuse;
            dto.Types = new CardType[] { card.Type };

            dto.Cost = card.LeftCost.ToString();
            dto.Text = card.LeftText;
            dto.SecondCost = card.RightCost.ToString();
            dto.SecondText = card.RightText;

            // printings
            foreach (SplitPrinting printing in card.Printings) {
                dto.Printings.Add(GetPrintingDto(printing));
            }

            return dto;
        }

        public PrintingDto GetPrintingDto(SplitPrinting printing)
        {
            PrintingDto dto = FromPrintingBase(printing);

            dto.Artist = printing.LeftArtist;
            dto.SecondArtist = printing.RightArtist;

            return dto;
        }
        #endregion

        #region TransformCard
        public CardDto GetCardDto(TransformCard card)
        {
            // from CardBase
            CardDto dto = FromCardBase<TransformPrinting>(card);

            // from TransformCard
            dto.Cost = card.Cost.ToString();
            dto.Power = card.NormalPower;
            dto.Text = card.NormalText;
            dto.Toughness = card.NormalToughness;
            dto.Tribes = card.NormalTribes;
            dto.Types = card.NormalTypes;

            dto.SecondName = card.TransformedName;
            dto.SecondPower = card.TransformedPower;
            dto.SecondText = card.TransformedText;
            dto.SecondToughness = card.TransformedToughness;
            dto.SecondTribes = card.TransformedTribes;
            dto.SecondTypes = card.TransformedTypes;

            // printings
            foreach (TransformPrinting printing in card.Printings) {
                dto.Printings.Add(GetPrintingDto(printing));
            }

            return dto;
        }

        public PrintingDto GetPrintingDto(TransformPrinting printing)
        {
            PrintingDto dto = FromPrintingBase(printing);

            dto.Artist = printing.NormalArtist;
            dto.FlavorText = printing.NormalFlavorText;

            dto.SecondArtist = printing.TransformedArtist;
            dto.SecondFlavorText = printing.TransformedFlavorText;
            dto.SecondMultiverseId = printing.TransformedMultiverseId;

            return dto;
        }
        #endregion

        #region External utility
        public CardDto GetCardDto(ICard card)
        {
            Type cardType = card.GetType();
            if (typeof(Card).IsAssignableFrom(cardType)) {
                return GetCardDto(card as Card);
            }
            else if (typeof(FlipCard).IsAssignableFrom(cardType)) {
                return GetCardDto(card as FlipCard);
            }
            else if (typeof(SplitCard).IsAssignableFrom(cardType)) {
                return GetCardDto(card as SplitCard);
            }
            else if (typeof(TransformCard).IsAssignableFrom(cardType)) {
                return GetCardDto(card as TransformCard);
            }

            throw new InvalidOperationException("Attempted to convert an ICard of type " + card.GetType().FullName + " to a CardDto.");
        }

        public SetDto GetSetDto(Set set)
        {
            SetDto retVal = _SetDtos.Where(s => s.Code == set.Code).FirstOrDefault();

            if (retVal == null) {
                retVal = new SetDto() {
                    CFName = set.CFName,
                    Code = set.Code,
                    Date = set.Date,
                    IsPromo = set.IsPromo,
                    Name = set.Name,
                    TCGPlayerName = set.TCGPlayerName
                };

                _SetDtos.Add(retVal);
            }

            return retVal;
        }
        #endregion

        #region Internal utility
        private CardDto FromCardBase<T>(CardBase<T> cardBase) where T : PrintingBase
        {
            CardDto dto = new CardDto();

            dto.LegalFormats = cardBase.LegalFormats;
            dto.Name = cardBase.Name;
            dto.Nicknames = cardBase.Nicknames;
            dto.Rulings = cardBase.Rulings.Select(r => new RulingDto() { Date = r.Date, Text = r.Text }).ToList();

            return dto;
        }

        private PrintingDto FromPrintingBase(PrintingBase printingBase)
        {
            PrintingDto dto = new PrintingDto();

            dto.MultiverseId = printingBase.MultiverseId;
            dto.Rarity = printingBase.Rarity;
            dto.Set = GetSetDto(printingBase.Set);
            dto.Watermark = printingBase.Watermark;

            return dto;
        }
        #endregion
    }
}