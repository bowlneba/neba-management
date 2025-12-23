using ErrorOr;
using Neba.Application.Messaging;

namespace Neba.Website.Application.Documents.Bylaws;

internal sealed class RefreshBylawsCacheCommandHandler(BylawsSyncBackgroundJob bylawsSyncBackgroundJob)
        : ICommandHandler<RefreshBylawsCacheCommand, string>
{
    private readonly BylawsSyncBackgroundJob _bylawsSyncBackgroundJob = bylawsSyncBackgroundJob;

    public Task<ErrorOr<string>> HandleAsync(
        RefreshBylawsCacheCommand command,
        CancellationToken cancellationToken)
    {
        string jobId = _bylawsSyncBackgroundJob.TriggerImmediateSync();

        return Task.FromResult<ErrorOr<string>>(jobId);
    }
}
