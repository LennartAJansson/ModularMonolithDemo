namespace EmployeesModule.Features.CreateEmployee;

using EmployeesModule.Core;
using EmployeesModule.Features.GetEmployee;
using FastEndpoints;

public sealed class CreateEmployeeEndpoint : Endpoint<CreateEmployeeRequest, CreateEmployeeResponse, CreateEmployeeMapper>
{
    public override void Configure()
    {
        Post("/employees");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateEmployeeRequest req, CancellationToken ct)
    {
        var result = await req.ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            HttpContext.Response.StatusCode = (int)result.ToHttpStatusCode();
            return;
        }

        var response = Map.FromEntity(result.Value!);
        await Send.CreatedAtAsync<GetEmployeeEndpoint>(
            new { id = result.Value.Id },
            response,
            cancellation: ct
        );
    }
}
