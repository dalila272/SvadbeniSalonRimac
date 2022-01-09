using SvadbeniSalon.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.DTO.Requests.Meni
{
    public class AddMeni
    {
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(150, ErrorMessage = "Maximum 150 characters")]
        public string Naziv { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000, ErrorMessage = "Maximum 1000 characters")]
        public string Opis { get; set; }
        public List<long> Hrana { get; set; }
        public List<long> Pice { get; set; }

        public List<Artikal> Artikli
        {
            get
            {
                var artikli = new List<Artikal>();
                if (Hrana != null)
                    artikli.AddRange(Hrana.Select(id => new Artikal
                    {
                        Id = id,
                        Tip = Shared.Enums.TipArtikla.Hrana
                    }));

                if (Pice != null)
                    artikli.AddRange(Pice.Select(id => new Artikal
                    {
                        Id = id,
                        Tip = Shared.Enums.TipArtikla.Pice
                    }));

                return artikli;
            }
        }
    }
}
