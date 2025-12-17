namespace Neba.IntegrationTests.Infrastructure.Collections;

/// <summary>
/// Defines a test collection for Awards integration tests.
/// Tests within this collection will not run in parallel to ensure database isolation.
/// </summary>
[CollectionDefinition(nameof(AwardsIntegrationTest), DisableParallelization = true)]
public sealed class AwardsIntegrationTest
{
}
