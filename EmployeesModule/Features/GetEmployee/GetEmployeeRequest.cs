namespace EmployeesModule.Features.GetEmployee;

using EmployeesModule.Core;
using EmployeesModule.Entities;
using FastEndpoints;

public sealed class GetEmployeeRequest : ICommand<Result<Employee>>
{
    public Guid Id { get; set; }
}
