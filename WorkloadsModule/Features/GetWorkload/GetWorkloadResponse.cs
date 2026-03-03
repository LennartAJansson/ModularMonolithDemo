namespace WorkloadsModule.Features.GetWorkload;

public sealed class GetWorkloadResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? StopDate { get; set; }
    public string? Comment { get; set; }
    public string? CustomerName { get; set; }
    public string? EmployeeName { get; set; }
}
