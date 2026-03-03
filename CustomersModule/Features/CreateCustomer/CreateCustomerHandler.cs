namespace CustomersModule.Features.CreateCustomer;

using CustomersContract;
using CustomersModule.Core;
using CustomersModule.Entities;
using CustomersModule.Infrastructure.Data.Context;
using Events.Abstract;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class CreateCustomerHandler(CustomersDbContext db, IEvents events) 
    : ICommandHandler<CreateCustomerRequest, Result<Customer>>
{
    public async Task<Result<Customer>> ExecuteAsync(CreateCustomerRequest command, CancellationToken ct)
    {
        // Check if customer with same OrgNumber already exists
        var existingCustomer = await db.Customers
            .FirstOrDefaultAsync(c => c.OrgNumber == command.OrgNumber, ct);

        if (existingCustomer is not null)
        {
            return Result<Customer>.Conflict($"Customer with OrgNumber {command.OrgNumber} already exists");
        }

        var customer = new Customer
        {
            Id = Guid.CreateVersion7(),
            OrgNumber = command.OrgNumber,
            Name = command.Name,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            Address = command.Address,
            PostalCode = command.PostalCode,
            City = command.City
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);

        // Publish event
        await events.PublishAsync(new CustomerCreated(
            Id: customer.Id,
            OrgNumber: customer.OrgNumber,
            Name: customer.Name,
            Email: customer.Email,
            PhoneNumber: customer.PhoneNumber,
            Address: customer.Address,
            PostalCode: customer.PostalCode,
            City: customer.City
        ), ct);

        return Result<Customer>.Success(customer);
    }
}
