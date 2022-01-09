using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Models
{
    public class Muzicar : BaseEntity
    {
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public List<Zanr> Zanrovi { get; set; }
    }
}
