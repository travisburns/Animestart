using Microsoft.AspNetCore.Identity;
using System;

namespace animestart.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfileImagePath { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}