namespace CustomersContract;

public sealed record CustomerCreated(
  Guid Id,
  string OrgNumber,
  string Name,
  string Email,
  string PhoneNumber,
  string Address,
  string PostalCode,
  string City);
