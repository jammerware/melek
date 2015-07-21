using System;
using System.Linq;
using Melek.Domain;
using Melek.Db.Dtos;

namespace Melek.Db.Factories
{
    public class ModelFactory
    {
        public ICard GetICard(CardDto dto)
        {
            switch(dto.CardModelType) {
                case CardModelType.Flip:
                    FlipCard flipCard = CardFromDto<FlipCard>(new FlipCard(), dto);

                    // specific fields
                    flipCard.Cost = new CardCostCollection(dto.Cost);

                    flipCard.FlippedName = dto.SecondName;
                    flipCard.FlippedPower = dto.SecondPower;
                    flipCard.FlippedText = dto.SecondText;
                    flipCard.FlippedToughness = dto.SecondToughness;
                    flipCard.FlippedTribes = dto.Tribes;
                    flipCard.FlippedTypes = dto.Types;

                    flipCard.NormalPower = dto.Power;
                    flipCard.NormalText = dto.Text;
                    flipCard.NormalToughness = dto.Toughness;
                    flipCard.NormalTribes = dto.Tribes;
                    flipCard.NormalTypes = dto.Types;

                    // printings
                    flipCard.Printings = dto.Printings.Select(p => {
                        FlipPrinting printing = PrintingFromDto<FlipPrinting>(new FlipPrinting(), p);
                        printing.Artist = p.Artist;

                        return printing;
                    }).ToList();

                    return flipCard;
                case CardModelType.Split:
                    SplitCard splitCard = CardFromDto<SplitCard>(new SplitCard(), dto);

                    // specific fields
                    splitCard.HasFuse = dto.HasFuse;
                    splitCard.Type = dto.Types.First();
                    splitCard.LeftCost = new CardCostCollection(dto.Cost);
                    splitCard.LeftText = dto.Text;
                    splitCard.RightCost = new CardCostCollection(dto.SecondCost);

                    // printings
                    splitCard.Printings = dto.Printings.Select(p => {
                        SplitPrinting printing = PrintingFromDto<SplitPrinting>(new SplitPrinting(), p);
                        printing.LeftArtist = p.Artist;
                        printing.RightArtist = p.SecondArtist;

                        return printing;
                    }).ToList();

                    return splitCard;
                case CardModelType.Transform:
                    TransformCard transformCard = CardFromDto<TransformCard>(new TransformCard(), dto);

                    // specific fields
                    transformCard.Cost = new CardCostCollection(dto.Cost);

                    transformCard.NormalPower = dto.Power;
                    transformCard.NormalText = dto.Text;
                    transformCard.NormalToughness = dto.Toughness;
                    transformCard.NormalTribes = dto.Tribes;
                    transformCard.NormalTypes = dto.Types;

                    transformCard.TransformedName = dto.SecondName;
                    transformCard.TransformedPower = dto.SecondPower;
                    transformCard.TransformedText = dto.SecondText;
                    transformCard.TransformedToughness = dto.SecondToughness;
                    transformCard.TransformedTribes = dto.SecondTribes;
                    transformCard.TransformedTypes = dto.SecondTypes;

                    transformCard.Printings = dto.Printings.Select(p => {
                        TransformPrinting printing = PrintingFromDto<TransformPrinting>(new TransformPrinting(), p);

                        printing.NormalArtist = p.Artist;
                        printing.NormalFlavorText = p.FlavorText;
                        printing.TransformedArtist = p.SecondArtist;
                        printing.TransformedFlavorText = p.SecondFlavorText;
                        printing.TransformedMultiverseId = p.SecondMultiverseId;

                        return printing;
                    }).ToList();

                    return transformCard;
                default:
                    Card card = CardFromDto<Card>(new Card(), dto);

                    // specific fieldz
                    card.Cost = new CardCostCollection(dto.Cost);
                    card.Power = dto.Power;
                    card.Text = dto.Text;
                    card.Toughness = dto.Toughness;
                    card.Tribes = dto.Tribes;
                    card.Types = dto.Types;

                    // printings
                    card.Printings = dto.Printings.Select(p => {
                        Printing printing = PrintingFromDto<Printing>(new Printing(), p);
                        printing.Artist = p.Artist;
                        printing.FlavorText = p.FlavorText;

                        return printing;
                    }).ToList();

                    return card;
            }

            throw new InvalidOperationException("Couldn't translate a dto with CardModelType " + dto.CardModelType.ToString());
        }

        private T CardFromDto<T>(T model, CardDto dto) where T : ICard
        {
            model.LegalFormats = dto.LegalFormats;
            model.Name = dto.Name;
            model.Nicknames = dto.Nicknames;
            model.Rulings = dto.Rulings.Select(r => new Ruling() { Date = r.Date, Text = r.Text }).ToList();

            return model;
        }

        private T PrintingFromDto<T>(T model, PrintingDto dto) where T : IPrinting
        {
            model.MultiverseId = dto.MultiverseId;
            model.Rarity = dto.Rarity;
            model.Set = dto.Set;
            model.Watermark = dto.Watermark;

            return model;
        }
    }
}