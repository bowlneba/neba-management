using Neba.Application.BackgroundJobs;

namespace Neba.Application.Documents;

internal sealed record SyncHtmlDocumentToStorageJob
    : IBackgroundJob
{
    public required string DocumentKey { get; init; }

    public required string ContainerName { get; init; }

    public required string DocumentName { get; init; }
}
