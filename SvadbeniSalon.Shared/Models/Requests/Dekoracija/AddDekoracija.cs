using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SvadbeniSalon.Shared.Models.Requests.Dekoracija
{
    public class AddDekoracija
    {
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(150, ErrorMessage = "Maximum 150 characters")]
        public string Naziv { get; set; }
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000, ErrorMessage = "Maximum 1000 characters")]
        public string Opis { get; set; }
        [Required(ErrorMessage = "Field is required")]
        public decimal Cijena { get; set; }
    }
}
