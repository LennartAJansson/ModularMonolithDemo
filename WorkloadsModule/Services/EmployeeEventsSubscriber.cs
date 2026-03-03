namespace WorkloadsModule.Services;

using EmployeesContract;
using Events.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkloadsModule.Entities;
using WorkloadsModule.Infrastructure.Data.Context;

public sealed class EmployeeEventsSubscriber(
    IEvents events,
    IServiceScopeFactory scopeFactory,
    ILogger<EmployeeEventsSubscriber> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EmployeeEventsSubscriber started");

        await foreach (var evt in events.SubscribeAsync<EmployeeCreated>(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WorkloadsDbContext>();

            try
            {
                var employee = new WorkloadEmployee
                {
                    Id = evt.Id,
                    FirstName = evt.FirstName,
                    LastName = evt.LastName,
                    Email = evt.Email,
                    PhoneNumber = evt.PhoneNumber
                };

                db.WorkloadEmployees.Add(employee);
                await db.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Persisted EmployeeCreated event for {EmployeeId}", evt.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing EmployeeCreated event for {EmployeeId}", evt.Id);
            }
        }
    }
}
