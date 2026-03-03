namespace EmployeesModule.Features.ListEmployees;

using EmployeesModule.Core;
using FastEndpoints;

public sealed class ListEmployeesEndpoint : Endpoint<ListEmployeesRequest, ListEmployeesResponse, ListEmployeesMapper>
{
    public override void Configure()
    {
        Get("/employees");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListEmployeesRequest req, CancellationToken ct)
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
