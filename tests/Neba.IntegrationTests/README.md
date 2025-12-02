# Integration Tests

This project contains integration tests for the Neba API using ASP.NET Core's `WebApplicationFactory`.

## Architecture

- **IntegrationTestBase**: Base class that provides database and web application factory setup
- **NebaWebApplicationFactory**: Custom `WebApplicationFactory` that configures the test database
- **WebsiteDatabase**: Provides a PostgreSQL test database using Testcontainers (from Neba.Tests project)

## Writing Integration Tests

### Basic Structure

Each test class should inherit from `IntegrationTestBase`:

```csharp
public sealed class MyIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task MyTest()
    {
        // Arrange - Reset database to clean state
        await ResetDatabaseAsync();

        // Create HTTP client
        using HttpClient client = Factory.CreateClient();

        // Act - Make API calls
        HttpResponseMessage response = await client.GetAsync(
            new Uri("/api/endpoint", UriKind.Relative));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
```

### Database Isolation

- Each test class gets its own database instance
- Use `ResetDatabaseAsync()` at the start of each test to ensure a clean database state
- The database is automatically started before tests and disposed after all tests complete

### Accessing the Factory

The `Factory` property provides access to the `WebApplicationFactory<Program>`:

```csharp
// Create HTTP client
using HttpClient client = Factory.CreateClient();

// Or access services
var service = Factory.Services.GetRequiredService<IMyService>();
```

### Seeding Data

Use the `SeedAsync` helper method to seed data into the database:

```csharp
[Fact]
public async Task MyTest()
{
    // Arrange - Seed test data
    await ResetDatabaseAsync();

    await SeedAsync(async context =>
    {
        context.Bowlers.Add(new Bowler
        {
            /* ... */
        });
        // No need to call SaveChangesAsync - it's done automatically
    });

    // Act & Assert...
}
```

### Querying Data

Use the `ExecuteAsync` helper to query the database directly:

```csharp
int bowlerCount = await ExecuteAsync(async context =>
    await context.Bowlers.CountAsync());
```

### Direct Database Access

The `Database` property provides direct access to the test database:

```csharp
string connectionString = Database.ConnectionString;
```

## Example Test

See [PlaceholderIntegrationTest.cs](PlaceholderIntegrationTest.cs) for a complete example.
