using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Models
{
    public class Muzicar : BaseEntity
    {
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public virtual ICollection<MuzicarZanr> Zanrovi { get; set; }
    }
}
