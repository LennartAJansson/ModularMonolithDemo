namespace WorkloadsModule.Core;

public static class ResultExtensions
{
    public static int ToHttpStatusCode(this Result result) => result.Status switch
    {
        ResultStatus.Success => 200,
        ResultStatus.NotFound => 404,
        ResultStatus.Invalid => 400,
        ResultStatus.Conflict => 409,
        ResultStatus.Failure => 500,
        _ => 500
    };
}
