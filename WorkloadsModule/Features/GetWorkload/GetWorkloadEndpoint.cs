namespace WorkloadsModule.Features.GetWorkload;

using FastEndpoints;
using WorkloadsModule.Core;

public sealed class GetWorkloadEndpoint : Endpoint<GetWorkloadRequest, GetWorkloadResponse, GetWorkloadMapper>
{
    public override void Configure()
    {
        Get("/workloads/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetWorkloadRequest req, CancellationToken ct)
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
