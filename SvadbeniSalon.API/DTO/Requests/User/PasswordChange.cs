using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.DTO.Requests.User
{
    public class PasswordChange
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }
}
