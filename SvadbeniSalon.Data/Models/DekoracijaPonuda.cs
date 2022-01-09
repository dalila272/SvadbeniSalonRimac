using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Models
{
    public class DekoracijaPonuda
    {
        public long PonudaId { get; set; }
        public Ponuda Ponuda { get; set; }
        public long DekoracijaId { get; set; }
        public Dekoracija Dekoracija { get; set; }
    }
}
