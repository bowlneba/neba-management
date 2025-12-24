using ErrorOr;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Documents.Bylaws;

internal sealed class RefreshBylawsCacheCommandHandler(IBylawsSyncBackgroundJob bylawsSyncBackgroundJob)
        : ICommandHandler<RefreshBylawsCacheCommand, string>
{

    public Task<ErrorOr<string>> HandleAsync(
        RefreshBylawsCacheCommand command,
        CancellationToken cancellationToken)
    {
        string jobId = bylawsSyncBackgroundJob.TriggerImmediateSync();

        return Task.FromResult<ErrorOr<string>>(jobId);
    }
}
