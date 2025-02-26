using System.ComponentModel.DataAnnotations;

namespace CdnFreelancerApi.Models.DTOs
{
    public class ChangePasswordDto
    {
        [Required, MinLength(6)]
        public required string CurrentPassword { get; set; }

        [Required, MinLength(6)]
        public required string NewPassword { get; set; }
    }
}
