namespace EmployeesModule.Features.ListEmployees;

using EmployeesModule.Core;
using EmployeesModule.Entities;
using FastEndpoints;

public sealed class ListEmployeesRequest : ICommand<Result<IEnumerable<Employee>>>
{
    // Empty request - no query parameters needed
    // Dummy property required for Swagger/OpenAPI documentation
    public bool? _ { get; init; }
}
