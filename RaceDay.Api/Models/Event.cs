namespace RaceDay.Api.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int OrganiserID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }

        // Navigation properties
        public AppUser Organiser { get; set; } = null!;
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
