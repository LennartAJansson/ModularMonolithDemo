namespace CustomersModule.Features.ListCustomers;

using CustomersModule.Core;
using CustomersModule.Entities;
using CustomersModule.Infrastructure.Data.Context;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class ListCustomersHandler(CustomersDbContext db)
    : ICommandHandler<ListCustomersRequest, Result<IEnumerable<Customer>>>
{
    public async Task<Result<IEnumerable<Customer>>> ExecuteAsync(ListCustomersRequest command, CancellationToken ct)
    {
        var customers = await db.Customers
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return Result<IEnumerable<Customer>>.Success(customers);
    }
}
