using System.Text.Json;
using System.Text.Json.Serialization;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Enums;

namespace IrisWeb.Tests.API;

public sealed class EnumWireFormatTests
{

    private static readonly JsonSerializerOptions Options = CriarOptions();

    private static JsonSerializerOptions CriarOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static CatalogModelVersionResponse VersaoCom(ModelVersionStatus status) => new(
        Guid.NewGuid(), "1.0.0", "onnx", status, 1024, "abc123", null,
        "vinicius", null, DateTime.UtcNow, null);

    private static string CampoTexto(object valor, string propriedade) =>
        JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(valor, Options))
            .GetProperty(propriedade)
            .GetString()!;

    [Theory]
    [InlineData(ModelVersionStatus.Approved, "approved")]
    [InlineData(ModelVersionStatus.Candidate, "candidate")]
    public void StatusDaVersaoVaiParaOFioComoStringCamelCase(ModelVersionStatus status, string esperado)
    {
        Assert.Equal(esperado, CampoTexto(VersaoCom(status), "status"));
    }

    [Fact]
    public void TipoDeTarefaDoModeloVaiParaOFioComoStringCamelCase()
    {
        var modelo = new CatalogModelResponse(
            Guid.NewGuid(), "facenet", "FaceNet", null, AiTaskType.FaceRecognition, ModelPlatform.Web,
            "vinicius", DateTime.UtcNow, [], null);

        Assert.Equal("faceRecognition", CampoTexto(modelo, "taskType"));
    }

    [Fact]
    public void EnumNuncaSaiComoNumero()
    {
        var json = JsonSerializer.Serialize(VersaoCom(ModelVersionStatus.Approved), Options);

        Assert.DoesNotContain("\"status\":3", json);
        Assert.Contains("\"status\":\"approved\"", json);
    }

    [Theory]
    [InlineData(ModelPlatform.Web, "web")]
    [InlineData(ModelPlatform.Desktop, "desktop")]
    [InlineData(ModelPlatform.Mobile, "mobile")]
    public void PlataformaDoModeloVaiParaOFioComoStringCamelCase(ModelPlatform platform, string esperado)
    {
        var modelo = new CatalogModelResponse(
            Guid.NewGuid(), "facenet", "FaceNet", null, AiTaskType.FaceRecognition, platform,
            "vinicius", DateTime.UtcNow, [], null);

        Assert.Equal(esperado, CampoTexto(modelo, "platform"));
    }
}
