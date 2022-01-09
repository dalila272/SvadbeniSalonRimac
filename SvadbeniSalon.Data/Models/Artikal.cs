using SvadbeniSalon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Models
{
    public class Artikal : BaseEntity
    {
        public string Naziv { get; set; }
        public TipArtikla Tip { get; set; }
        public decimal Cijena { get; set; }
    }
}
