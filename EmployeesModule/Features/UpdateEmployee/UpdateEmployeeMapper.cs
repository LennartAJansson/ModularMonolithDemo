namespace EmployeesModule.Features.UpdateEmployee;

using EmployeesModule.Entities;
using FastEndpoints;

public sealed class UpdateEmployeeMapper : Mapper<UpdateEmployeeRequest, UpdateEmployeeResponse, Employee>
{
    public override UpdateEmployeeResponse FromEntity(Employee entity) => new()
    {
        Id = entity.Id,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Email = entity.Email
    };
}
