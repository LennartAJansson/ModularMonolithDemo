namespace WorkloadsModule.Features.GetWorkload;

using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;
using WorkloadsModule.Infrastructure.Data.Context;

public sealed class GetWorkloadHandler(WorkloadsDbContext db)
    : ICommandHandler<GetWorkloadRequest, Result<Workload>>
{
    public async Task<Result<Workload>> ExecuteAsync(GetWorkloadRequest command, CancellationToken ct)
    {
        var workload = await db.Workloads
            .Include(w => w.Customer)
            .Include(w => w.Employee)
            .FirstOrDefaultAsync(w => w.Id == command.Id, ct);

        return workload is null 
            ? Result<Workload>.NotFound($"Workload with ID {command.Id} not found") 
            : Result<Workload>.Success(workload);
    }
}
