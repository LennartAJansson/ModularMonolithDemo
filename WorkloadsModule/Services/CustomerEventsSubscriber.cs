namespace WorkloadsModule.Services;

using CustomersContract;
using Events.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkloadsModule.Entities;
using WorkloadsModule.Infrastructure.Data.Context;

public sealed class CustomerEventsSubscriber(
    IEvents events,
    IServiceScopeFactory scopeFactory,
    ILogger<CustomerEventsSubscriber> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CustomerEventsSubscriber started");

        await foreach (var evt in events.SubscribeAsync<CustomerCreated>(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WorkloadsDbContext>();

            try
            {
                var customer = new WorkloadCustomer
                {
                    Id = evt.Id,
                    Name = evt.Name,
                    Email = evt.Email,
                    PhoneNumber = evt.PhoneNumber
                };

                db.WorkloadCustomers.Add(customer);
                await db.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Persisted CustomerCreated event for {CustomerId}", evt.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing CustomerCreated event for {CustomerId}", evt.Id);
            }
        }
    }
}
