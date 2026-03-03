namespace CustomersModule.Features.CreateCustomer;

using CustomersModule.Entities;
using FastEndpoints;

public sealed class CreateCustomerMapper : Mapper<CreateCustomerRequest, CreateCustomerResponse, Customer>
{
    public override CreateCustomerResponse FromEntity(Customer entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Email = entity.Email
    };
}
