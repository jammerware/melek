﻿using System.Collections.Generic;
using Melek.Models;

namespace Melek.Db.Dtos
{
    public class CardDto
    {
        // database biz
        public int CardId { get; set; }

        // general properties
        public string Cost { get; set; }
        public ICollection<Format> LegalFormats { get; set; }
        public string Name { get; set; }
        public ICollection<string> Nicknames { get; set; }
        public int? Power { get; set; }
        public ICollection<Ruling> Rulings { get; set; }
        public string Text { get; set; }
        public int? Toughness { get; set; }
        public ICollection<string> Tribes { get; set; }
        public ICollection<CardType> Types { get; set; }
        
        // "second" for flipped/split/transform
        public string SecondCost { get; set; }
        public string SecondName { get; set; }
        public int? SecondPower { get; set; }
        public int? SecondToughness { get; set; }
        public string SecondText { get; set; }
        public ICollection<string> SecondTribes { get; set; }
        public ICollection<CardType> SecondTypes { get; set; }
        
        // split only
        public bool HasFuse { get; set; }

        // printings
        public ICollection<PrintingDto> Printings { get; set; }

        public CardDto()
        {
            Printings = new List<PrintingDto>();
        }
    }
}