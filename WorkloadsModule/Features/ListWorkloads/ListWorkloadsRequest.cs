namespace WorkloadsModule.Features.ListWorkloads;

using FastEndpoints;
using WorkloadsModule.Core;
using WorkloadsModule.Entities;

public sealed class ListWorkloadsRequest : ICommand<Result<IEnumerable<Workload>>>
{
    // Empty request - no query parameters needed
    // Dummy property required for Swagger/OpenAPI documentation
    public bool? _ { get; init; }
}
