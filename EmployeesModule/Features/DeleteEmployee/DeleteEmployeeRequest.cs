namespace EmployeesModule.Features.DeleteEmployee;

using EmployeesModule.Core;
using FastEndpoints;

public sealed class DeleteEmployeeRequest : ICommand<Result>
{
    public Guid Id { get; set; }
}
