namespace Neba.IntegrationTests;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection
    : ICollectionFixture<IntegrationTestWebAppFactory>;