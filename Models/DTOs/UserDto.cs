using System.ComponentModel.DataAnnotations;

namespace CdnFreelancerApi.Models.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Skillsets { get; set; } = string.Empty;
        public string Hobby { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
    }
}

