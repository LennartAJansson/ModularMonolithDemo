namespace WorkloadsModule.Features.ListWorkloads;

using FastEndpoints;
using WorkloadsModule.Core;

public sealed class ListWorkloadsEndpoint : Endpoint<ListWorkloadsRequest, ListWorkloadsResponse, ListWorkloadsMapper>
{
    public override void Configure()
    {
        Get("/workloads");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListWorkloadsRequest req, CancellationToken ct)
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
