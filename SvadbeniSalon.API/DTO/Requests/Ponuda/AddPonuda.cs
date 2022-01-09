using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.DTO.Requests.Ponuda
{
    public class AddPonuda
    {
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(150, ErrorMessage = "Maximum 150 characters")]
        public string Naziv { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(2000, ErrorMessage = "Maximum 2000 characters")]
        public string Opis { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [Range(0, 100000, ErrorMessage = "Value is not in expected range (0-100000)")]
        public decimal Cijena { get; set; }
        public long MeniId { get; set; }
        [Required(ErrorMessage = "Field is required")]
        public List<long> Dekoracije { get; set; }
        [Required(ErrorMessage = "Field is required")]
        public List<long> Muzicari { get; set; }
    }
}
