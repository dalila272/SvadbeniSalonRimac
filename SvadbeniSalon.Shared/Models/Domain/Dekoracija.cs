using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Models
{
    public class Dekoracija : BaseEntity
    {
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public decimal Cijena { get; set; }

    }
}
