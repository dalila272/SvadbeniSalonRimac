using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Models
{
    public class MeniArtikal
    {
        public long ArtikalId { get; set; }
        public Artikal Artikal { get; set; }
        public long MeniId { get; set; }
        public Meni Meni { get; set; }
    }
}
