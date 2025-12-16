namespace Neba.Domain;

/// <summary>
/// Ulid extensions.
/// </summary>
public static class UlidExtensions
{
    #pragma warning disable CA1034 // Nested types should not be visible

    extension(Ulid)
    {
        /// <summary>
        /// Gets an empty Ulid (all zeros).
        /// </summary>
        public static Ulid Empty
            => new(Guid.Empty);
    }
}
