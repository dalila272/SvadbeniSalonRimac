using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Models
{
    public class MuzicarZanr
    {
        public long MuzicarId { get; set; }
        public Muzicar Muzicar { get; set; }
        public long ZanrId { get; set; }
        public Zanr Zanr { get; set; }
    }
}
