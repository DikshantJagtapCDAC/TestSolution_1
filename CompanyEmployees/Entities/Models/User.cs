using Microsoft.AspNetCore.Identity;

namespace CompanyEmployees.Entities.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhotoUrl { get; set; }
        public int Workdays { get; set; }


    }
}
