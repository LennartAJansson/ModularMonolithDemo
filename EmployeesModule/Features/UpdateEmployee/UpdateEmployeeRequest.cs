namespace EmployeesModule.Features.UpdateEmployee;

using EmployeesModule.Core;
using EmployeesModule.Entities;
using FastEndpoints;

public sealed class UpdateEmployeeRequest : ICommand<Result<Employee>>
{
    public Guid Id { get; set; }
    public required string SSN { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required string PostalCode { get; set; }
    public required string City { get; set; }
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
}
