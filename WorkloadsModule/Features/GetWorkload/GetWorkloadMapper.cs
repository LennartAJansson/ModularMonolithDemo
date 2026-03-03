namespace WorkloadsModule.Features.GetWorkload;

using FastEndpoints;
using WorkloadsModule.Entities;

public sealed class GetWorkloadMapper : Mapper<GetWorkloadRequest, GetWorkloadResponse, Workload>
{
    public override GetWorkloadResponse FromEntity(Workload entity) => new()
    {
        Id = entity.Id,
        StartDate = entity.StartDate,
        StopDate = entity.StopDate,
        Comment = entity.Comment,
        CustomerName = entity.Customer?.Name,
        EmployeeName = entity.Employee != null ? $"{entity.Employee.FirstName} {entity.Employee.LastName}" : null
    };
}
