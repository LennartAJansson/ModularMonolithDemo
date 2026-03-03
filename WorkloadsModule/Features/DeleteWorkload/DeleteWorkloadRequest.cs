namespace WorkloadsModule.Features.DeleteWorkload;

using FastEndpoints;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;

public sealed class DeleteWorkloadRequest : ICommand<Result<Workload>>
{
    public Guid Id { get; set; }
}
