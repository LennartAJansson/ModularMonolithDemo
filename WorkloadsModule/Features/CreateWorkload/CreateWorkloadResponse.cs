namespace WorkloadsModule.Features.CreateWorkload;

public sealed class CreateWorkloadResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? StopDate { get; set; }
}
