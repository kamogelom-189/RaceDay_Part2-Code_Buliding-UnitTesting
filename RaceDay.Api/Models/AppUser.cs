namespace RaceDay.Api.Models
{
    public class AppUser
    {
        public int UserId { get; set; }

        public int RoleId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public Role? Role { get; set; }

        public ICollection<RaceEvent> OrganisedEvents { get; set; }
            = new List<RaceEvent>();

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}