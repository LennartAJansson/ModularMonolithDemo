namespace EmployeesModule.Features.CreateEmployee;

using EmployeesContract;
using EmployeesModule.Core;
using EmployeesModule.Entities;
using EmployeesModule.Infrastructure.Data.Context;
using Events.Abstract;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class CreateEmployeeHandler(EmployeesDbContext db, IEvents events)
    : ICommandHandler<CreateEmployeeRequest, Result<Employee>>
{
    public async Task<Result<Employee>> ExecuteAsync(CreateEmployeeRequest command, CancellationToken ct)
    {
        // Check if employee with same SSN or Email already exists
        var existingEmployee = await db.Employees
            .FirstOrDefaultAsync(e => e.SSN == command.SSN || e.Email == command.Email, ct);

        if (existingEmployee is not null)
        {
            if (existingEmployee.SSN == command.SSN)
                return Result<Employee>.Conflict($"Employee with SSN {command.SSN} already exists");
            
            return Result<Employee>.Conflict($"Employee with Email {command.Email} already exists");
        }

        var employee = new Employee
        {
            Id = Guid.CreateVersion7(),
            SSN = command.SSN,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            Address = command.Address,
            PostalCode = command.PostalCode,
            City = command.City,
            Salary = command.Salary,
            HireDate = command.HireDate
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);

        // Publish event
        await events.PublishAsync(new EmployeeCreated(
            Id: employee.Id,
            SSN: employee.SSN,
            FirstName: employee.FirstName,
            LastName: employee.LastName,
            Email: employee.Email,
            PhoneNumber: employee.PhoneNumber,
            Address: employee.Address,
            PostalCode: employee.PostalCode,
            City: employee.City,
            Salary: employee.Salary,
            HireDate: employee.HireDate
        ), ct);

        return Result<Employee>.Success(employee);
    }
}
