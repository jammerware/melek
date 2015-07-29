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
        IReadOnlyList<string> Nicknames { get; set; }
        IList<IPrinting> Printings { get; set; }
        IReadOnlyList<Ruling> Rulings { get; set; }

        int GetConvertedManaCost();
        bool IsColor(MagicColor color);
        bool IsColors(IEnumerable<MagicColor> colors);
        bool IsMulticolored();

        /// <summary>
        /// Gets the last non-promo printing of this card (or the last printing if there aren't any non-promo ones).
        /// </summary>
        /// <returns></returns>
        IPrinting GetLastPrinting();
    }
}