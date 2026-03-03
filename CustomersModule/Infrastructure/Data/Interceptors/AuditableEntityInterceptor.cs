namespace CustomersModule.Infrastructure.Data.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Handle soft delete
            if (entry.State == EntityState.Deleted)
            {
                var isDeletedProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
                var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");

                if (isDeletedProperty is not null && updatedAtProperty is not null)
                {
                    entry.State = EntityState.Modified;
                    isDeletedProperty.CurrentValue = true;
                    updatedAtProperty.CurrentValue = utcNow;
                }
            }
            // Handle CreatedAt on insert
            else if (entry.State == EntityState.Added)
            {
                var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAtProperty is not null)
                {
                    createdAtProperty.CurrentValue = utcNow;
                }
            }
            // Handle UpdatedAt on update
            else if (entry.State == EntityState.Modified)
            {
                var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedAtProperty is not null)
                {
                    updatedAtProperty.CurrentValue = utcNow;
                }
            }
        }
    }
}
