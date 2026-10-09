using FluentResults;
using IrisWeb.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using IrisWeb.Application.Communication.FaceRecognition.Responses;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.Interfaces.FaceRecognition;

namespace IrisWeb.Tests.API;

public sealed class FaceRecognitionControllerTests
{
    [Fact]
    public async Task Match_Success_ReturnsResponseDtoWithoutResultEnvelope()
    {
        var expected = new MatchFaceResponse("12345", "12345", 0.42f);
        var controller = CreateController(Result.Ok(expected));

        var action = await controller.Match(new MatchFaceRequest([1f, 0f, 0f], "v1"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var body = Assert.IsType<MatchFaceResponse>(ok.Value);
        Assert.Equal(expected, body);
    }

    [Fact]
    public async Task Match_Unknown_KeepsNullScoreOnTheDto()
    {
        var expected = new MatchFaceResponse("desconhecido", null, null);
        var controller = CreateController(Result.Ok(expected));

        var action = await controller.Match(new MatchFaceRequest([1f, 0f, 0f], "v1"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var body = Assert.IsType<MatchFaceResponse>(ok.Value);
        Assert.Null(body.Score);
        Assert.Null(body.Registration);
        Assert.Equal("desconhecido", body.Label);
    }

    [Fact]
    public async Task Match_Failure_ReturnsBadRequest()
    {
        var controller = CreateController(Result.Fail<MatchFaceResponse>("Embedding vazio."));

        var action = await controller.Match(new MatchFaceRequest([], "v1"), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(action.Result);
    }

    private static FaceRecognitionController CreateController(Result<MatchFaceResponse> match) =>
        new(new StubMatchFaceUseCase(match), new StubEnrollFaceUseCase(), new StubEnrollFaceBatchUseCase());

    private sealed class StubMatchFaceUseCase(Result<MatchFaceResponse> match) : IMatchFaceUseCase
    {
        public Task<Result<MatchFaceResponse>> ExecuteAsync(
            MatchFaceRequest request,
            CancellationToken cancellationToken) => Task.FromResult(match);
    }

    private sealed class StubEnrollFaceUseCase : IEnrollFaceUseCase
    {
        public Task<Result> ExecuteAsync(EnrollFaceRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Ok());
    }

    private sealed class StubEnrollFaceBatchUseCase : IEnrollFaceBatchUseCase
    {
        public Task<Result<EnrollFaceBatchResponse>> ExecuteAsync(
            EnrollFaceBatchRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(Result.Ok(new EnrollFaceBatchResponse("enrolled", request.Embeddings.Length, null, null, 0f)));
    }
}
