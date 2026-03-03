namespace EmployeesModule.Features.CreateEmployee;

using EmployeesModule.Entities;
using FastEndpoints;

public sealed class CreateEmployeeMapper : Mapper<CreateEmployeeRequest, CreateEmployeeResponse, Employee>
{
    public override CreateEmployeeResponse FromEntity(Employee entity) => new()
    {
        Id = entity.Id,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Email = entity.Email
    };
}
