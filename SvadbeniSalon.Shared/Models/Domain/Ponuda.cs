using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Models
{
    public class Ponuda : BaseEntity
    {
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public decimal Cijena { get; set; }
        public long MeniId { get; set; }
        public Meni Meni { get; set; }
        public List<Muzicar> Muzicari { get; set; }
        public List<Dekoracija> Dekoracije { get; set; }
    }
}
