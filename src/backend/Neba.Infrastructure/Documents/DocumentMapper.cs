using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Google.Apis.Docs.v1.Data;

namespace Neba.Infrastructure.Documents;

internal sealed class DocumentMapper(GoogleDocsSettings settings)
{
    private const string ClosingOrderedList = "</ol>";
    private const string ClosingUnorderedList = "</ul>";
    private const string OpeningOrderedList = "<ol";
    private const string OpeningUnorderedList = "<ul>";
    private const string ListItem = "  <li>";
    private const string ClosingListItem = "</li>";
    private const string TableTag = "<table";
    private const string ClosingTableTag = "</table>";
    private const string TableRow = "  <tr>";
    private const string ClosingTableRow = "  </tr>";
    private const string TableCell = "    <td>";
    private const string ClosingTableCell = "</td>";
    private const string HeadingPrefix = "HEADING_";

    private readonly Dictionary<string, Dictionary<int, int>> _listCounters = [];
    private readonly Dictionary<string, string> _headingIds = [];
    private readonly Dictionary<string, string> _documentRouteMap
        = settings.Documents.ToDictionary(d => d.DocumentId, d => d.Route);

    /// <summary>
    /// Normalizes text content by replacing Unicode special characters with HTML entities.
    /// This prevents encoding issues when displaying content in browsers.
    /// </summary>
    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // First, HTML encode to prevent XSS and handle basic special characters
        text = HttpUtility.HtmlEncode(text);

        // Then replace smart quotes and other typographic characters with their HTML entity equivalents
        // (HtmlEncode doesn't handle these, leaving them as UTF-8 which can cause display issues)
        text = text.Replace("\u2018", "&#8216;", StringComparison.Ordinal); // Left single quotation mark
        text = text.Replace("\u2019", "&#8217;", StringComparison.Ordinal); // Right single quotation mark (apostrophe)
        text = text.Replace("\u201C", "&#8220;", StringComparison.Ordinal); // Left double quotation mark
        text = text.Replace("\u201D", "&#8221;", StringComparison.Ordinal); // Right double quotation mark
        text = text.Replace("\u2013", "&#8211;", StringComparison.Ordinal); // En dash
        text = text.Replace("\u2014", "&#8212;", StringComparison.Ordinal); // Em dash
        text = text.Replace("\u2026", "&#8230;", StringComparison.Ordinal); // Horizontal ellipsis

