namespace SOS.ContainerIntegrationTests.Setup;

/// <summary>
/// Represents a collection definition for integration tests.
/// </summary>
/// <remarks>
/// Parallelization is turned off to simplify writing tests that use 
/// temporary test databases running in containers.
/// </remarks>
[CollectionDefinition(name: Name, DisableParallelization = true)]
public class IntegrationTestsCollection : ICollectionFixture<IntegrationTestsFixture>
{
    /// <summary>
    /// The name of the integration tests collection.
    /// </summary>
    public const string Name = "IntegrationTestsCollection";
}
