namespace WorkloadsModule.Features.UpdateWorkload;

using FastEndpoints;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;

public sealed class UpdateWorkloadRequest : ICommand<Result<Workload>>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? StopDate { get; set; }
    public string? Comment { get; set; }
}
