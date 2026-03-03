namespace CustomersModule.Features.UpdateCustomer;

public sealed class UpdateCustomerResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
}
