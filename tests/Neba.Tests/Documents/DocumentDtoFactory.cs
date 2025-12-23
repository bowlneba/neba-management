using Neba.Application.Documents;

namespace Neba.Tests.Documents;

/// <summary>
/// Factory for creating test DocumentDto instances.
/// </summary>
public static class DocumentDtoFactory
{
    public const string DefaultContent = "<h1>Test Document</h1><p>This is test content.</p>";

    public static DocumentDto Create(
        string? content = null,
        IReadOnlyDictionary<string, string>? metadata = null)
        => new()
        {
            Content = content ?? DefaultContent,
            Metadata = metadata ?? new Dictionary<string, string>()
        };

    public static DocumentDto CreateWithMetadata(
        string? content = null,
        string? author = "TestUser",
        string? version = "1.0",
        string? status = "published")
        => new()
        {
            Content = content ?? DefaultContent,
            Metadata = new Dictionary<string, string>
            {
                ["author"] = author ?? "TestUser",
                ["version"] = version ?? "1.0",
                ["status"] = status ?? "published"
            }
        };

    public static DocumentDto CreateBylaws(
        string? content = null,
        IReadOnlyDictionary<string, string>? metadata = null)
        => new()
        {
            Content = content ?? "<h1>Bylaws</h1><p>These are the bylaws...</p>",
            Metadata = metadata ?? new Dictionary<string, string>()
        };

    public static DocumentDto CreateTournamentRules(
        string? content = null,
        IReadOnlyDictionary<string, string>? metadata = null)
        => new()
        {
            Content = content ?? "<h1>Tournament Rules</h1><p>These are the rules...</p>",
            Metadata = metadata ?? new Dictionary<string, string>()
        };
}
