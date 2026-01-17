using Neba.Infrastructure.Documents;
using Neba.Tests.Website;

namespace Neba.UnitTests.Website.Documents;

[Trait("Category", "Unit")]
[Trait("Component", "Website.Documents")]
public sealed class HtmlProcessorTests
{
    [Fact(DisplayName = "Extracts body content from HTML document")]
    public void ProcessExportedHtml_WithBodyTags_ExtractsBodyContent()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<html><head><title>Test</title></head><body><p>Hello World</p></body></html>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("<p>Hello World</p>");
        result.ShouldNotContain("<html>");
        result.ShouldNotContain("<head>");
        result.ShouldNotContain("<title>");
    }

    [Fact(DisplayName = "Returns original HTML when no body tags present")]
    public void ProcessExportedHtml_WithoutBodyTags_ReturnsOriginalHtml()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<p>Hello World</p>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("<p>Hello World</p>");
    }

    [Fact(DisplayName = "Replaces Google Docs URL with internal route for known document")]
    public void ProcessExportedHtml_WithGoogleDocsLink_ReplacesWithInternalRoute()
    {
        // Arrange
        const string docId = "1abc123def456";
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create(("bylaws", docId, "/bylaws"));
        var processor = new HtmlProcessor(settings);
        string rawHtml = $"<body><a href=\"https://docs.google.com/document/d/{docId}/edit\">Bylaws</a></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("href=\"/bylaws\"");
        result.ShouldNotContain("docs.google.com");
    }

    [Fact(DisplayName = "Replaces Google Docs URL with user ID in path")]
    public void ProcessExportedHtml_WithGoogleDocsLinkWithUserId_ReplacesWithInternalRoute()
    {
        // Arrange
        const string docId = "1abc123def456";
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create(("tournament-rules", docId, "/tournaments/rules"));
        var processor = new HtmlProcessor(settings);
        string rawHtml = $"<body><a href=\"https://docs.google.com/document/u/0/d/{docId}/edit\">Rules</a></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("href=\"/tournament-rules\"");
        result.ShouldNotContain("docs.google.com");
    }

    [Fact(DisplayName = "Preserves unknown Google Docs URLs")]
    public void ProcessExportedHtml_WithUnknownGoogleDocsLink_PreservesOriginalUrl()
    {
        // Arrange
        const string unknownDocId = "unknown123";
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create(("bylaws", "known123", "/bylaws"));
        var processor = new HtmlProcessor(settings);
        string rawHtml = $"<body><a href=\"https://docs.google.com/document/d/{unknownDocId}/edit\">Unknown</a></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain($"https://docs.google.com/document/d/{unknownDocId}/edit");
    }

    [Fact(DisplayName = "Generates human-readable IDs for headings")]
    public void ProcessExportedHtml_WithHeadingId_GeneratesHumanReadableId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">Tournament Rules</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"tournament-rules\"");
        result.ShouldContain("data-original-id=\"h.abc123\"");
    }

    [Fact(DisplayName = "Generates unique IDs for duplicate headings")]
    public void ProcessExportedHtml_WithDuplicateHeadings_GeneratesUniqueIds()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">Section</h1><h1 id=\"h.def456\">Section</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"section\"");
        result.ShouldContain("id=\"section-1\"");
    }

    [Fact(DisplayName = "Updates anchor links to use new heading IDs")]
    public void ProcessExportedHtml_WithAnchorLink_UpdatesToNewId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">Tournament Rules</h1><a href=\"#h.abc123\">Go to rules</a></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("href=\"#tournament-rules\"");
        result.ShouldNotContain("href=\"#h.abc123\"");
    }

    [Fact(DisplayName = "Handles bookmark spans with link text")]
    public void ProcessExportedHtml_WithBookmarkSpanAndLink_GeneratesIdFromLinkText()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><span id=\"h.bookmark1\"></span><a href=\"#h.bookmark1\">Important Section</a></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"important-section\"");
        result.ShouldContain("href=\"#important-section\"");
    }

    [Fact(DisplayName = "Handles bookmark spans without link text")]
    public void ProcessExportedHtml_WithBookmarkSpanWithoutLink_UsesOriginalId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><span id=\"h.orphanbookmark\"></span></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"orphanbookmark\"");
        result.ShouldContain("data-original-id=\"h.orphanbookmark\"");
    }

    [Fact(DisplayName = "Handles bookmark anchor elements")]
    public void ProcessExportedHtml_WithBookmarkAnchorElement_GeneratesHumanReadableId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><a id=\"h.anchormark\"></a><a href=\"#h.anchormark\">Link Text</a></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"link-text\"");
        result.ShouldContain("href=\"#link-text\"");
    }

    [Fact(DisplayName = "Removes special characters from generated slugs")]
    public void ProcessExportedHtml_WithSpecialCharactersInHeading_RemovesSpecialCharacters()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">Section 10.3: Hall of Fame!</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"section-10.3-hall-of-fame\"");
    }

    [Fact(DisplayName = "Handles empty heading text")]
    public void ProcessExportedHtml_WithEmptyHeadingText_UsesFallbackId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.empty\">   </h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"section\"");
    }

    [Fact(DisplayName = "Strips HTML tags from heading content for ID generation")]
    public void ProcessExportedHtml_WithHtmlTagsInHeading_StripsTagsForId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\"><strong>Bold</strong> Title</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"bold-title\"");
    }

    [Fact(DisplayName = "Handles body tag with attributes")]
    public void ProcessExportedHtml_WithBodyTagAttributes_ExtractsBodyContent()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<html><body class=\"docs-body\" style=\"margin:0\"><p>Content</p></body></html>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("<p>Content</p>");
        result.ShouldNotContain("<body");
    }

    [Fact(DisplayName = "Handles multiline body content")]
    public void ProcessExportedHtml_WithMultilineBodyContent_ExtractsAllContent()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = @"<html>
