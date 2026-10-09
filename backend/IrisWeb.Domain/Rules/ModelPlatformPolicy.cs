using IrisWeb.Domain.Enums;

namespace IrisWeb.Domain.Rules;

public static class ModelPlatformPolicy
{
    public const string WebRuntime = "onnxruntime-web";

    public const string DesktopRuntime = "openvino";

    public const string WebFormat = "onnx";

    public const string DesktopFormat = "ir";

    public const string WebEntrypointExtension = ".onnx";

    public const string DesktopEntrypointExtension = ".xml";

    public static string RequiredRuntime(ModelPlatform platform) => platform switch
    {
        ModelPlatform.Web or ModelPlatform.Mobile => WebRuntime,
        ModelPlatform.Desktop => DesktopRuntime,
        _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
    };

    public static string RequiredFormat(ModelPlatform platform) => platform switch
    {
        ModelPlatform.Web or ModelPlatform.Mobile => WebFormat,
        ModelPlatform.Desktop => DesktopFormat,
        _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
    };

    public static string RequiredEntrypointExtension(ModelPlatform platform) => platform switch
    {
        ModelPlatform.Web or ModelPlatform.Mobile => WebEntrypointExtension,
        ModelPlatform.Desktop => DesktopEntrypointExtension,
        _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
    };

    public static bool AcceptsRuntime(ModelPlatform platform, string? runtime) =>
        Matches(RequiredRuntime(platform), runtime);

    public static bool AcceptsFormat(ModelPlatform platform, string? format) =>
        Matches(RequiredFormat(platform), format);

    public static bool AcceptsEntrypoint(ModelPlatform platform, string? entrypoint) =>
        entrypoint is not null &&
        entrypoint.Trim().EndsWith(RequiredEntrypointExtension(platform), StringComparison.OrdinalIgnoreCase);

    private static bool Matches(string required, string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        string.Equals(required, value.Trim(), StringComparison.OrdinalIgnoreCase);
}
