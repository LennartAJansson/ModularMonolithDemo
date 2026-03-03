namespace EmployeesModule.Features.CreateEmployee;

using EmployeesModule.Core;
using EmployeesModule.Entities;
using FastEndpoints;

public sealed class CreateEmployeeRequest : ICommand<Result<Employee>>
{
    public required string SSN { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required string PostalCode { get; set; }
    public required string City { get; set; }
    public decimal Salary { get; set; }
    public DateTimeOffset HireDate { get; set; }
}
