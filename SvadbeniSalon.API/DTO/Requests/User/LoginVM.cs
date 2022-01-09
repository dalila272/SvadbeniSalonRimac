using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.DTO.Requests.User
{
    public class LoginVM
    {
        
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
