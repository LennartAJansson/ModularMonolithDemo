namespace WorkloadsModule.Features.DeleteWorkload;

using FastEndpoints;
using WorkloadsModule.Core;

public sealed class DeleteWorkloadEndpoint : Endpoint<DeleteWorkloadRequest>
{
    public override void Configure()
    {
        Delete("/workloads/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteWorkloadRequest req, CancellationToken ct)
    {
        var result = await req.ExecuteAsync(ct);

        if (result.IsSuccess)
        {
            await Send.NoContentAsync(ct);
        }
        else if (result.Error is not null)
        {
            var statusCode = result.ToHttpStatusCode();
            await Send.StringAsync(result.Error, statusCode, cancellation: ct);
        }
    }
}
