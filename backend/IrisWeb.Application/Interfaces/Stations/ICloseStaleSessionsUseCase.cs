using FluentResults;

namespace IrisWeb.Application.Interfaces.Stations;

public interface ICloseStaleSessionsUseCase
{
    /// <summary>
    /// Fecha presenca, vacancia e rosto desconhecido abertos ha mais que <paramref name="maxOpenAge"/>,
    /// de qualquer posto. Rede de seguranca contra sessao que nenhum app jamais voltou a abrir para
    /// fechar sozinha (ver <c>OpenDevice*UseCase.CloseStrayOpenAsync</c>, que cobre o caso comum).
    /// Devolve quantas linhas fechou.
    /// </summary>
    Task<Result<int>> ExecuteAsync(TimeSpan maxOpenAge, CancellationToken cancellationToken);
}
