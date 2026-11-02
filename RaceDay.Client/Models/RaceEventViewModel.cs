namespace RaceDay.Client.Models;

public class RaceEventViewModel
{
    public int EventId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal? DistanceKM { get; set; }
    public string EventType { get; set; } = "";
}
