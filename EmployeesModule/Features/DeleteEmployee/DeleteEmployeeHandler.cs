namespace EmployeesModule.Features.DeleteEmployee;

using EmployeesContract;
using EmployeesModule.Core;
using EmployeesModule.Infrastructure.Data.Context;
using Events.Abstract;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteEmployeeHandler(EmployeesDbContext db, IEvents events)
    : ICommandHandler<DeleteEmployeeRequest, Result>
{
    public async Task<Result> ExecuteAsync(DeleteEmployeeRequest command, CancellationToken ct)
    {
        var employee = await db.Employees
            .FirstOrDefaultAsync(e => e.Id == command.Id, ct);

        if (employee is null)
            return Result.NotFound($"Employee with ID {command.Id} not found");

        // Soft delete handled by interceptor
        db.Employees.Remove(employee);

        await db.SaveChangesAsync(ct);

        // Publish event
        await events.PublishAsync(new EmployeeDeleted(employee.Id), ct);

        return Result.Success();
    }
}
