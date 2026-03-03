namespace WorkloadsModule.Features.ListWorkloads;

using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;
using WorkloadsModule.Infrastructure.Data.Context;

public sealed class ListWorkloadsHandler(WorkloadsDbContext db)
    : ICommandHandler<ListWorkloadsRequest, Result<IEnumerable<Workload>>>
{
    public async Task<Result<IEnumerable<Workload>>> ExecuteAsync(ListWorkloadsRequest command, CancellationToken ct)
    {
        var workloads = await db.Workloads
            .Include(w => w.Customer)
            .Include(w => w.Employee)
            .OrderByDescending(w => w.StartDate)
            .ToListAsync(ct);

        return Result<IEnumerable<Workload>>.Success(workloads);
    }
}
