using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Models.Requests.Muzicar
{

    public class FilterMuzicar
    {
        public string Naziv { get; set; }
        public IEnumerable<long> Zanrovi { get; set; }
    }
}
