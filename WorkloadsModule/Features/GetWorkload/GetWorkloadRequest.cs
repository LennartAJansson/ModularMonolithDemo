namespace WorkloadsModule.Features.GetWorkload;

using FastEndpoints;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;

public sealed class GetWorkloadRequest : ICommand<Result<Workload>>
{
    public Guid Id { get; set; }
}
