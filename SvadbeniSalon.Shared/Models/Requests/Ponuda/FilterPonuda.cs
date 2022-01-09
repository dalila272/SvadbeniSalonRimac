using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Models.Requests.Ponuda
{
    public class FilterPonuda
    {
        public long? MeniId { get; set; }
        public List<long> DekoracijeIds { get; set; }
        public string Muzicar { get; set; }
    }
}
