using System.Text.Json.Serialization;

namespace MeOne.API.Errors;

public class ErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; }

    [JsonPropertyName("details")]
    public string Details { get; }

    public ErrorResponse(string error, string details)
    {
        Error = error;
        Details = details;
    }

    public static ErrorResponse From(string error, string details) => new(error, details);

    public static ErrorResponse From(string error, IEnumerable<string> details) =>
        new(error, string.Join("\n", details));
}
