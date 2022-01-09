using SvadbeniSalon.Database.Models;
using SvadbeniSalon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.DTO.Requests.User
{
    public class AddUser
    {
        [Required]
        public string Username { get; set; }
        public string Lozinka { get; set; }
        [Required]
        public string Ime { get; set; }
        [Required]
        public string Prezime { get; set; }
        [Required]
        public string Email { get; set; }
        public Uloga Uloga { get; set; }
    }
}
