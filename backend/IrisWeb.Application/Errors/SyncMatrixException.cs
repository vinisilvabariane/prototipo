namespace IrisWeb.Application.Errors;

public sealed class SyncMatrixException(string message, int upstreamStatusCode, string? detail)
    : Exception(message)
{
    public int UpstreamStatusCode { get; } = upstreamStatusCode;

    public string? Detail { get; } = detail;

    public bool IsRefusal => UpstreamStatusCode is 400 or 404;
}
