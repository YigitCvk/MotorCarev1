using Microsoft.EntityFrameworkCore;
using MotorCare.Infrastructure.Persistence;

const string connectionString = "Host=localhost;Port=5432;Database=motorcare;Username=motorcare;Password=motorcare_dev_password";

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
    .Options;

await using var db = new ApplicationDbContext(options);

await using var conn = db.Database.GetDbConnection();
await conn.OpenAsync();

await using (var historyCmd = conn.CreateCommand())
{
    historyCmd.CommandText = "select \"MigrationId\" from \"__EFMigrationsHistory\" order by \"MigrationId\";";
    await using var reader = await historyCmd.ExecuteReaderAsync();
    Console.WriteLine("MIGRATIONS:");
    while (await reader.ReadAsync())
    {
        Console.WriteLine(reader.GetString(0));
    }
}

await using (var tableCmd = conn.CreateCommand())
{
    tableCmd.CommandText = "select tablename from pg_tables where schemaname='public' and tablename ilike 'inventory%';";
    await using var reader = await tableCmd.ExecuteReaderAsync();
    Console.WriteLine("TABLES:");
    while (await reader.ReadAsync())
    {
        Console.WriteLine(reader.GetString(0));
    }
}

Console.WriteLine("APPLY_MIGRATIONS");
await db.Database.MigrateAsync();
Console.WriteLine("DONE");
