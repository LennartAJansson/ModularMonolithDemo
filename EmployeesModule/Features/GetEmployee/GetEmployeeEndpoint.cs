namespace EmployeesModule.Features.GetEmployee;

using EmployeesModule.Core;
using FastEndpoints;

public sealed class GetEmployeeEndpoint : Endpoint<GetEmployeeRequest, GetEmployeeResponse, GetEmployeeMapper>
{
    public override void Configure()
    {
        Get("/employees/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetEmployeeRequest req, CancellationToken ct)
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