<body>
<p>Line 1</p>
<p>Line 2</p>
</body>
</html>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("<p>Line 1</p>");
        result.ShouldContain("<p>Line 2</p>");
    }

    [Fact(DisplayName = "Replaces multiple consecutive hyphens with single hyphen")]
    public void ProcessExportedHtml_WithMultipleSpacesInHeading_CollapsesToSingleHyphen()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">Section   Title</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"section-title\"");
        result.ShouldNotContain("section---title");
    }

    [Fact(DisplayName = "Trims hyphens from start and end of generated ID")]
    public void ProcessExportedHtml_WithLeadingTrailingSpecialChars_TrimsHyphens()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">- Title -</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"title\"");
        result.ShouldNotContain("id=\"-title-\"");
    }

    [Fact(DisplayName = "Decodes HTML entities in heading text for ID generation")]
    public void ProcessExportedHtml_WithHtmlEntitiesInHeading_DecodesEntities()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">Q&amp;A Section</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"qa-section\"");
    }

    [Fact(DisplayName = "Handles bookmark ID without h. prefix")]
    public void ProcessExportedHtml_WithBookmarkIdWithoutPrefix_HandlesGracefully()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><span id=\"custom-bookmark\"></span></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"custom-bookmark\"");
    }

    [Fact(DisplayName = "Processes multiple Google Docs links in same document")]
    public void ProcessExportedHtml_WithMultipleGoogleDocsLinks_ReplacesAll()
    {
        // Arrange
        const string docId1 = "doc1abc";
        const string docId2 = "doc2def";
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create(
            ("bylaws", docId1, "/bylaws"),
            ("rules", docId2, "/rules"));
        var processor = new HtmlProcessor(settings);
        string rawHtml = $"<body><a href=\"https://docs.google.com/document/d/{docId1}/edit\">Bylaws</a><a href=\"https://docs.google.com/document/d/{docId2}/edit\">Rules</a></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("href=\"/bylaws\"");
        result.ShouldContain("href=\"/rules\"");
    }

    [Fact(DisplayName = "Converts uppercase heading text to lowercase ID")]
    public void ProcessExportedHtml_WithUppercaseHeading_GeneratesLowercaseId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">ARTICLE VII - HALL OF FAME</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"article-vii-hall-of-fame\"");
    }

    [Fact(DisplayName = "Handles heading with only numbers")]
    public void ProcessExportedHtml_WithNumericOnlyHeading_GeneratesValidId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml = "<body><h1 id=\"h.abc123\">2024</h1></body>";

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"2024\"");
    }

    [Fact(DisplayName = "Handles h2 through h6 headings")]
    public void ProcessExportedHtml_WithVariousHeadingLevels_ProcessesAllLevels()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var processor = new HtmlProcessor(settings);
        const string rawHtml =
        """
            <body>
                <h2 id="h.h2">Heading 2</h2>
                <h3 id="h.h3">Heading 3</h3>
                <h4 id="h.h4">Heading 4</h4>
                <h5 id="h.h5">Heading 5</h5>
                <h6 id="h.h6">Heading 6</h6>
            </body>
        """;

        // Act
        string result = processor.ProcessExportedHtml(rawHtml);

        // Assert
        result.ShouldContain("id=\"heading-2\"");
        result.ShouldContain("id=\"heading-3\"");
        result.ShouldContain("id=\"heading-4\"");
        result.ShouldContain("id=\"heading-5\"");
        result.ShouldContain("id=\"heading-6\"");
    }
}
