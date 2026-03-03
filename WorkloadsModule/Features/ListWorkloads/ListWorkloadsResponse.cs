namespace WorkloadsModule.Features.ListWorkloads;

public sealed class ListWorkloadsResponse
{
    public required IEnumerable<WorkloadDto> Workloads { get; init; }
}

public sealed class WorkloadDto
{
    public Guid Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? StopDate { get; set; }
    public string? Comment { get; set; }
    public string? CustomerName { get; set; }
    public string? EmployeeName { get; set; }
}
