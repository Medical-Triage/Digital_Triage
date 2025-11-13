using DigitalTriage.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace DigitalTriage.Tests.TestFixtures;

/// <summary>
/// Provides an in-memory database context for testing.
/// Each test gets a fresh database instance to ensure isolation.
/// </summary>
public class DatabaseFixture : IDisposable
{
    public MedicalTriageDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MedicalTriageDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new MedicalTriageDbContext(options);
    }

    public void Dispose()
    {
        // In-memory database is automatically disposed when context is disposed
    }
}

/// <summary>
/// Factory for creating logger instances for testing.
/// </summary>
public static class TestLoggerFactory
{
    public static ILogger<T> Create<T>() => LoggerFactory.Create(builder => builder.AddSimpleConsole()).CreateLogger<T>();
}

