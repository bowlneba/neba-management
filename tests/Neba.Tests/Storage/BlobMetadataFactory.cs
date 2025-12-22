namespace Neba.Tests.Storage;

/// <summary>
/// Factory for creating test blob metadata dictionaries.
/// </summary>
public static class BlobMetadataFactory
{
    public static Dictionary<string, string> CreateSimple()
        => new()
        {
            ["author"] = "TestUser",
            ["version"] = "1.0",
            ["environment"] = "test"
        };

    public static Dictionary<string, string> CreateDocument()
        => new()
        {
            ["documentType"] = "invoice",
            ["year"] = "2025"
        };

    public static Dictionary<string, string> CreateStatus(
        string status = "draft",
        string version = "1.0")
        => new()
        {
            ["status"] = status,
            ["version"] = version
        };

    public static Dictionary<string, string> CreateWithReviewer(
        string status = "published",
        string version = "2.0",
        string reviewedBy = "Admin")
        => new()
        {
            ["status"] = status,
            ["version"] = version,
            ["reviewedBy"] = reviewedBy
        };

    public static Dictionary<string, string> CreateWithSpecialCharacters()
        => new()
        {
            ["description"] = "Test with spaces and special chars: @#$%",
            ["path"] = "/documents/2025/invoices",
            ["email"] = "test@example.com"
        };

    public static Dictionary<string, string> CreateCategory(string category)
        => new() { ["category"] = category };
}
