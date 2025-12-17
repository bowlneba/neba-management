namespace Neba.IntegrationTests.Infrastructure.Collections;

/// <summary>
/// Defines a test collection for Repository integration tests.
/// Tests within this collection will not run in parallel to ensure database isolation.
/// </summary>
[CollectionDefinition(nameof(RepositoryIntegrationTests), DisableParallelization = true)]
public sealed class RepositoryIntegrationTests
{
}
