namespace RaceDay.Api.Models
{
    public class Category
    {
        public int EventCategoryID { get; set; }
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public decimal Distance { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public Event Event { get; set; } = null!;
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
