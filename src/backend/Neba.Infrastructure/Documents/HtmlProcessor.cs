using System.Text;
using System.Text.RegularExpressions;

namespace Neba.Infrastructure.Documents;

internal sealed class HtmlProcessor(GoogleDocsSettings settings)
{
    private readonly IReadOnlyCollection<GoogleDocument> _documents = settings.Documents;

    /// <summary>
    /// Post-processes HTML exported from Google Docs to:
    /// 1. Extract body content
    /// 2. Replace Google Docs URLs with internal routes
    /// 3. Generate human-readable anchor IDs for headings and bookmarks
    /// </summary>
    public string ProcessExportedHtml(string rawHtml)
    {
        // Extract body content
        string bodyContent = ExtractBodyContent(rawHtml);

        // Replace Google Docs links with internal routes
        string processedHtml = ReplaceGoogleDocsLinks(bodyContent);

        // Generate human-readable IDs for headings and update anchor links
        return GenerateHumanReadableIds(processedHtml);
    }

    private static string ExtractBodyContent(string rawHtml)
    {
        // Extract content between <body> tags
        Match bodyMatch = Regex.Match(rawHtml, "<body[^>]*>(.*?)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
        if (!bodyMatch.Success)
        {
            return rawHtml;
        }

        return bodyMatch.Groups[1].Value.Trim();
    }

    private string ReplaceGoogleDocsLinks(string html)
    {
        // Build a map of documentId -> route
        var documentRouteMap = _documents.ToDictionary(
            doc => doc.DocumentId,
            doc => doc.Name
        );

        // Replace Google Docs URLs with internal routes
        // Pattern: https://docs.google.com/document/d/DOCUMENT_ID/edit or /u/0/d/DOCUMENT_ID/edit
        const string pattern = @"https://docs\.google\.com/document/(?:u/\d+/)?d/([a-zA-Z0-9_-]+)(?:/[^""']*)?";

        return Regex.Replace(html, pattern, match =>
        {
            string documentId = match.Groups[1].Value;

            // If this document is in our configuration, replace with internal route
            if (documentRouteMap.TryGetValue(documentId, out string? routeName))
            {
                return $"/{routeName}";
            }

            // Otherwise, keep the original Google Docs URL
            return match.Value;
        }, RegexOptions.None, TimeSpan.FromSeconds(1));
    }

    private static string GenerateHumanReadableIds(string html)
    {
        // Build a map of old Google Docs IDs to new human-readable IDs
        var idMap = new Dictionary<string, string>();
        var usedIds = new HashSet<string>();

        // First pass: Find all elements with IDs and generate human-readable versions
        // Match headings like <h1 id="h.abc123">Section 10.3 Hall of Fame</h1>
        const string headingPattern = @"<(h[1-6])\s+[^>]*id=""([^""]+)""[^>]*>(.*?)</\1>";
        html = Regex.Replace(html, headingPattern, match =>
        {
            string oldId = match.Groups[2].Value;
            string content = match.Groups[3].Value;

            // Extract plain text from HTML content
            string plainText = Regex.Replace(content, "<[^>]+>", "", RegexOptions.None, TimeSpan.FromSeconds(1));

            // Generate human-readable ID
            string newId = GenerateSlug(plainText, usedIds);
            idMap[oldId] = newId;

            // Replace the entire match with updated ID
            return Regex.Replace(match.Value, $@"id=""{Regex.Escape(oldId)}""", $@"id=""{newId}"" data-original-id=""{oldId}""", RegexOptions.None, TimeSpan.FromSeconds(1));
        }, RegexOptions.Singleline, TimeSpan.FromSeconds(5));

        // Also handle bookmark spans like <span id="h.abc123"></span> or <a id="h.abc123"></a>
        // For these, we'll try to use the link text that references them
        const string bookmarkPattern = @"<(span|a)\s+[^>]*id=""([^""]+)""[^>]*></\1>";
        MatchCollection bookmarkMatches = Regex.Matches(html, bookmarkPattern, RegexOptions.None, TimeSpan.FromSeconds(5));

        foreach (Match bookmarkMatch in bookmarkMatches)
        {
            string oldId = bookmarkMatch.Groups[2].Value;

            // Try to find a link that points to this bookmark to get context
            string linkPattern = $@"<a[^>]+href=""#{Regex.Escape(oldId)}""[^>]*>([^<]+)</a>";
            Match linkMatch = Regex.Match(html, linkPattern, RegexOptions.None, TimeSpan.FromSeconds(1));

            string newId;
            if (linkMatch.Success)
            {
                string linkText = linkMatch.Groups[1].Value;
                newId = GenerateSlug(linkText, usedIds);
            }
            else
            {
                // Fallback: use the old ID if we can't determine good text
                newId = oldId.StartsWith("h.", StringComparison.Ordinal) ? oldId.Substring(2) : oldId;
                newId = EnsureUnique(newId, usedIds);
            }

            idMap[oldId] = newId;
        }

        // Replace bookmark IDs in the HTML
        foreach (KeyValuePair<string, string> kvp in idMap)
        {
            string oldId = kvp.Key;
            string newId = kvp.Value;

            // Update bookmark elements (not already updated by heading replacement)
            html = Regex.Replace(html,
                $@"(<(?:span|a)\s+[^>]*)id=""{Regex.Escape(oldId)}""([^>]*></(?:span|a)>)",
                $@"$1id=""{newId}"" data-original-id=""{oldId}""$2",
                RegexOptions.None, TimeSpan.FromSeconds(1));
        }

        // Second pass: Update all anchor links to use new IDs
        foreach (KeyValuePair<string, string> kvp in idMap)
        {
            string oldId = kvp.Key;
            string newId = kvp.Value;

            // Update href="#oldId" to href="#newId"
            html = Regex.Replace(html,
                $@"href=""#{Regex.Escape(oldId)}""",
                $@"href=""#{newId}""",
                RegexOptions.None, TimeSpan.FromSeconds(1));
        }

        return html;
    }

    private static string GenerateSlug(string text, HashSet<string> usedIds)
    {
        // Remove HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);

        // Convert to lowercase (URLs are case-insensitive and lowercase is the standard convention)
#pragma warning disable CA1308 // Normalize strings to uppercase
        text = text.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

        // Remove special characters, keep only alphanumeric, spaces, hyphens, and periods
        text = Regex.Replace(text, @"[^a-z0-9\s\-\.]", "", RegexOptions.None, TimeSpan.FromSeconds(1));

        // Replace multiple spaces with single space
        text = Regex.Replace(text, @"\s+", " ", RegexOptions.None, TimeSpan.FromSeconds(1));

        // Trim and replace spaces with hyphens
        text = text.Trim().Replace(" ", "-", StringComparison.Ordinal);

        // Remove multiple consecutive hyphens
        text = Regex.Replace(text, "-+", "-", RegexOptions.None, TimeSpan.FromSeconds(1));

        // Trim hyphens from start and end
        text = text.Trim('-');

        // Ensure it's not empty
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "section";
        }

        // Ensure uniqueness
        return EnsureUnique(text, usedIds);
    }

    private static string EnsureUnique(string baseId, HashSet<string> usedIds)
    {
        string uniqueId = baseId;
        int counter = 1;

        while (usedIds.Contains(uniqueId))
        {
            uniqueId = $"{baseId}-{counter}";
            counter++;
        }

        usedIds.Add(uniqueId);
        return uniqueId;
    }
}
