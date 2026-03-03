namespace WorkloadsModule.Features.CreateWorkload;

using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;
using WorkloadsModule.Infrastructure.Data.Context;

public sealed class CreateWorkloadHandler(WorkloadsDbContext db) 
    : ICommandHandler<CreateWorkloadRequest, Result<Workload>>
{
    public async Task<Result<Workload>> ExecuteAsync(CreateWorkloadRequest command, CancellationToken ct)
    {
        // Validate customer exists
        var customerExists = await db.WorkloadCustomers
            .AnyAsync(c => c.Id == command.CustomerId, ct);

        if (!customerExists)
            return Result<Workload>.NotFound($"Customer with ID {command.CustomerId} not found");

        // Validate employee exists
        var employeeExists = await db.WorkloadEmployees
            .AnyAsync(e => e.Id == command.EmployeeId, ct);

        if (!employeeExists)
            return Result<Workload>.NotFound($"Employee with ID {command.EmployeeId} not found");

        var workload = new Workload
        {
            Id = Guid.CreateVersion7(),
            StartDate = command.StartDate,
            StopDate = command.StopDate,
            Comment = command.Comment,
            CustomerId = command.CustomerId,
            EmployeeId = command.EmployeeId
        };

        db.Workloads.Add(workload);
        await db.SaveChangesAsync(ct);

        // Load navigation properties for response
        await db.Entry(workload)
            .Reference(w => w.Customer)
            .LoadAsync(ct);

        await db.Entry(workload)
            .Reference(w => w.Employee)
            .LoadAsync(ct);

        return Result<Workload>.Success(workload);
    }
}
