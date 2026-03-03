namespace CustomersModule.Features.ListCustomers;

public sealed class ListCustomersResponse
{
    public required IEnumerable<CustomerDto> Customers { get; init; }
}

public sealed class CustomerDto
{
    public required Guid Id { get; init; }
    public required string OrgNumber { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string City { get; init; }
}
