namespace CustomersModule.Features.GetCustomer;

using CustomersModule.Core;
using CustomersModule.Entities;
using FastEndpoints;

public sealed class GetCustomerRequest : ICommand<Result<Customer>>
{
    public Guid Id { get; set; }
}
