using SvadbeniSalon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SvadbeniSalon.Shared.Models.Requests.Artikal
{
    public class AddArtikal
    {
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(150, ErrorMessage = "Maximum 150 characters")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public decimal Cijena { get; set; }
        public TipArtikla Tip { get; set; }

    }
}
