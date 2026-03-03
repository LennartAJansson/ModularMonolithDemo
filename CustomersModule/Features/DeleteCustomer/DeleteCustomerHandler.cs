namespace CustomersModule.Features.DeleteCustomer;

using CustomersContract;
using CustomersModule.Core;
using CustomersModule.Infrastructure.Data.Context;
using Events.Abstract;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteCustomerHandler(CustomersDbContext db, IEvents events)
    : ICommandHandler<DeleteCustomerRequest, Result>
{
    public async Task<Result> ExecuteAsync(DeleteCustomerRequest command, CancellationToken ct)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == command.Id, ct);

        if (customer is null)
            return Result.NotFound($"Customer with ID {command.Id} not found");

        // Soft delete handled by interceptor
        db.Customers.Remove(customer);

        await db.SaveChangesAsync(ct);

        // Publish event
        await events.PublishAsync(new CustomerDeleted(customer.Id), ct);

        return Result.Success();
    }
}
