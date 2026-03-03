namespace WorkloadsModule.Features.CreateWorkload;

using FastEndpoints;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;

public sealed class CreateWorkloadRequest : ICommand<Result<Workload>>
{
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? StopDate { get; set; }
    public string? Comment { get; set; }
    public Guid CustomerId { get; set; }
    public Guid EmployeeId { get; set; }
}
