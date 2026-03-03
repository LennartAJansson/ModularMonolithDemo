namespace EmployeesModule.Features.ListEmployees;

using EmployeesModule.Core;
using EmployeesModule.Entities;
using EmployeesModule.Infrastructure.Data.Context;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class ListEmployeesHandler(EmployeesDbContext db)
    : ICommandHandler<ListEmployeesRequest, Result<IEnumerable<Employee>>>
{
    public async Task<Result<IEnumerable<Employee>>> ExecuteAsync(ListEmployeesRequest command, CancellationToken ct)
    {
        var employees = await db.Employees
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(ct);

        return Result<IEnumerable<Employee>>.Success(employees);
    }
}
