namespace WorkloadsModule.Features.UpdateWorkload;

using FastEndpoints;
using WorkloadsModule.Core;

public sealed class UpdateWorkloadEndpoint : Endpoint<UpdateWorkloadRequest, UpdateWorkloadResponse, UpdateWorkloadMapper>
{
    public override void Configure()
    {
        Put("/workloads/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateWorkloadRequest req, CancellationToken ct)
    {
        var result = await req.ExecuteAsync(ct);

        if (result.IsSuccess && result.Value is not null)
        {
            var response = Map.FromEntity(result.Value);
            await Send.OkAsync(response, ct);
        }
        else if (result.Error is not null)
        {
            var statusCode = result.ToHttpStatusCode();
            await Send.StringAsync(result.Error, statusCode, cancellation: ct);
        }
    }
}
