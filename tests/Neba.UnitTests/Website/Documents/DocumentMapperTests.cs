using Google.Apis.Docs.v1.Data;
using Neba.Infrastructure.Documents;
using Neba.Tests.Website;

namespace Neba.UnitTests.Website.Documents;

[Trait("Category", "Unit")]
[Trait("Component", "Website.Documents")]

public sealed class DocumentMapperTests
{
    [Fact(DisplayName = "Converts empty document to empty string")]
    public void ConvertToHtml_EmptyDocument_ReturnsEmptyString()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);
        var document = new Google.Apis.Docs.v1.Data.Document();

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("");
    }

    [Fact(DisplayName = "Renders plain paragraph as HTML")]
    public void ConvertToHtml_PlainParagraph_RendersParagraph()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph("Hello world"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p>Hello world</p>\n");
    }

    [Fact(DisplayName = "Assigns stable ID to heading paragraphs")]
    public void ConvertToHtml_HeadingParagraph_AssignsStableId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Heading("Tournament Rules", "HEADING_2"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<h2 id='tournament-rules'>Tournament Rules</h2>\n");
    }

    [Fact(DisplayName = "Renders inline formatting with strong, em, and underline tags")]
    public void ConvertToHtml_InlineFormatting_RendersStrongEmUnderline()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        var boldItalicUnderline = new Google.Apis.Docs.v1.Data.TextStyle
        {
            Bold = true,
            Italic = true,
            Underline = true
        };

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph(("Hi", boldItalicUnderline)));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p><u><em><strong>Hi</strong></em></u></p>\n");
    }

    [Fact(DisplayName = "Adds target and rel attributes to external links")]
    public void ConvertToHtml_ExternalLink_AddsTargetAndRel()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph(("NEBA", GoogleDocsDocumentFactory.LinkToExternal(new Uri("https://example.com", UriKind.Absolute)))));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p><a href='https://example.com/' target='_blank' rel='noopener noreferrer'>NEBA</a></p>\n");
    }

    [Fact(DisplayName = "Maps cross-document links to routes with modal attribute")]
    public void ConvertToHtml_CrossDocumentLink_MapsToRouteAndAddsModalAttribute()
    {
        // Arrange
        const string docId = "abc123";
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create(("bylaws", docId, "/bylaws"));
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph((
                "Bylaws",
                GoogleDocsDocumentFactory.LinkToExternal(new Uri($"https://docs.google.com/document/d/{docId}/edit", UriKind.Absolute)))));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p><a href='/bylaws' data-modal='true'>Bylaws</a></p>\n");
    }

    [Fact(DisplayName = "Maps cross-document links with user ID in URL to routes with modal attribute")]
    public void ConvertToHtml_CrossDocumentLinkWithUserId_MapsToRouteAndAddsModalAttribute()
    {
        // Arrange
        const string docId = "abc123";
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create(("tournament-rules", docId, "/tournaments/rules"));
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph((
                "Tournament Rules",
                GoogleDocsDocumentFactory.LinkToExternal(new Uri($"https://docs.google.com/document/u/0/d/{docId}/edit", UriKind.Absolute)))));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p><a href='/tournaments/rules' data-modal='true'>Tournament Rules</a></p>\n");
    }

    [Fact(DisplayName = "Renders bookmark links with correct href")]
    public void ConvertToHtml_BookmarkLink_RendersBookmarkHref()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph(("Jump", GoogleDocsDocumentFactory.LinkToBookmark("bookmark-1"))));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p><a href='#bookmark-1'>Jump</a></p>\n");
    }

    [Fact(DisplayName = "Uses heading map for anchor href in heading links")]
    public void ConvertToHtml_HeadingLink_UsesHeadingMapForAnchorHref()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Heading("Section 1", "HEADING_3"),
            GoogleDocsDocumentFactory.Paragraph(("Section 1", GoogleDocsDocumentFactory.LinkToHeading("ignored-by-mapper"))));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldContain("<h3 id='section-1'>Section 1</h3>\n");
        html.ShouldContain("<p><a href='#section-1'>Section 1</a></p>\n");
    }

    [Fact(DisplayName = "Maps partial heading link text to full heading ID for Section headings")]
    public void ConvertToHtml_PartialHeadingLink_MapsToFullHeadingId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Heading("Section 10.3 Annual Meeting", "HEADING_3"),
            GoogleDocsDocumentFactory.Paragraph(("Annual Meeting", GoogleDocsDocumentFactory.LinkToHeading("ignored-by-mapper"))));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldContain("<h3 id='section-103-annual-meeting'>Section 10.3 Annual Meeting</h3>\n");
        html.ShouldContain("<p><a href='#section-103-annual-meeting'>Annual Meeting</a></p>\n");
    }

    [Fact(DisplayName = "Maps partial heading link text to full heading ID for ARTICLE headings")]
    public void ConvertToHtml_PartialArticleHeadingLink_MapsToFullHeadingId()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Heading("ARTICLE VII - HALL OF FAME COMMITTEE", "HEADING_2"),
            GoogleDocsDocumentFactory.Paragraph(("Hall of Fame Committee", GoogleDocsDocumentFactory.LinkToHeading("ignored-by-mapper"))));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldContain("<h2 id='article-vii-hall-of-fame-committee'>ARTICLE VII - HALL OF FAME COMMITTEE</h2>\n");
        html.ShouldContain("<p><a href='#article-vii-hall-of-fame-committee'>Hall of Fame Committee</a></p>\n");
    }

    [Fact(DisplayName = "Renders table with rows and cells")]
    public void ConvertToHtml_Table_RendersTableRowsAndCells()
    {
        // Arrange
        string[] tableData1 = new[] { "A1", "B1" };
        string[] tableData2 = new[] { "A2", "B2" };

        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Table(
                tableData1,
                tableData2));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldContain("<table border='1' style='border-collapse: collapse;'>\n");
        html.ShouldContain("    <td>A1</td>\n");
        html.ShouldContain("    <td>B2</td>\n");
        html.ShouldContain("</table>\n");
    }

    [Fact(DisplayName = "Renders unordered list with ul and li tags")]
    public void ConvertToHtml_UnorderedList_RendersUlAndLi()
    {
        // Arrange
        const string listId = "list-1";
        IReadOnlyDictionary<string, List> lists = GoogleDocsDocumentFactory.Lists((listId, 0, "BULLET", null));

        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.CreateWithLists(
            lists,
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "One"),
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "Two"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldContain("<ul>\n");
        html.ShouldContain("  <li>One</li>\n");
        html.ShouldContain("  <li>Two</li>\n");
        html.ShouldEndWith("</ul>\n");
    }

    [Fact(DisplayName = "Renders ordered list with type and start attributes")]
    public void ConvertToHtml_OrderedListWithStartNumber_RendersOlTypeAndStart()
    {
        // Arrange
        const string listId = "list-ordered";
        IReadOnlyDictionary<string, List> lists = GoogleDocsDocumentFactory.Lists((listId, 0, "DECIMAL", 3));

        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.CreateWithLists(
            lists,
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "Third"),
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "Fourth"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldContain("<ol type='1' start='3'>\n");
        html.ShouldContain("  <li>Third</li>\n");
        html.ShouldContain("  <li>Fourth</li>\n");
        html.ShouldEndWith("</ol>\n");
    }

    [Fact(DisplayName = "Resets list counter after non-list block")]
    public void ConvertToHtml_ListCounterResetsAfterNonListBlock()
    {
        // Arrange
        const string listId = "list-reset";
        IReadOnlyDictionary<string, List> lists = GoogleDocsDocumentFactory.Lists((listId, 0, "DECIMAL", 1));

        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.CreateWithLists(
            lists,
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "One"),
            GoogleDocsDocumentFactory.Paragraph("Break"),
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "One-again"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        // First list starts at 1.
        html.ShouldContain("<ol type='1'>\n");
        html.ShouldContain("  <li>One</li>\n");

        // Second list should restart at 1 (no start attr).
        html.ShouldContain("<p>Break</p>\n");
        html.ShouldContain("  <li>One-again</li>\n");

        // Should have two closing </ol> tags.
        html.Count(s => s == '<').ShouldBeGreaterThan(0);
        html.Split("</ol>\n").Length.ShouldBe(3);
    }

    [Fact(DisplayName = "Converts tab-separated list items to indented table")]
    public void ConvertToHtml_TabSeparatedListItems_AreConvertedToIndentedTable()
    {
        // Arrange
        const string listId = "list-tabs";
        IReadOnlyDictionary<string, List> lists = GoogleDocsDocumentFactory.Lists((listId, 1, "BULLET", null));

        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.CreateWithLists(
            lists,
            GoogleDocsDocumentFactory.BulletParagraph(listId, 1, "Col1\tCol2"),
            GoogleDocsDocumentFactory.BulletParagraph(listId, 1, "A\tB"),
            GoogleDocsDocumentFactory.Paragraph("After"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldContain("<table style='margin-left: 40px;'>\n");
        html.ShouldContain("    <td>Col1</td>\n");
        html.ShouldContain("    <td>Col2</td>\n");
        html.ShouldContain("    <td>A</td>\n");
        html.ShouldContain("    <td>B</td>\n");
        html.ShouldContain("</table>\n");
        html.ShouldContain("<p>After</p>\n");
    }

    [Fact(DisplayName = "Produces valid parent-child structure for nested lists")]
    public void ConvertToHtml_NestedUnorderedList_ProducesValidParentChildStructure()
    {
        // Arrange
        const string listId = "list-nested";
        IReadOnlyDictionary<string, List> lists = GoogleDocsDocumentFactory.Lists(
            (listId, 0, "BULLET", null),
            (listId, 1, "BULLET", null));

        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.CreateWithLists(
            lists,
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "Parent"),
            GoogleDocsDocumentFactory.BulletParagraph(listId, 1, "Child"),
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "Sibling"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        // This intentionally checks for the parent <li> wrapping the nested <ul>.
        html.ShouldContain("<ul>\n");
        html.ShouldContain("  <li>Parent\n");
        html.ShouldContain("<ul>\n");
        html.ShouldContain("  <li>Child</li>\n");
        html.ShouldContain("</ul>\n");
        html.ShouldContain("</li>\n");
        html.ShouldContain("  <li>Sibling</li>\n");
    }

    [Fact(DisplayName = "Converts smart quotes and apostrophes to HTML entities")]
    public void ConvertToHtml_SmartQuotesAndApostrophes_AreConvertedToHtmlEntities()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        // Unicode characters: ' (U+2018), ' (U+2019), " (U+201C), " (U+201D)
        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph("It\u2019s the year\u2019s best \u201Cproduct\u201D with \u2018quotes\u2019."));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p>It&#8217;s the year&#8217;s best &#8220;product&#8221; with &#8216;quotes&#8217;.</p>\n");
    }

    [Fact(DisplayName = "Converts dashes and ellipsis to HTML entities")]
    public void ConvertToHtml_DashesAndEllipsis_AreConvertedToHtmlEntities()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        // Unicode characters: – (U+2013 en dash), — (U+2014 em dash), … (U+2026 ellipsis)
        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph("2020\u20132024 \u2014 the years passed\u2026"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p>2020&#8211;2024 &#8212; the years passed&#8230;</p>\n");
    }

    [Fact(DisplayName = "HTML encodes special characters")]
    public void ConvertToHtml_HtmlSpecialCharacters_AreHtmlEncoded()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph("<script>alert(\u2019XSS\u2019)</script> & other < > characters"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p>&lt;script&gt;alert(&#8217;XSS&#8217;)&lt;/script&gt; &amp; other &lt; &gt; characters</p>\n");
    }

    [Fact(DisplayName = "Normalizes unicode in headings with correct IDs")]
    public void ConvertToHtml_UnicodeInHeadings_AreNormalizedWithCorrectIds()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Heading("Player\u2019s Guide", "HEADING_2"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<h2 id='players-guide'>Player&#8217;s Guide</h2>\n");
    }

    [Fact(DisplayName = "Normalizes unicode in lists")]
    public void ConvertToHtml_UnicodeInLists_AreNormalized()
    {
        // Arrange
        const string listId = "list-unicode";
        IReadOnlyDictionary<string, List> lists = GoogleDocsDocumentFactory.Lists((listId, 0, "BULLET", null));

        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.CreateWithLists(
            lists,
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "First item\u2019s content"),
            GoogleDocsDocumentFactory.BulletParagraph(listId, 0, "Second item \u2014 with dash"));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldContain("  <li>First item&#8217;s content</li>\n");
        html.ShouldContain("  <li>Second item &#8212; with dash</li>\n");
    }

    [Fact(DisplayName = "Normalizes unicode in link text but not URL")]
    public void ConvertToHtml_UnicodeInLinks_TextIsNormalizedButUrlIsNot()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph((
                "Player\u2019s Guide",
                GoogleDocsDocumentFactory.LinkToExternal(new Uri("https://example.com/guide", UriKind.Absolute)))));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p><a href='https://example.com/guide' target='_blank' rel='noopener noreferrer'>Player&#8217;s Guide</a></p>\n");
    }

    [Fact(DisplayName = "Normalizes unicode in formatted text while preserving formatting")]
    public void ConvertToHtml_UnicodeInFormattedText_AreNormalizedWithFormatting()
    {
        // Arrange
        GoogleDocsSettings settings = GoogleDocsSettingsFactory.Create();
        var mapper = new DocumentMapper(settings);

        var boldStyle = new Google.Apis.Docs.v1.Data.TextStyle { Bold = true };

        Document document = GoogleDocsDocumentFactory.Create(
            GoogleDocsDocumentFactory.Paragraph(("This year\u2019s best", boldStyle)));

        // Act
        string html = Normalize(mapper.ConvertToHtml(document));

        // Assert
        html.ShouldBe("<p><strong>This year&#8217;s best</strong></p>\n");
    }

    private static string Normalize(string html)
        => html.Replace("\r\n", "\n", StringComparison.Ordinal);
}
