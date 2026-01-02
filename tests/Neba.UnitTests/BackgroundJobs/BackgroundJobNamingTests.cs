using Neba.Application.BackgroundJobs;

namespace Neba.UnitTests.BackgroundJobs;
[Trait("Category", "Unit")]
[Trait("Component", "BackgroundJobs")]

public sealed class BackgroundJobNamingTests
{
    [Fact(DisplayName = "IBackgroundJob provides custom display name")]
    public void BackgroundJob_ProvidesCustomDisplayName()
    {
        // Arrange
        var job = new TestNamedJob { Name = "Test Document" };

        // Act
        string displayName = job.JobName;

        // Assert
        displayName.ShouldBe("Test Job: Test Document");
    }

    [Fact(DisplayName = "IBackgroundJob with simple name uses type name")]
    public void BackgroundJob_CanUseTypeNameForSimpleJobs()
    {
        // Arrange
        var job = new TestSimpleJob();

        // Act
        string displayName = job.JobName;

        // Assert
        displayName.ShouldBe("TestSimpleJob");
    }

    [Fact(DisplayName = "Custom job name includes contextual information")]
    public void CustomJobName_IncludesContextualInformation()
    {
        // Arrange
        var job = new TestNamedJob { Name = "Bylaws" };

        // Act
        string displayName = job.JobName;

        // Assert
        displayName.ShouldContain("Bylaws");
        displayName.ShouldBe("Test Job: Bylaws");
    }

    // Test jobs for verification
    private sealed record TestSimpleJob : IBackgroundJob
    {
        public string JobName => nameof(TestSimpleJob);
    }

    private sealed record TestNamedJob : IBackgroundJob
    {
        public required string Name { get; init; }

        public string JobName => $"Test Job: {Name}";
    }
}

