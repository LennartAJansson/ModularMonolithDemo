namespace EmployeesModule.Features.ListEmployees;

using EmployeesModule.Entities;
using FastEndpoints;

public sealed class ListEmployeesMapper : ResponseMapper<ListEmployeesResponse, IEnumerable<Employee>>
{
    public override ListEmployeesResponse FromEntity(IEnumerable<Employee> entities) => new()
    {
        Employees = entities.Select(e => new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            PhoneNumber = e.PhoneNumber,
            Salary = e.Salary
        })
    };
}
