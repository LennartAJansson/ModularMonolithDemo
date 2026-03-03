namespace CustomersModule.Features.GetCustomer;

using CustomersModule.Entities;
using FastEndpoints;

public sealed class GetCustomerMapper : Mapper<GetCustomerRequest, GetCustomerResponse, Customer>
{
    public override GetCustomerResponse FromEntity(Customer entity) => new()
    {
        Id = entity.Id,
        OrgNumber = entity.OrgNumber,
        Name = entity.Name,
        Email = entity.Email,
        PhoneNumber = entity.PhoneNumber,
        Address = entity.Address,
        PostalCode = entity.PostalCode,
        City = entity.City
    };
}
