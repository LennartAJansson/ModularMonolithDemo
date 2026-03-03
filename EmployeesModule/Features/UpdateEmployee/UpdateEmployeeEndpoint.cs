namespace EmployeesModule.Features.UpdateEmployee;

using EmployeesModule.Core;
using FastEndpoints;

public sealed class UpdateEmployeeEndpoint : Endpoint<UpdateEmployeeRequest, UpdateEmployeeResponse, UpdateEmployeeMapper>
{
    public override void Configure()
    {
        Put("/employees/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateEmployeeRequest req, CancellationToken ct)
    {
        var result = await req.ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            HttpContext.Response.StatusCode = (int)result.ToHttpStatusCode();
            return;
        }

        var response = Map.FromEntity(result.Value!);
        await Send.OkAsync(response, ct);
    }
}
