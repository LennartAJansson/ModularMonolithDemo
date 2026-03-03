namespace CustomersModule.Features.UpdateCustomer;

using CustomersContract;
using CustomersModule.Core;
using CustomersModule.Entities;
using CustomersModule.Infrastructure.Data.Context;
using Events.Abstract;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateCustomerHandler(CustomersDbContext db, IEvents events)
    : ICommandHandler<UpdateCustomerRequest, Result<Customer>>
{
    public async Task<Result<Customer>> ExecuteAsync(UpdateCustomerRequest command, CancellationToken ct)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == command.Id, ct);

        if (customer is null)
            return Result<Customer>.NotFound($"Customer with ID {command.Id} not found");

        // Check if OrgNumber or Email is taken by another customer
        var duplicate = await db.Customers
            .FirstOrDefaultAsync(c => c.Id != command.Id && 
                (c.OrgNumber == command.OrgNumber || c.Email == command.Email), ct);

        if (duplicate is not null)
        {
            if (duplicate.OrgNumber == command.OrgNumber)
                return Result<Customer>.Conflict($"OrgNumber {command.OrgNumber} is already used by another customer");
            
            return Result<Customer>.Conflict($"Email {command.Email} is already used by another customer");
        }

        customer.OrgNumber = command.OrgNumber;
        customer.Name = command.Name;
        customer.Email = command.Email;
        customer.PhoneNumber = command.PhoneNumber;
        customer.Address = command.Address;
        customer.PostalCode = command.PostalCode;
        customer.City = command.City;

        await db.SaveChangesAsync(ct);

        // Publish event
        await events.PublishAsync(new CustomerUpdated(
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
