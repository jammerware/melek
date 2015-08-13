using System.Collections.Generic;
using Melek.Domain.Json;
using Newtonsoft.Json;

namespace Melek.Domain
{
    [JsonConverter(typeof(CardJsonConverter))]
    public interface ICard
    {
        IReadOnlyList<Format> LegalFormats { get; set; }
        string Name { get; set; }
        IList<string> Nicknames { get; }
        IList<IPrinting> Printings { get; set; }
        IReadOnlyList<Ruling> Rulings { get; set; }

        int GetConvertedManaCost();
        bool IsColor(MagicColor color);
        bool IsColors(IEnumerable<MagicColor> colors);
        bool IsMulticolored();

        /// <summary>
        /// Gets all costs associated with the card. In the overwhelming majority of cases, this will only contain one entry - the cost of the card - but in the case of things like
        /// split cards, it'll contain 2 or more items to reflect all the potential costs of the card. In later versions of the API, I MAY do things like incorporating additional, kicker
        /// or alternative costs in this list, but nothing on that front for now. The bottom line is, if you want to know the specifics of where these costs come from, convert the ICard
        /// to the appropriate concrete type and then interrogate it.
        /// </summary>
        IReadOnlyList<CardCostCollection> AllCosts { get; }
        IReadOnlyList<CardType> AllTypes { get; }

        /// <summary>
        /// Gets the last non-promo printing of this card (or the last printing if there aren't any non-promo ones).
        /// </summary>
        /// <returns></returns>
        IPrinting GetLastPrinting();
    }
}