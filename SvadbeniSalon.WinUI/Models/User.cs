using SvadbeniSalon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.WinUI.Models
{
    public class User
    {
        public string Username { get; set; }
        public Uloga Uloga { get; set; }
        public string Email { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
    }
}
