namespace CustomersModule.Features.GetCustomer;

using CustomersModule.Core;
using FastEndpoints;

public sealed class GetCustomerEndpoint : Endpoint<GetCustomerRequest, GetCustomerResponse, GetCustomerMapper>
{
    public override void Configure()
    {
        Get("/customers/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetCustomerRequest req, CancellationToken ct)
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
