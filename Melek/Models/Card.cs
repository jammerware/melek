using System;
using System.Collections.Generic;
using Bazam.Slugging;

namespace Melek.Models
{
    public class Card : Slugger
    {
        public CardType[] CardTypes { get; set; }
        public CardCostCollection Cost { get; set; }
        public string Name { get; set; }
        public string[] Nicknames { get; set; }
        public int? Power { get; set; }
        public List<CardPrinting> Printings { get; set; }
        public string Text { get; set; }
        public int? Toughness { get; set; }
        public string Tribe { get; set; }
        public string Watermark { get; set; }

        public Card()
        {
            Printings = new List<CardPrinting>();
        }

        protected override string SlugBase()
        {
            return Name;
        }

        public Card Copy()
        {
            return this.MemberwiseClone() as Card;
        }
    }
}