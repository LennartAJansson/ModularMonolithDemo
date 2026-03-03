namespace EmployeesModule.Features.GetEmployee;

using EmployeesModule.Entities;
using FastEndpoints;

public sealed class GetEmployeeMapper : Mapper<GetEmployeeRequest, GetEmployeeResponse, Employee>
{
    public override GetEmployeeResponse FromEntity(Employee entity) => new()
    {
        Id = entity.Id,
        SSN = entity.SSN,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        Email = entity.Email,
        PhoneNumber = entity.PhoneNumber,
        Address = entity.Address,
        PostalCode = entity.PostalCode,
        City = entity.City,
        Salary = entity.Salary,
        HireDate = entity.HireDate
    };
}
