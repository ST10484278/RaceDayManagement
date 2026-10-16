using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Define Primary Keys ---
        modelBuilder.Entity<Role>().HasKey(r => r.RoleID);
        modelBuilder.Entity<AppUser>().HasKey(u => u.UserID);
        modelBuilder.Entity<Event>().HasKey(e => e.EventID);
        modelBuilder.Entity<Category>().HasKey(c => c.EventCategoryID);
        modelBuilder.Entity<Enrolment>().HasKey(e => e.EnrollmentID);
        modelBuilder.Entity<Result>().HasKey(r => r.ResultID);

        // --- Unique Constraints ---
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.RoleName)
            .IsUnique();

        // --- Relationships & Delete Behavior ---

        // 1. One Role -> Many Users
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleID)
            .OnDelete(DeleteBehavior.Restrict);

        // 2. One Organiser (AppUser) -> Many Events
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Restrict);

        // 3. One Event -> Many Categories
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        // 4. One Participant (AppUser) -> Many Enrolments
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.ParticipantID)
            .OnDelete(DeleteBehavior.Restrict);

        // 5. One Category -> Many Enrolments
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.EventCategoryID)
            .OnDelete(DeleteBehavior.Restrict);

        // 6. One Enrolment -> Zero or One Result
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithMany(e => e.Results)
            .HasForeignKey(r => r.EnrollmentID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}