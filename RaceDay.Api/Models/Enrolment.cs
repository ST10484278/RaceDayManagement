namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrollmentID { get; set; }
        public int ParticipantID { get; set; }
        public int EventCategoryID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string EnrollmentStatus { get; set; } = string.Empty;
        public int RaceNumber { get; set; }

        // Navigation properties
        public AppUser Participant { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
}
