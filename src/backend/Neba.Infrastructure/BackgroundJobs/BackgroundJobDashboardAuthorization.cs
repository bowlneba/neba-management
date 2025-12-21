using Hangfire.Dashboard;

namespace Neba.Infrastructure.BackgroundJobs;

internal sealed class BackgroundJobDashboardAuthorizationFilter
    : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
