namespace CustomersModule.Features.ListCustomers;

using CustomersModule.Entities;
using FastEndpoints;

public sealed class ListCustomersMapper : ResponseMapper<ListCustomersResponse, IEnumerable<Customer>>
{
    public override ListCustomersResponse FromEntity(IEnumerable<Customer> entities) => new()
    {
        Customers = entities.Select(c => new CustomerDto
        {
            Id = c.Id,
            OrgNumber = c.OrgNumber,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            City = c.City
        })
    };
}
