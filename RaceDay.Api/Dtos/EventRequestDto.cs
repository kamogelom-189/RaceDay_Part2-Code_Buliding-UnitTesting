using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class EventRequestDto
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public DateTime? EventDate { get; set; }

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 9999.99)]
        public decimal DistanceKM { get; set; }

        [Required]
        [StringLength(10)]
        public string EventType { get; set; } = "run";
    }
}