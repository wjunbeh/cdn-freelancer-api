using System.ComponentModel.DataAnnotations;

namespace CdnFreelancerApi.Models.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        public required string Username { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }
        public string Skillsets { get; set; } = string.Empty;
        public string Hobby { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public required string Password { get; set; }
    }
}
