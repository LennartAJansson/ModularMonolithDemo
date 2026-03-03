namespace CustomersModule.Features.GetCustomer;

public sealed class GetCustomerResponse
{
    public required Guid Id { get; init; }
    public required string OrgNumber { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Address { get; init; }
    public required string PostalCode { get; init; }
    public required string City { get; init; }
}
