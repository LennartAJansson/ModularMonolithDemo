namespace WorkloadsModule.Features.DeleteWorkload;

using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;
using WorkloadsModule.Infrastructure.Data.Context;

public sealed class DeleteWorkloadHandler(WorkloadsDbContext db) 
    : ICommandHandler<DeleteWorkloadRequest, Result<Workload>>
{
    public async Task<Result<Workload>> ExecuteAsync(DeleteWorkloadRequest command, CancellationToken ct)
    {
        var workload = await db.Workloads
            .FirstOrDefaultAsync(w => w.Id == command.Id && !w.IsDeleted, ct);

        if (workload is null)
            return Result<Workload>.NotFound($"Workload with ID {command.Id} not found");

        workload.IsDeleted = true;
        await db.SaveChangesAsync(ct);

        return Result<Workload>.Success(workload);
    }
}
