namespace WorkloadsModule.Features.CreateWorkload;

using FastEndpoints;
using WorkloadsModule.Entities;

public sealed class CreateWorkloadMapper : Mapper<CreateWorkloadRequest, CreateWorkloadResponse, Workload>
{
    public override CreateWorkloadResponse FromEntity(Workload entity) => new()
    {
        Id = entity.Id,
        StartDate = entity.StartDate,
        StopDate = entity.StopDate
    };
}
