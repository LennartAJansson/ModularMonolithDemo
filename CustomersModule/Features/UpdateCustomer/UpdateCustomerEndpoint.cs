namespace CustomersModule.Features.UpdateCustomer;

using CustomersModule.Core;
using FastEndpoints;

public sealed class UpdateCustomerEndpoint : Endpoint<UpdateCustomerRequest, UpdateCustomerResponse, UpdateCustomerMapper>
{
    public override void Configure()
    {
        Put("/customers/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateCustomerRequest req, CancellationToken ct)
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
