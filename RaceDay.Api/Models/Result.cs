namespace RaceDay.Api.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public string RaceNumber { get; set; } = string.Empty;

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        public Enrolment? Enrolment { get; set; }
    }
}