namespace CustomersModule.Features.CreateCustomer;

using CustomersModule.Core;
using CustomersModule.Entities;
using CustomersModule.Features.GetCustomer;
using FastEndpoints;

public sealed class CreateCustomerEndpoint : Endpoint<CreateCustomerRequest, CreateCustomerResponse, CreateCustomerMapper>
{
    public override void Configure()
    {
        Post("/customers");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
    {
        var result = await req.ExecuteAsync(ct);

        if (result.IsSuccess && result.Value is not null)
        {
            var response = Map.FromEntity(result.Value);
            await Send.CreatedAtAsync<GetCustomerEndpoint>(
                new { id = result.Value.Id },
                response,
                cancellation: ct
            );
        }
        else if (result.Error is not null)
        {
            var statusCode = result.ToHttpStatusCode();
            await Send.StringAsync(result.Error, statusCode, cancellation: ct);
        }
    }
}
