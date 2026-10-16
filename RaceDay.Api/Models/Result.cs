namespace RaceDay.Api.Models
{
    public class Result
    {
        public int ResultID { get; set; }
        public int EnrollmentID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public string Pace { get; set; } = string.Empty;
        public int Position { get; set; }
        public string Status { get; set; } = string.Empty;

        // Navigation property
        public Enrolment Enrolment { get; set; } = null!;
    }
}
