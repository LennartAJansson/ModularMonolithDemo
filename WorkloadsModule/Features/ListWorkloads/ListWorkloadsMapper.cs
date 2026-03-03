namespace WorkloadsModule.Features.ListWorkloads;

using FastEndpoints;
using WorkloadsModule.Entities;

public sealed class ListWorkloadsMapper : ResponseMapper<ListWorkloadsResponse, IEnumerable<Workload>>
{
    public override ListWorkloadsResponse FromEntity(IEnumerable<Workload> entities) => new()
    {
        Workloads = entities.Select(w => new WorkloadDto
        {
            Id = w.Id,
            StartDate = w.StartDate,
            StopDate = w.StopDate,
            Comment = w.Comment,
            CustomerName = w.Customer?.Name,
            EmployeeName = w.Employee != null ? $"{w.Employee.FirstName} {w.Employee.LastName}" : null
        })
    };
}
