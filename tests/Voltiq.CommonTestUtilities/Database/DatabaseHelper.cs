using Microsoft.EntityFrameworkCore;
using Voltiq.Infrastructure.Persistence;

namespace Voltiq.CommonTestUtilities.Database;

public static class DatabaseHelper
{
    public static async Task CleanAsync(ApplicationDbContext dbContext)
    {
        var tableNames = dbContext.Model.GetEntityTypes()
            .Select(e => e.GetTableName())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .Select(t => $"\"{t}\"");

        var sql = $"TRUNCATE TABLE {string.Join(", ", tableNames)} RESTART IDENTITY CASCADE";
        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }
}
