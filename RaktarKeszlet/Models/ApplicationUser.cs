using System;
using Microsoft.AspNetCore.Identity;

namespace RaktarKeszlet.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        //Kapcsolat a céghez
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }    
    }
}