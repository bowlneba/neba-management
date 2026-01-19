using System.Net.Mime;
using Neba.Infrastructure.Documents;

namespace Neba.UnitTests.Infrastructure.Documents;

[Trait("Category", "Unit")]
[Trait("Component", "Infrastructure.Documents")]
public sealed class GoogleDocsMetricsTests
{
    [Fact(DisplayName = "RecordExportSuccess with valid parameters completes successfully")]
    public void RecordExportSuccess_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "Bylaws";
        string documentId = "1abc123def456";
        double durationMs = 2500.0;
        long sizeBytes = 15000;
        string contentType = MediaTypeNames.Text.Html;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, contentType));
    }

    [Fact(DisplayName = "RecordExportSuccess with fast export completes successfully")]
    public void RecordExportSuccess_WithFastExport_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "SmallDoc";
        string documentId = "2xyz456abc789";
        double durationMs = 100.0;
        long sizeBytes = 2000;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, MediaTypeNames.Text.Html));
    }

    [Fact(DisplayName = "RecordExportSuccess with large export completes successfully")]
    public void RecordExportSuccess_WithLargeExport_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "LargeDoc";
        string documentId = "3def789xyz123";
        double durationMs = 5000.0;
        long sizeBytes = 500000;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, MediaTypeNames.Text.Html));
    }

    [Fact(DisplayName = "RecordExportSuccess with zero size completes successfully")]
    public void RecordExportSuccess_WithZeroSize_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "EmptyDoc";
        string documentId = "4ghi012jkl345";
        double durationMs = 50.0;
        long sizeBytes = 0;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, MediaTypeNames.Text.Html));
    }

    [Fact(DisplayName = "RecordExportSuccess with decimal duration completes successfully")]
    public void RecordExportSuccess_WithDecimalDuration_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "PrecisionDoc";
        string documentId = "5mno678pqr901";
        double durationMs = 1234.5678;
        long sizeBytes = 10000;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, MediaTypeNames.Text.Html));
    }

    [Fact(DisplayName = "RecordExportSuccess with multiple different exports completes successfully")]
    public void RecordExportSuccess_WithMultipleDifferentExports_CompletesSuccessfully()
    {
        // Act & Assert
        Should.NotThrow(() =>
        {
            GoogleDocsMetrics.RecordExportSuccess("Doc1", "id1", 1000.0, 5000, MediaTypeNames.Text.Html);
            GoogleDocsMetrics.RecordExportSuccess("Doc2", "id2", 2000.0, 10000, MediaTypeNames.Text.Html);
            GoogleDocsMetrics.RecordExportSuccess("Doc3", "id3", 1500.0, 7500, MediaTypeNames.Text.Html);
        });
    }

    [Fact(DisplayName = "RecordExportFailure with valid parameters completes successfully")]
    public void RecordExportFailure_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "FailedDoc";
        string documentId = "6stu345vwx678";
        string contentType = MediaTypeNames.Text.Html;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportFailure(
            documentName, documentId, contentType));
    }

    [Fact(DisplayName = "RecordExportFailure with different content types completes successfully")]
    public void RecordExportFailure_WithDifferentContentTypes_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "FailedDoc";
        string documentId = "7yza901bcd234";

        // Act & Assert
        Should.NotThrow(() =>
        {
            GoogleDocsMetrics.RecordExportFailure(documentName, documentId, MediaTypeNames.Text.Html);
            GoogleDocsMetrics.RecordExportFailure(documentName, documentId, "text/plain");
        });
    }

    [Fact(DisplayName = "RecordExportFailure with multiple failures completes successfully")]
    public void RecordExportFailure_WithMultipleFailures_CompletesSuccessfully()
    {
        // Act & Assert
        Should.NotThrow(() =>
        {
            GoogleDocsMetrics.RecordExportFailure("Doc1", "id1", MediaTypeNames.Text.Html);
            GoogleDocsMetrics.RecordExportFailure("Doc2", "id2", MediaTypeNames.Text.Html);
            GoogleDocsMetrics.RecordExportFailure("Doc3", "id3", MediaTypeNames.Text.Html);
        });
    }

    [Fact(DisplayName = "RecordExportSuccess followed by RecordExportFailure completes successfully")]
    public void RecordExportSuccessThenFailure_CompleteSuccessfully()
    {
        // Act & Assert
        Should.NotThrow(() =>
        {
            GoogleDocsMetrics.RecordExportSuccess("Doc1", "id1", 1000.0, 5000, MediaTypeNames.Text.Html);
            GoogleDocsMetrics.RecordExportFailure("Doc2", "id2", MediaTypeNames.Text.Html);
        });
    }

    [Fact(DisplayName = "RecordExportSuccess with long document name completes successfully")]
    public void RecordExportSuccess_WithLongDocumentName_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "VeryLongDocumentNameWithManyCharactersToTestStringHandling";
        string documentId = "8efg567hij890";
        double durationMs = 1500.0;
        long sizeBytes = 20000;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, MediaTypeNames.Text.Html));
    }

    [Fact(DisplayName = "RecordExportSuccess with special characters in document name completes successfully")]
    public void RecordExportSuccess_WithSpecialCharactersInName_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "Document-Name_2024 (v2).txt";
        string documentId = "9klm234nop567";
        double durationMs = 1000.0;
        long sizeBytes = 5000;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, MediaTypeNames.Text.Html));
    }

    [Fact(DisplayName = "RecordExportSuccess with very large duration completes successfully")]
    public void RecordExportSuccess_WithVeryLargeDuration_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "SlowDoc";
        string documentId = "10qrs901tuv234";
        double durationMs = 120000.0; // 2 minutes
        long sizeBytes = 50000;

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, MediaTypeNames.Text.Html));
    }

    [Fact(DisplayName = "RecordExportSuccess with very large file size completes successfully")]
    public void RecordExportSuccess_WithVeryLargeFileSize_CompletesSuccessfully()
    {
        // Arrange
        string documentName = "HugeDoc";
        string documentId = "11wxy567zabc890";
        double durationMs = 10000.0;
        long sizeBytes = 10000000; // 10MB

        // Act & Assert
        Should.NotThrow(() => GoogleDocsMetrics.RecordExportSuccess(
            documentName, documentId, durationMs, sizeBytes, MediaTypeNames.Text.Html));
    }
}
