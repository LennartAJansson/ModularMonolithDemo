namespace CustomersModule.Features.GetCustomer;

using CustomersModule.Core;
using CustomersModule.Entities;
using CustomersModule.Infrastructure.Data.Context;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class GetCustomerHandler(CustomersDbContext db)
    : ICommandHandler<GetCustomerRequest, Result<Customer>>
{
    public async Task<Result<Customer>> ExecuteAsync(GetCustomerRequest command, CancellationToken ct)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == command.Id, ct);

        return customer is null 
            ? Result<Customer>.NotFound($"Customer with ID {command.Id} not found") 
            : Result<Customer>.Success(customer);
    }
}
