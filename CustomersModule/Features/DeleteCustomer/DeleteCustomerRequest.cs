namespace CustomersModule.Features.DeleteCustomer;

using CustomersModule.Core;
using FastEndpoints;

public sealed class DeleteCustomerRequest : ICommand<Result>
{
    public Guid Id { get; set; }
}