        return text;
    }

    public string ConvertToHtml(Document document)
    {
        // First pass: collect all headings to build an anchor map
        BuildHeadingMap(document);

        var html = new StringBuilder();
        var listStack = new Stack<(string ListId, int Level, bool IsOrdered, string? GlyphFormat)>();
        var context = new ListProcessingContext(html, listStack);

        if (document.Body?.Content != null)
        {
            // Use while loop to properly handle tabular lists that consume multiple elements
            int i = 0;
            while (i < document.Body.Content.Count)
            {
                StructuralElement element = document.Body.Content[i];

                if (element.Table != null)
                {
                    CloseAllLists(context.ListStack, html, context.HasOpenListItem);
                    context.HasOpenListItem = false;
                    context.PreviousListId = null;
                    context.PreviousLevel = null;
                    html.AppendLine(ConvertTableToHtml(element.Table));
                    i++;
                }
                else if (element.Paragraph != null)
                {
                    Paragraph paragraph = element.Paragraph;
                    Bullet bullet = paragraph.Bullet;

                    if (bullet != null)
                    {
                        int elementsConsumed = ProcessListItem(
                            document,
                            context,
                            paragraph,
                            bullet,
                            i);

                        i += elementsConsumed + 1; // Move past current element and any consumed elements
                    }
                    else
                    {
                        CloseAllLists(context.ListStack, html, context.HasOpenListItem);
                        context.HasOpenListItem = false;
                        context.PreviousListId = null;
                        context.PreviousLevel = null;
                        html.AppendLine(ConvertParagraphToHtml(paragraph));
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }

            // Close any remaining open lists
            CloseAllLists(context.ListStack, html, context.HasOpenListItem);
        }

        return html.ToString();
    }

    private sealed class ListProcessingContext(StringBuilder html, Stack<(string ListId, int Level, bool IsOrdered, string? GlyphFormat)> listStack)
    {
        public StringBuilder Html { get; } = html;
        public Stack<(string ListId, int Level, bool IsOrdered, string? GlyphFormat)> ListStack { get; } = listStack;
        public string? PreviousListId { get; set; }
        public int? PreviousLevel { get; set; }
        public bool HasOpenListItem { get; set; }
    }

    private int ProcessListItem(
        Document document,
        ListProcessingContext context,
        Paragraph paragraph,
        Bullet bullet,
        int currentIndex)
    {
        string listId = bullet.ListId;
        int nestingLevel = bullet.NestingLevel ?? 0;

        InitializeListCounter(document, listId, nestingLevel);

        (bool IsOrdered, string? GlyphFormat) glyphInfo = GetListGlyphInfo(document, listId, nestingLevel);
        bool isOrdered = glyphInfo.IsOrdered;
        string? glyphFormat = glyphInfo.GlyphFormat;

        string itemText = ExtractTextFromParagraph(paragraph, processLinks: true);
        bool containsTabs = itemText.Contains('\t', StringComparison.Ordinal);

        if (containsTabs)
        {
            return HandleTabularListItems(
                document,
                context,
                itemText,
                listId,
                nestingLevel,
                currentIndex);
        }

        bool needsNewList = context.PreviousListId != listId ||
                           context.PreviousLevel != nestingLevel ||
                           context.ListStack.Count == 0;

        // Check if next item is a nested list (deeper level with same listId)
        bool hasNestedList = false;
        if (currentIndex + 1 < document.Body.Content.Count)
        {
            var nextElement = document.Body.Content[currentIndex + 1];
            if (nextElement.Paragraph?.Bullet != null)
            {
                var nextBullet = nextElement.Paragraph.Bullet;
                int nextNestingLevel = nextBullet.NestingLevel ?? 0;
                hasNestedList = nextBullet.ListId == listId && nextNestingLevel > nestingLevel;
            }
        }

        if (needsNewList)
        {
            HandleNewList(context.Html, context.ListStack, listId, nestingLevel, isOrdered, glyphFormat, context.HasOpenListItem);
            context.HasOpenListItem = false;
        }
        else
        {
            // Close previous list item if one is open
            if (context.HasOpenListItem)
            {
                context.Html.AppendLine(ClosingListItem);
                context.HasOpenListItem = false;
            }
            _listCounters[listId][nestingLevel]++;
        }

        // Write the list item opening and content
        context.Html.Append(CultureInfo.InvariantCulture, $"{ListItem}{itemText}");

        // Only close the list item if there's no nested list coming
        if (!hasNestedList)
        {
            context.Html.AppendLine(ClosingListItem);
            context.HasOpenListItem = false;
        }
        else
        {
            context.Html.AppendLine(); // Just add newline, keep <li> open
            context.HasOpenListItem = true;
        }

        context.PreviousListId = listId;
        context.PreviousLevel = nestingLevel;

        return 0; // No additional elements consumed
    }

    private void InitializeListCounter(Document document, string listId, int nestingLevel)
    {
        if (!_listCounters.TryGetValue(listId, out Dictionary<int, int>? levelCounters))
        {
            levelCounters = new Dictionary<int, int>();
            _listCounters[listId] = levelCounters;
        }

        if (!levelCounters.ContainsKey(nestingLevel))
        {
            int? startNumber = GetListItemStartNumber(document, listId, nestingLevel);
            // Initialize to startNumber - 1 because we increment BEFORE rendering
            levelCounters[nestingLevel] = (startNumber ?? 1) - 1;
        }
    }

    private int HandleTabularListItems(
        Document document,
        ListProcessingContext context,
        string itemText,
        string listId,
        int nestingLevel,
        int currentIndex)
    {
        CloseAllLists(context.ListStack, context.Html, context.HasOpenListItem);
        context.HasOpenListItem = false;
        context.PreviousListId = null;
        context.PreviousLevel = null;

        var tableRows = new List<string> { itemText };
        int elementsToSkip = 0;

        for (int lookAheadIndex = currentIndex + 1; lookAheadIndex < document.Body.Content.Count; lookAheadIndex++)
        {
            StructuralElement nextElement = document.Body.Content[lookAheadIndex];

            if (nextElement.Paragraph?.Bullet != null &&
                nextElement.Paragraph.Bullet.ListId == listId &&
                (nextElement.Paragraph.Bullet.NestingLevel ?? 0) == nestingLevel)
            {
                string nextItemText = ExtractTextFromParagraph(nextElement.Paragraph, processLinks: true);

                if (nextItemText.Contains('\t', StringComparison.Ordinal))
                {
                    tableRows.Add(nextItemText);
                    elementsToSkip++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        context.Html.AppendLine(ConvertTabularListToTable(tableRows, nestingLevel));

        // Ensure counter is initialized before incrementing
        if (!_listCounters.TryGetValue(listId, out Dictionary<int, int>? levelCounters))
        {
            levelCounters = [];
            _listCounters[listId] = levelCounters;
        }
        if (!levelCounters.ContainsKey(nestingLevel))
        {
            int? startNumber = GetListItemStartNumber(document, listId, nestingLevel);
            levelCounters[nestingLevel] = (startNumber ?? 1) - 1;
        }

        levelCounters[nestingLevel] += tableRows.Count;

        return elementsToSkip;
    }

    private void HandleNewList(
        StringBuilder html,
        Stack<(string ListId, int Level, bool IsOrdered, string? GlyphFormat)> listStack,
        string listId,
        int nestingLevel,
        bool isOrdered,
        string? glyphFormat,
        bool hasOpenListItem)
    {
        // Close deeper or different lists
        while (listStack.Count > 0)
        {
            (string ListId, int Level, bool IsOrdered, string? GlyphFormat) peek = listStack.Peek();
            if (peek.ListId == listId && peek.Level < nestingLevel)
            {
                // This is a parent list, keep it open (nested list case)
                break;
            }

            // Close the list
            (string ListId, int Level, bool IsOrdered, string? GlyphFormat) item = listStack.Pop();
            html.AppendLine(item.IsOrdered ? ClosingOrderedList : ClosingUnorderedList);

            // If there was an open list item from the parent level, close it now
            if (hasOpenListItem && listStack.Count > 0)
            {
                html.AppendLine(ClosingListItem);
                hasOpenListItem = false;
            }
        }

        // Increment counter BEFORE opening the list
        _listCounters[listId][nestingLevel]++;

        // Open new list
        if (isOrdered)
        {
            int currentCount = _listCounters[listId][nestingLevel];
            string listTypeAttr = GetListTypeAttribute(glyphFormat);
            string startAttr = currentCount != 1 ? $" start='{currentCount}'" : "";
            html.AppendLine(CultureInfo.InvariantCulture, $"{OpeningOrderedList}{listTypeAttr}{startAttr}>");
        }
        else
        {
            html.AppendLine(OpeningUnorderedList);
        }

        listStack.Push((listId, nestingLevel, isOrdered, glyphFormat));
    }

    private void CloseAllLists(Stack<(string ListId, int Level, bool IsOrdered, string? GlyphFormat)> listStack, StringBuilder html, bool hasOpenListItem)
    {
        // First close any open list item
        if (hasOpenListItem)
        {
            html.AppendLine(ClosingListItem);
        }

        // Collect unique listIds that are being closed
        var closedListIds = new HashSet<string>();

        // Then close all open lists
        while (listStack.Count > 0)
        {
            (string ListId, int Level, bool IsOrdered, string? GlyphFormat) item = listStack.Pop();
            html.AppendLine(item.IsOrdered ? ClosingOrderedList : ClosingUnorderedList);
            closedListIds.Add(item.ListId);
        }

        // Reset counters for all levels of lists that were completely closed
        // This ensures that if we encounter this list again later in the document,
        // it starts from the beginning instead of continuing from where it left off
        foreach (string listId in closedListIds)
        {
            _listCounters.Remove(listId);
        }
    }

    private static string? ExtractDocIdFromGoogleDocsUrl(string url)
    {
        // Match Google Docs URLs like:
        // https://docs.google.com/document/d/DOCUMENT_ID/edit
        // https://docs.google.com/document/d/DOCUMENT_ID/
        Match match = Regex.Match(url, @"docs\.google\.com/document/d/([a-zA-Z0-9_-]+)", RegexOptions.None, TimeSpan.FromSeconds(1));
        return match.Success ? match.Groups[1].Value : null;
    }

    private (string Href, bool IsExternal, bool IsModal) ProcessLinkUrl(string url, string? headingId, string? bookmarkId, string linkText)
    {
        string href;
        bool isExternal = false;
        bool isModal = false;

        if (!string.IsNullOrEmpty(headingId))
        {
            // Internal link to heading within current document
            string trimmedLinkText = linkText.Trim();
            if (_headingIds.TryGetValue(trimmedLinkText, out string? existingId))
            {
                href = $"#{existingId}";
            }
            else
            {
                href = $"#{CreateHeadingId(trimmedLinkText)}";
            }
        }
        else if (!string.IsNullOrEmpty(bookmarkId))
        {
            // Internal bookmark link
            href = $"#{bookmarkId}";
        }
        else if (!string.IsNullOrEmpty(url))
        {
            // Check if this is a Google Docs link that maps to our site
            string? docId = ExtractDocIdFromGoogleDocsUrl(url);
            if (docId != null && _documentRouteMap?.ContainsKey(docId) == true)
            {
                // Cross-document link to another document in our system
                href = _documentRouteMap[docId];
                isModal = true;
            }
            else
            {
                // External URL
                href = url;
                isExternal = true;
            }
        }
        else
        {
            href = "#";
        }

        return (href, isExternal, isModal);
    }

    private static string ConvertTabularListToTable(List<string> rows, int nestingLevel)
    {
        var tableHtml = new StringBuilder();

        // Calculate indentation based on nesting level (e.g., 40px per level)
        int indentPx = nestingLevel * 40;
        string styleAttr = indentPx > 0 ? $" style='margin-left: {indentPx}px;'" : "";

        tableHtml.AppendLine(CultureInfo.InvariantCulture, $"{TableTag}{styleAttr}>");

        foreach (string row in rows)
        {
            // Split by tabs
            string[] columns = row.Split('\t');

            tableHtml.AppendLine("  <tr>");
            foreach (string column in columns)
            {
                string trimmedColumn = column.Trim();
                if (!string.IsNullOrEmpty(trimmedColumn))
                {
                    tableHtml.AppendLine(CultureInfo.InvariantCulture, $"{TableCell}{trimmedColumn}{ClosingTableCell}");
                }
            }
            tableHtml.AppendLine("  </tr>");
        }

        tableHtml.AppendLine("</table>");
        return tableHtml.ToString();
    }

    private void BuildHeadingMap(Document document)
    {
        if (document.Body?.Content == null) return;

        IEnumerable<Paragraph> paragraphs = document.Body.Content
            .Where(e => e.Paragraph != null)
            .Select(e => e.Paragraph!);

        foreach (Paragraph paragraph in paragraphs)
        {
            ParagraphStyle style = paragraph.ParagraphStyle;

            // Check if this is a heading
            if (style?.NamedStyleType != null && style.NamedStyleType.StartsWith(HeadingPrefix, StringComparison.Ordinal))
            {
                string headingText = GetPlainTextFromParagraph(paragraph);
                if (!string.IsNullOrWhiteSpace(headingText))
                {
                    string trimmedText = headingText.Trim();
                    // Store the mapping from heading text to ID if not already present
                    if (!_headingIds.ContainsKey(trimmedText))
                    {
                        string headingId = CreateHeadingId(headingText);
                        _headingIds[trimmedText] = headingId;
                    }
                }
            }
        }
    }

#pragma warning disable CA1308 // Normalize to lowercase for IDs
    private static string CreateHeadingId(string headingText)
    {
        // Convert to lowercase and replace spaces/special chars with hyphens
        string id = headingText.Trim()
            .ToLowerInvariant()
            .Replace(" ", "-", StringComparison.Ordinal)
            .Replace("'", "", StringComparison.Ordinal)
            .Replace("\"", "", StringComparison.Ordinal);

        // Remove any characters that aren't alphanumeric or hyphens
        id = new string(id.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Remove consecutive hyphens
        while (id.Contains("--", StringComparison.Ordinal))
        {
            id = id.Replace("--", "-", StringComparison.Ordinal);
        }

        return id.Trim('-');
    }

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
    private static string GetPlainTextFromParagraph(Paragraph paragraph)
    {
        var text = new StringBuilder();

        if (paragraph.Elements != null)
        {
            foreach (ParagraphElement? element in paragraph.Elements)
            {
                if (element.TextRun?.Content != null)
                {
                    text.Append(element.TextRun.Content);
                }
            }
        }

        return text.ToString();
    }

    private static int? GetListItemStartNumber(Document document, string listId, int nestingLevel)
    {
        if (document.Lists != null &&
            document.Lists.TryGetValue(listId, out List? list) &&
            list?.ListProperties?.NestingLevels != null &&
            nestingLevel < list.ListProperties.NestingLevels.Count)
        {
            NestingLevel nestingLevelProps = list.ListProperties.NestingLevels[nestingLevel];
            return nestingLevelProps.StartNumber;
        }

        return null;
    }

    private static (bool IsOrdered, string? GlyphFormat) GetListGlyphInfo(Document document, string listId, int nestingLevel)
    {
        if (document.Lists != null &&
            document.Lists.TryGetValue(listId, out List? list) &&
            list?.ListProperties?.NestingLevels != null &&
            nestingLevel < list.ListProperties.NestingLevels.Count)
        {
            NestingLevel nestingLevelProps = list.ListProperties.NestingLevels[nestingLevel];
            string glyphType = nestingLevelProps.GlyphType;

            if (glyphType != null)
            {
                bool isOrdered = glyphType != "GLYPH_TYPE_UNSPECIFIED" &&
                                !glyphType.Contains("BULLET", StringComparison.OrdinalIgnoreCase);
                return (isOrdered, glyphType);
            }
        }

        return (false, null);
    }

    private static string GetListTypeAttribute(string? glyphFormat)
    {
        if (string.IsNullOrEmpty(glyphFormat))
            return "";

        // Map Google Docs glyph types to HTML list-style-type
        return glyphFormat switch
        {
            "DECIMAL" => " type='1'",                    // 1, 2, 3
            "ZERO_DECIMAL" => " type='1'",               // 01, 02, 03 (HTML doesn't support this exactly)
            "ALPHA" => " type='a'",                      // a, b, c
            "UPPER_ALPHA" => " type='A'",                // A, B, C
            "ROMAN" => " type='i'",                      // i, ii, iii
            "UPPER_ROMAN" => " type='I'",                // I, II, III
            var _ => ""
        };
    }

    private string ConvertParagraphToHtml(Paragraph paragraph)
    {
        var paragraphText = new StringBuilder();

        if (paragraph.Elements != null)
        {
            foreach (ParagraphElement? paragraphElement in paragraph.Elements)
            {
                if (paragraphElement.TextRun?.Content != null)
                {
                    string text = paragraphElement.TextRun.Content;
                    TextStyle? textStyle = paragraphElement.TextRun.TextStyle;

                    text = FormatTextElement(text, textStyle);
                    paragraphText.Append(text);
                }
            }
        }

        // Apply paragraph styling
        ParagraphStyle style = paragraph.ParagraphStyle;
        (string tag, string? id) = GetParagraphTagAndId(paragraph, style);

        string idAttr = !string.IsNullOrEmpty(id) ? $" id='{id}'" : "";
        return $"<{tag}{idAttr}>{paragraphText}</{tag}>";
    }

    private string FormatTextElement(string text, TextStyle? textStyle)
    {
        // Normalize text to handle Unicode characters properly
        text = NormalizeText(text);

        // Check for links
        if (textStyle?.Link != null)
        {
            string url = textStyle.Link.Url;
            string headingId = textStyle.Link.HeadingId;
            string bookmarkId = textStyle.Link.BookmarkId;

            (string? href, bool isExternal, bool isModal) = ProcessLinkUrl(url, headingId, bookmarkId, text);

            // Build attributes
            string targetAttr = isExternal ? " target='_blank' rel='noopener noreferrer'" : "";
            string modalAttr = isModal ? " data-modal='true'" : "";

            return $"<a href='{href}'{targetAttr}{modalAttr}>{text}</a>";
        }

        // Apply text formatting
        return ApplyTextFormatting(text, textStyle);
    }

    private static string ApplyTextFormatting(string text, TextStyle? textStyle)
    {
        if (textStyle?.Bold ?? false)
            text = $"<strong>{text}</strong>";
        if (textStyle?.Italic ?? false)
            text = $"<em>{text}</em>";
        if (textStyle?.Underline ?? false)
            text = $"<u>{text}</u>";
        return text;
    }

    private (string Tag, string? Id) GetParagraphTagAndId(Paragraph paragraph, ParagraphStyle style)
    {
        string tag = "p";
        string? id = null;

        if (style?.NamedStyleType != null)
        {
            tag = style.NamedStyleType switch
            {
                "HEADING_1" => "h1",
                "HEADING_2" => "h2",
                "HEADING_3" => "h3",
                "HEADING_4" => "h4",
                var _ => "p"
            };

            // Add ID to headings so they can be linked to
            if (!style.NamedStyleType.StartsWith(HeadingPrefix, StringComparison.Ordinal))
            {
                return (tag, id);
            }

            string headingText = GetPlainTextFromParagraph(paragraph).Trim();
            if (_headingIds.TryGetValue(headingText, out string? existingId))
            {
                id = existingId;
            }
        }

        return (tag, id);
    }

    private static string ConvertTableToHtml(Table table)
    {
        var tableHtml = new StringBuilder();
        tableHtml.AppendLine("<table border='1' style='border-collapse: collapse;'>");

        if (table.TableRows != null)
        {
            foreach (TableRow? row in table.TableRows)
            {
                tableHtml.AppendLine(TableRow);
                AppendTableCells(row, tableHtml);
                tableHtml.AppendLine(ClosingTableRow);
            }
        }

        tableHtml.AppendLine(ClosingTableTag);
        return tableHtml.ToString();
    }

    private static void AppendTableCells(TableRow row, StringBuilder tableHtml)
    {
        if (row.TableCells == null) return;

        foreach (TableCell? cell in row.TableCells)
        {
            tableHtml.Append(TableCell);
            AppendCellContent(cell, tableHtml);
            tableHtml.AppendLine(ClosingTableCell);
        }
    }

    private static void AppendCellContent(TableCell cell, StringBuilder tableHtml)
    {
        if (cell.Content == null) return;

        foreach (StructuralElement? cellElement in cell.Content)
        {
            if (cellElement.Paragraph != null)
            {
                string cellText = GetPlainTextFromParagraph(cellElement.Paragraph);
                tableHtml.Append(cellText);
            }
        }
    }

    private string ExtractTextFromParagraph(Paragraph paragraph, bool processLinks = false)
    {
        var text = new StringBuilder();

        if (paragraph.Elements != null)
        {
            foreach (ParagraphElement? element in paragraph.Elements)
            {
                if (element.TextRun?.Content != null)
                {
                    string content = element.TextRun.Content;
                    TextStyle? textStyle = element.TextRun.TextStyle;

                    content = processLinks && textStyle?.Link != null
                        ? FormatTextElement(content, textStyle)
                        : NormalizeAndFormatText(content, textStyle);

                    text.Append(content);
                }
            }
        }

        return text.ToString();
    }

    private static string NormalizeAndFormatText(string text, TextStyle? textStyle)
    {
        text = NormalizeText(text);
        return ApplyTextFormatting(text, textStyle);
    }
}
