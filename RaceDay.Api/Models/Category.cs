namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKM { get; set; }

        public decimal EntryFee { get; set; }

        public RaceEvent? Event { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}