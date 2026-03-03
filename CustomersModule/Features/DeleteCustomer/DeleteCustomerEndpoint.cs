namespace CustomersModule.Features.DeleteCustomer;

using CustomersModule.Core;
using FastEndpoints;

public sealed class DeleteCustomerEndpoint : Endpoint<DeleteCustomerRequest>
{
    public override void Configure()
    {
        Delete("/customers/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteCustomerRequest req, CancellationToken ct)
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
