using System.ComponentModel.DataAnnotations;

namespace CdnFreelancerApi.Models.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        public string Skillsets { get; set; } = string.Empty;

        public string Hobby { get; set; } = string.Empty;
    }
}
