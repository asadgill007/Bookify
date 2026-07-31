using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bookify.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by `dotnet ef migrations add` so migrations can be
/// generated against the SQL Server provider even though development runs use
/// the InMemory provider.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=Bookify;Trusted_Connection=True;TrustServerCertificate=True;",
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
