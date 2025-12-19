using Google.Apis.Docs.v1.Data;

namespace Neba.Tests.Website;

public static class GoogleDocsDocumentFactory
{
    public static Document Create(params StructuralElement[] content)
        => new()
        {
            Body = new Body
            {
                Content = content.ToList()
            }
        };

    public static Document CreateWithLists(
        IReadOnlyDictionary<string, Google.Apis.Docs.v1.Data.List> lists,
        params StructuralElement[] content)
        => new()
        {
            Lists = new Dictionary<string, Google.Apis.Docs.v1.Data.List>(lists),
            Body = new Body
            {
                Content = content.ToList()
            }
        };

    public static StructuralElement Paragraph(string text, ParagraphStyle? paragraphStyle = null)
        => new()
        {
            Paragraph = new Paragraph
            {
                ParagraphStyle = paragraphStyle,
                Elements =
                [
                    new ParagraphElement
                    {
                        TextRun = new TextRun
                        {
                            Content = text,
                            TextStyle = new TextStyle()
                        }
                    }
                ]
            }
        };

    public static StructuralElement Paragraph(params (string Content, TextStyle? Style)[] runs)
        => new()
        {
            Paragraph = new Paragraph
            {
                ParagraphStyle = new ParagraphStyle { NamedStyleType = "NORMAL_TEXT" },
                Elements = runs
                    .Select(r => new ParagraphElement
                    {
                        TextRun = new TextRun
                        {
                            Content = r.Content,
                            TextStyle = r.Style ?? new TextStyle()
                        }
                    })
                    .ToList()
            }
        };

    public static StructuralElement Heading(string headingText, string namedStyleType)
        => new()
        {
            Paragraph = new Paragraph
            {
                ParagraphStyle = new ParagraphStyle { NamedStyleType = namedStyleType },
                Elements =
                [
                    new ParagraphElement
                    {
                        TextRun = new TextRun
                        {
                            Content = headingText,
                            TextStyle = new TextStyle()
                        }
                    }
                ]
            }
        };

    public static StructuralElement BulletParagraph(
        string listId,
        int nestingLevel,
        params (string Content, TextStyle? Style)[] runs)
        => new()
        {
            Paragraph = new Paragraph
            {
                Bullet = new Bullet { ListId = listId, NestingLevel = nestingLevel },
                ParagraphStyle = new ParagraphStyle { NamedStyleType = "NORMAL_TEXT" },
                Elements = runs
                    .Select(r => new ParagraphElement
                    {
                        TextRun = new TextRun
                        {
                            Content = r.Content,
                            TextStyle = r.Style ?? new TextStyle()
                        }
                    })
                    .ToList()
            }
        };

    public static StructuralElement BulletParagraph(string listId, int nestingLevel, string text)
        => BulletParagraph(listId, nestingLevel, (text, null));

    public static StructuralElement Table(params string[][] rows)
        => new()
        {
            Table = new Google.Apis.Docs.v1.Data.Table
            {
                TableRows = rows
                    .Select(r => new TableRow
                    {
                        TableCells = r
                            .Select(cellText => new TableCell
                            {
                                Content =
                                [
                                    new StructuralElement
                                    {
                                        Paragraph = new Paragraph
                                        {
                                            Elements =
                                            [
                                                new ParagraphElement
                                                {
                                                    TextRun = new TextRun
                                                    {
                                                        Content = cellText,
                                                        TextStyle = new TextStyle()
                                                    }
                                                }
                                            ]
                                        }
                                    }
                                ]
                            })
                            .ToList()
                    })
                    .ToList()
            }
        };

    public static IReadOnlyDictionary<string, Google.Apis.Docs.v1.Data.List> Lists(params (string ListId, int NestingLevel, string GlyphType, int? StartNumber)[] levels)
    {
        var grouped = levels
            .GroupBy(x => x.ListId, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => new Google.Apis.Docs.v1.Data.List
                {
                    ListProperties = new ListProperties
                    {
                        NestingLevels = g
                            .OrderBy(x => x.NestingLevel)
                            .Select(x => new NestingLevel
                            {
                                GlyphType = x.GlyphType,
                                StartNumber = x.StartNumber
                            })
                            .ToList()
                    }
                },
                StringComparer.Ordinal);

        return grouped;
    }

    public static TextStyle Bold() => new() { Bold = true };
    public static TextStyle Italic() => new() { Italic = true };
    public static TextStyle Underline() => new() { Underline = true };

    public static TextStyle LinkToExternal(Uri uri)
        => new()
        {
            Link = new Link { Url = uri.ToString() }
        };

    public static TextStyle LinkToBookmark(string bookmarkId)
        => new()
        {
            Link = new Link { BookmarkId = bookmarkId }
        };

    public static TextStyle LinkToHeading(string headingId)
        => new()
        {
            Link = new Link { HeadingId = headingId }
        };
}
