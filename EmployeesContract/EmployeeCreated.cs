namespace EmployeesContract;

public sealed record EmployeeCreated(
  Guid Id,
  string SSN,
  string FirstName,
  string LastName,
  string Email,
  string PhoneNumber,
  string Address,
  string PostalCode,
  string City,
  decimal Salary,
  DateTimeOffset HireDate);
