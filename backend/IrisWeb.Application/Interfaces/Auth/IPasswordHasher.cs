namespace IrisWeb.Application.Interfaces.Auth;

public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>
    /// Com <paramref name="hash"/> null ainda gasta o mesmo tempo de uma verificação real e devolve
    /// false: matrícula inexistente não pode responder mais rápido que senha errada.
    /// </summary>
    bool Verify(string password, string? hash);
}
