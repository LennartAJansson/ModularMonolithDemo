namespace WorkloadsModule.Features.CreateWorkload;

using FastEndpoints;
using WorkloadsModule.Core;
using WorkloadsModule.Features.GetWorkload;

public sealed class CreateWorkloadEndpoint : Endpoint<CreateWorkloadRequest, CreateWorkloadResponse, CreateWorkloadMapper>
{
    public override void Configure()
    {
        Post("/workloads");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateWorkloadRequest req, CancellationToken ct)
    {
        var result = await req.ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            HttpContext.Response.StatusCode = (int)result.ToHttpStatusCode();
            return;
        }

        var response = Map.FromEntity(result.Value!);
        await Send.CreatedAtAsync<GetWorkloadEndpoint>(
            new { id = result.Value.Id },
            response,
            cancellation: ct
        );
    }
}
