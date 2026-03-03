namespace EmployeesModule.Features.DeleteEmployee;

using EmployeesModule.Core;
using FastEndpoints;

public sealed class DeleteEmployeeEndpoint : Endpoint<DeleteEmployeeRequest>
{
    public override void Configure()
    {
        Delete("/employees/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteEmployeeRequest req, CancellationToken ct)
    {
        var result = await req.ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            HttpContext.Response.StatusCode = (int)result.ToHttpStatusCode();
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
