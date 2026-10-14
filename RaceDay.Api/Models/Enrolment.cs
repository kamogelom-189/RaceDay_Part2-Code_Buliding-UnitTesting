namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string PaymentStatus { get; set; } = "Pending";

        public AppUser? Participant { get; set; }

        public Category? Category { get; set; }

        public Result? Result { get; set; }
    }
}