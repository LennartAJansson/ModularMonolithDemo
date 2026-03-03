namespace CustomersModule.Features.CreateCustomer;

public sealed class CreateCustomerResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}
