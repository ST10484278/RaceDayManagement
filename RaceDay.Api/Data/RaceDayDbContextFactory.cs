using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RaceDay.Api.Data;

public class RaceDayDbContextFactory : IDesignTimeDbContextFactory<RaceDayDbContext>
{
    public RaceDayDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RaceDayDbContext>();

        // Matches your LocalDB setup from appsettings.json
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;");

        return new RaceDayDbContext(optionsBuilder.Options);
    }
}