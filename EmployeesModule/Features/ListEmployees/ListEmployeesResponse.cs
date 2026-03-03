namespace EmployeesModule.Features.ListEmployees;

public sealed class ListEmployeesResponse
{
    public required IEnumerable<EmployeeDto> Employees { get; init; }
}

public sealed class EmployeeDto
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public decimal Salary { get; init; }
}
