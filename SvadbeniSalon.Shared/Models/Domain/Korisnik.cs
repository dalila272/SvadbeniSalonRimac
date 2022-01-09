using SvadbeniSalon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SvadbeniSalon.Shared.Models
{
    public class Korisnik : BaseEntity
    {
        public string Username { get; set; }
        [JsonIgnore]
        public string PasswordHash { get; set; }
        [JsonIgnore]
        public string PasswordSalt { get; set; }
        public Uloga Uloga { get; set; }
        public string Email { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
    }
}
