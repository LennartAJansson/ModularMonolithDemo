namespace EmployeesModule.Features.GetEmployee;

public sealed class GetEmployeeResponse
{
    public required Guid Id { get; init; }
    public required string SSN { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Address { get; init; }
    public required string PostalCode { get; init; }
    public required string City { get; init; }
    public decimal Salary { get; init; }
    public DateTimeOffset HireDate { get; init; }
}
