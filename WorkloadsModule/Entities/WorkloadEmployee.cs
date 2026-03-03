namespace WorkloadsModule.Entities;

public sealed class WorkloadEmployee
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }

    // Navigation property
    public ICollection<Workload> Workloads { get; set; } = new HashSet<Workload>();

    // Audit fields
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}