using System.Collections.Generic;

namespace Melek.Models
{
    public interface ICard<T> where T : PrintingBase
    {
        IReadOnlyList<Format> LegalFormats { get; set; }
        string Name { get; set; }
        IReadOnlyList<string> Nicknames { get; set; }
        IReadOnlyList<T> Printings { get; set; }
        IReadOnlyList<Ruling> Rulings { get; set; }
        
        bool IsColor(MagicColor color);
        bool IsColors(IEnumerable<MagicColor> colors);

        /// <summary>
        /// Gets the last non-promo printing of this card (or the last printing if there aren't any non-promo ones).
        /// </summary>
        /// <returns></returns>
        PrintingBase GetLastPrinting();
    }
}