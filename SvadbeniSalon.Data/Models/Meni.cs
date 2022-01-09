using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Models
{
    public class Meni : BaseEntity
    {

        public string Naziv { get; set; }
        public string Opis { get; set; }
        public decimal Cijena { get; set; }
        public ICollection<MeniArtikal> Artikli { get; set; }

    }
}
