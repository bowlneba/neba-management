namespace Neba.IntegrationTests.Infrastructure;

/// <summary>
/// Defines a test collection for Titles integration tests.
/// Tests within this collection will not run in parallel to ensure database isolation.
/// </summary>
[CollectionDefinition(nameof(TitlesIntegrationTestCollection), DisableParallelization = true)]
public sealed class TitlesIntegrationTestCollection
{
}
