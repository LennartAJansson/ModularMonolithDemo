namespace EmployeesModule.Features.UpdateEmployee;

using EmployeesContract;
using EmployeesModule.Core;
using EmployeesModule.Entities;
using EmployeesModule.Infrastructure.Data.Context;
using Events.Abstract;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateEmployeeHandler(EmployeesDbContext db, IEvents events)
    : ICommandHandler<UpdateEmployeeRequest, Result<Employee>>
{
    public async Task<Result<Employee>> ExecuteAsync(UpdateEmployeeRequest command, CancellationToken ct)
    {
        var employee = await db.Employees
            .FirstOrDefaultAsync(e => e.Id == command.Id, ct);

        if (employee is null)
            return Result<Employee>.NotFound($"Employee with ID {command.Id} not found");

        // Check if SSN or Email is taken by another employee
        var duplicate = await db.Employees
            .FirstOrDefaultAsync(e => e.Id != command.Id && 
                (e.SSN == command.SSN || e.Email == command.Email), ct);

        if (duplicate is not null)
        {
            if (duplicate.SSN == command.SSN)
                return Result<Employee>.Conflict($"SSN {command.SSN} is already used by another employee");
            
            return Result<Employee>.Conflict($"Email {command.Email} is already used by another employee");
        }

        employee.SSN = command.SSN;
        employee.FirstName = command.FirstName;
        employee.LastName = command.LastName;
        employee.Email = command.Email;
        employee.PhoneNumber = command.PhoneNumber;
        employee.Address = command.Address;
        employee.PostalCode = command.PostalCode;
        employee.City = command.City;
        employee.Salary = command.Salary;
        employee.HireDate = command.HireDate;

        await db.SaveChangesAsync(ct);

        // Publish event
        await events.PublishAsync(new EmployeeUpdated(
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
