using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Models
{
    public class MuzicariPonuda
    {
        public long PonudaId { get; set; }
        public Ponuda Ponuda { get; set; }
        public long MuzicarId { get; set; }
        public Muzicar Muzicar { get; set; }
    }
}
