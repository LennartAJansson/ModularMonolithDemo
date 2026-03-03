namespace CustomersModule.Features.ListCustomers;

using CustomersModule.Core;
using FastEndpoints;

public sealed class ListCustomersEndpoint : Endpoint<ListCustomersRequest, ListCustomersResponse, ListCustomersMapper>
{
    public override void Configure()
    {
        Get("/customers");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListCustomersRequest req, CancellationToken ct)
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
