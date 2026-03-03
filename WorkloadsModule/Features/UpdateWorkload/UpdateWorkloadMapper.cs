namespace WorkloadsModule.Features.UpdateWorkload;

using FastEndpoints;
using WorkloadsModule.Entities;

public sealed class UpdateWorkloadMapper : Mapper<UpdateWorkloadRequest, UpdateWorkloadResponse, Workload>
{
    public override UpdateWorkloadResponse FromEntity(Workload entity) => new()
    {
        Id = entity.Id,
        StartDate = entity.StartDate,
        StopDate = entity.StopDate
    };
}
