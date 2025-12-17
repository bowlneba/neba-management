namespace Neba.IntegrationTests.Infrastructure;

/// <summary>
/// Defines a test collection for Awards integration tests.
/// Tests within this collection will not run in parallel to ensure database isolation.
/// </summary>
[CollectionDefinition(nameof(AwardsIntegrationTestCollection), DisableParallelization = true)]
public sealed class AwardsIntegrationTestCollection
{
}
