
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999.99")]
        public decimal DistanceKM { get; set; }

        [Range(typeof(decimal), "0", "1000000")]
        public decimal EntryFee { get; set; }
    }
}
