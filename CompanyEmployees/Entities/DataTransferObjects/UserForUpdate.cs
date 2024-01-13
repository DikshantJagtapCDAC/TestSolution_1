using System.ComponentModel.DataAnnotations;

namespace CompanyEmployees.Entities.DataTransferObjects
{
    public class UserForUpdate
    {
        public string Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "photoUrl is required")]
        public string? PhotoUrl { get; set; }

        public string? password { get; set; }

        public string? conformPassword { get;set; }
    }
}
