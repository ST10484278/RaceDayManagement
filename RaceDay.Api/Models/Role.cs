namespace RaceDay.Api.Models
{
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;

        // Navigation property
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}
