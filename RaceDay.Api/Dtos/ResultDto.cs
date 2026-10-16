
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateResultDto
    {
        [Required]
        [StringLength(20)]
        public string RaceNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int? Position { get; set; }
    }
}
