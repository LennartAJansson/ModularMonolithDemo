namespace EmployeesModule.Features.GetEmployee;

using EmployeesModule.Core;
using EmployeesModule.Entities;
using EmployeesModule.Infrastructure.Data.Context;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class GetEmployeeHandler(EmployeesDbContext db)
    : ICommandHandler<GetEmployeeRequest, Result<Employee>>
{
    public async Task<Result<Employee>> ExecuteAsync(GetEmployeeRequest command, CancellationToken ct)
    {
        var employee = await db.Employees
            .FirstOrDefaultAsync(e => e.Id == command.Id, ct);

        return employee is null 
            ? Result<Employee>.NotFound($"Employee with ID {command.Id} not found") 
            : Result<Employee>.Success(employee);
    }
}
