namespace WorkloadsModule.Features.UpdateWorkload;

using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;
using WorkloadsModule.Infrastructure.Data.Context;

public sealed class UpdateWorkloadHandler(WorkloadsDbContext db) 
    : ICommandHandler<UpdateWorkloadRequest, Result<Workload>>
{
    public async Task<Result<Workload>> ExecuteAsync(UpdateWorkloadRequest command, CancellationToken ct)
    {
        var workload = await db.Workloads
            .FirstOrDefaultAsync(w => w.Id == command.Id && !w.IsDeleted, ct);

        if (workload is null)
            return Result<Workload>.NotFound($"Workload with ID {command.Id} not found");

        var customerExists = await db.WorkloadCustomers
            .AnyAsync(c => c.Id == command.CustomerId && !c.IsDeleted, ct);

        if (!customerExists)
            return Result<Workload>.Invalid($"Customer with ID {command.CustomerId} not found");

        var employeeExists = await db.WorkloadEmployees
            .AnyAsync(e => e.Id == command.EmployeeId && !e.IsDeleted, ct);

        if (!employeeExists)
            return Result<Workload>.Invalid($"Employee with ID {command.EmployeeId} not found");

        workload.CustomerId = command.CustomerId;
        workload.EmployeeId = command.EmployeeId;
        workload.StartDate = command.StartDate;
        workload.StopDate = command.StopDate;
        workload.Comment = command.Comment;

        await db.SaveChangesAsync(ct);

        return Result<Workload>.Success(workload);
    }
}
