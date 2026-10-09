using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Tests.Domain;

public sealed class ModelPlatformPolicyTests
{
    [Theory]
    [InlineData(ModelPlatform.Web, "onnxruntime-web", "onnx", ".onnx")]
    [InlineData(ModelPlatform.Mobile, "onnxruntime-web", "onnx", ".onnx")]
    [InlineData(ModelPlatform.Desktop, "openvino", "ir", ".xml")]
    public void CadaPlataformaTemUmValorSo(ModelPlatform platform, string runtime, string format, string extension)
    {
        Assert.Equal(runtime, ModelPlatformPolicy.RequiredRuntime(platform));
        Assert.Equal(format, ModelPlatformPolicy.RequiredFormat(platform));
        Assert.Equal(extension, ModelPlatformPolicy.RequiredEntrypointExtension(platform));
    }

    [Fact]
    public void RuntimeNomeiaOMotorEFormatoNomeiaOArquivo()
    {
        Assert.NotEqual(
            ModelPlatformPolicy.RequiredRuntime(ModelPlatform.Web),
            ModelPlatformPolicy.RequiredFormat(ModelPlatform.Web));
    }

    [Theory]
    [InlineData(ModelPlatform.Web, "onnxruntime-web", true)]
    [InlineData(ModelPlatform.Web, " ONNXRuntime-Web ", true)]
    [InlineData(ModelPlatform.Web, "onnx", false)]
    [InlineData(ModelPlatform.Web, "onnxruntime", false)]
    [InlineData(ModelPlatform.Web, "openvino", false)]
    [InlineData(ModelPlatform.Web, "", false)]
    [InlineData(ModelPlatform.Web, null, false)]
    [InlineData(ModelPlatform.Desktop, "openvino", true)]
    [InlineData(ModelPlatform.Desktop, " OpenVINO ", true)]
    [InlineData(ModelPlatform.Desktop, "onnxruntime-web", false)]
    [InlineData(ModelPlatform.Desktop, "onnx", false)]
    [InlineData(ModelPlatform.Desktop, null, false)]
    [InlineData(ModelPlatform.Mobile, "onnxruntime-web", true)]
    [InlineData(ModelPlatform.Mobile, "openvino", false)]
    [InlineData(ModelPlatform.Mobile, null, false)]
    public void RuntimeSoPassaNoValorExatoDoAlvo(ModelPlatform platform, string? runtime, bool esperado)
    {
        Assert.Equal(esperado, ModelPlatformPolicy.AcceptsRuntime(platform, runtime));
    }

    [Theory]
    [InlineData(ModelPlatform.Web, "onnx", true)]
    [InlineData(ModelPlatform.Web, "ONNX", true)]
    [InlineData(ModelPlatform.Web, "ir", false)]
    [InlineData(ModelPlatform.Desktop, "IR", true)]
    [InlineData(ModelPlatform.Desktop, "onnx", false)]
    [InlineData(ModelPlatform.Desktop, null, false)]
    public void FormatoSegueOAlvo(ModelPlatform platform, string? format, bool esperado)
    {
        Assert.Equal(esperado, ModelPlatformPolicy.AcceptsFormat(platform, format));
    }

    [Theory]
    [InlineData(ModelPlatform.Web, "facenet.onnx", true)]
    [InlineData(ModelPlatform.Web, "FACENET.ONNX", true)]
    [InlineData(ModelPlatform.Web, "facenet.xml", false)]
    [InlineData(ModelPlatform.Desktop, "fpc/model.xml", true)]
    [InlineData(ModelPlatform.Desktop, "model.bin", false)]
    [InlineData(ModelPlatform.Desktop, null, false)]
    [InlineData(ModelPlatform.Mobile, "facenet.onnx", true)]
    [InlineData(ModelPlatform.Mobile, "facenet.xml", false)]
    public void EntrypointSegueAExtensaoDaPlataforma(ModelPlatform platform, string? entrypoint, bool esperado)
    {
        Assert.Equal(esperado, ModelPlatformPolicy.AcceptsEntrypoint(platform, entrypoint));
    }

    [Fact]
    public void MobileCompartilhaMotorEFormatoComWeb()
    {
        Assert.Equal(
            ModelPlatformPolicy.RequiredRuntime(ModelPlatform.Web),
            ModelPlatformPolicy.RequiredRuntime(ModelPlatform.Mobile));
        Assert.Equal(
            ModelPlatformPolicy.RequiredFormat(ModelPlatform.Web),
            ModelPlatformPolicy.RequiredFormat(ModelPlatform.Mobile));
        Assert.Equal(
            ModelPlatformPolicy.RequiredEntrypointExtension(ModelPlatform.Web),
            ModelPlatformPolicy.RequiredEntrypointExtension(ModelPlatform.Mobile));
    }

    [Fact]
    public void NenhumValorServeAosDoisAlvos()
    {
        Assert.NotEqual(
            ModelPlatformPolicy.RequiredRuntime(ModelPlatform.Web),
            ModelPlatformPolicy.RequiredRuntime(ModelPlatform.Desktop));

        Assert.NotEqual(
            ModelPlatformPolicy.RequiredFormat(ModelPlatform.Web),
            ModelPlatformPolicy.RequiredFormat(ModelPlatform.Desktop));

        Assert.NotEqual(
            ModelPlatformPolicy.RequiredEntrypointExtension(ModelPlatform.Web),
            ModelPlatformPolicy.RequiredEntrypointExtension(ModelPlatform.Desktop));
    }
}
