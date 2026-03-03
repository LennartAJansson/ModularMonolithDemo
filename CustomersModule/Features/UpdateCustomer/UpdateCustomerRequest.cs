namespace CustomersModule.Features.UpdateCustomer;

using CustomersModule.Core;
using CustomersModule.Entities;
using FastEndpoints;

public sealed class UpdateCustomerRequest : ICommand<Result<Customer>>
{
    public Guid Id { get; set; }
    public required string OrgNumber { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required string PostalCode { get; set; }
    public required string City { get; set; }
}
