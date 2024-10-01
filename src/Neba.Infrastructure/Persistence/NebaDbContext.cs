using Microsoft.EntityFrameworkCore;

namespace Neba.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the Neba application.
/// </summary>
public abstract class NebaDbContext
    : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NebaDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    protected NebaDbContext(DbContextOptions options)
        : base(options)
    { }
}