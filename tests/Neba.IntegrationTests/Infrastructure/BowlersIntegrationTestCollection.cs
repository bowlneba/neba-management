namespace Neba.IntegrationTests.Infrastructure;

/// <summary>
/// Defines a test collection for Bowlers integration tests.
/// Tests within this collection will not run in parallel to ensure database isolation.
/// </summary>
[CollectionDefinition(nameof(BowlersIntegrationTestCollection), DisableParallelization = true)]
public sealed class BowlersIntegrationTestCollection
{
}
