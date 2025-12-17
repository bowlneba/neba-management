namespace Neba.IntegrationTests.Infrastructure.Collections;

/// <summary>
/// Defines a test collection for Bowlers integration tests.
/// Tests within this collection will not run in parallel to ensure database isolation.
/// </summary>
[CollectionDefinition(nameof(BowlersIntegrationTests), DisableParallelization = true)]
public sealed class BowlersIntegrationTests
{
}
