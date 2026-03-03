namespace WorkloadsModule.Entities;

public sealed class Workload
{
  public Guid Id { get; set; }
  public DateTimeOffset StartDate { get; set; }
  public DateTimeOffset? StopDate { get; set; }
  public string? Comment { get; set; }

  // Foreign keys
  public Guid CustomerId { get; set; }
  public Guid EmployeeId { get; set; }

  // Navigation properties
  public WorkloadCustomer? Customer { get; set; }
  public WorkloadEmployee? Employee { get; set; }

  // Audit fields
  public DateTimeOffset? CreatedAt { get; set; }
  public DateTimeOffset? UpdatedAt { get; set; }
  public bool IsDeleted { get; set; }
}
