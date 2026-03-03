namespace CustomersModule.Features.ListCustomers;

using CustomersModule.Core;
using CustomersModule.Entities;
using FastEndpoints;

public sealed class ListCustomersRequest : ICommand<Result<IEnumerable<Customer>>>
{
    // Empty request - no query parameters needed
    // Dummy property required for Swagger/OpenAPI documentation
    public bool? _ { get; init; }
}
