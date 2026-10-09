namespace IrisWeb.Application.Interfaces;

public interface ICurrentActor
{

    string? Registration { get; }

    /// <summary>Id do usuário logado (claim <c>sub</c>); null quando a chamada é anônima.</summary>
    ulong? UserId { get; }

    bool IsInRole(string role);
}
