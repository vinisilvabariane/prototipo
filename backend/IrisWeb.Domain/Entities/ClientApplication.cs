namespace IrisWeb.Domain.Entities;

/// <summary>
/// Linha da tabela <c>applications</c>: sistemas que o usuário pode acessar, publicados no claim
/// <c>applications</c> do JWT. Não se chama <c>Application</c> para não colidir com o namespace
/// <c>IrisWeb.Application</c>.
/// </summary>
public sealed class ClientApplication : Entity<ulong>
{
    private ClientApplication() { }

    public string Name { get; private set; } = string.Empty;

    public bool Active { get; private set; } = true;

    public static ClientApplication Create(string name) => new() { Name = name.Trim() };
}
