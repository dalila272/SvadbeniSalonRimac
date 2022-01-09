using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.DTO.Requests.Muzicar
{
    public class AddMuzicar
    {
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public List<long> Zanrovi { get; set; }
    }
}
