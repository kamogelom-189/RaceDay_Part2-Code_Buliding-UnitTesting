using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
    }
}