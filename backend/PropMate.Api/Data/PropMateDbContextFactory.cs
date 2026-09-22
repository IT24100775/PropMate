using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PropMate.Api.Data;

public class PropMateDbContextFactory : IDesignTimeDbContextFactory<PropMateDbContext>
{
    public PropMateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PropMateDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("PROPMATE_DB_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "PROPMATE_DB_CONNECTION environment variable is not set.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new PropMateDbContext(optionsBuilder.Options);
    }
}
