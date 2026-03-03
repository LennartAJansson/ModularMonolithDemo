namespace CustomersModule.Entities;

public sealed class Customer
{
  public Guid Id { get; set; }
  public required string OrgNumber { get; set; }
  public required string Name { get; set; }
  public required string Email { get; set; }
  public required string PhoneNumber { get; set; }
  public required string Address { get; set; }
  public required string PostalCode { get; set; }
  public required string City { get; set; }
  public DateTimeOffset? CreatedAt { get; set; }
  public DateTimeOffset? UpdatedAt { get; set; }
  public bool IsDeleted { get; set; }
}