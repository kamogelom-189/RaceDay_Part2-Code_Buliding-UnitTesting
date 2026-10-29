namespace RaceDay.Api.Models
{
    public class RaceEvent
    {
        public int EventId { get; set; }

        public int OrganiserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public decimal DistanceKM { get; set; }

        public string EventType { get; set; } = "run";

        public string? BannerUrl { get; set; }

        public AppUser? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; }
            = new List<Category>();
    }
}
