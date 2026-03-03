namespace WorkloadsModule.Features.UpdateWorkload;

public sealed class UpdateWorkloadResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? StopDate { get; set; }
}
