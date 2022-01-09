using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Models
{
    public class Ponuda : BaseEntity
    {
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public decimal Cijena { get; set; }
        public long MeniId { get; set; }
        public Meni Meni { get; set; }
        public ICollection<MuzicariPonuda> MuzicariPonuda { get; set; }
        public ICollection<DekoracijaPonuda> DekoracijePonuda { get; set; }
    }
}
