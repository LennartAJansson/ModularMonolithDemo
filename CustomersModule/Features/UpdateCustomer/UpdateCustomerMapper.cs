namespace CustomersModule.Features.UpdateCustomer;

using CustomersModule.Entities;
using FastEndpoints;

public sealed class UpdateCustomerMapper : Mapper<UpdateCustomerRequest, UpdateCustomerResponse, Customer>
{
    public override UpdateCustomerResponse FromEntity(Customer entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Email = entity.Email
    };
}
