using IrisWeb.Domain.Entities;

namespace IrisWeb.Application.Interfaces.Auth;

/// <summary>
/// Emite o JWT de acesso. O usuário tem que vir com roles, aplicações e departamento carregados
/// (<c>IUserRepository.GetWithAccess*</c>), senão os claims saem vazios.
/// </summary>
public interface IAccessTokenIssuer
{
    string Issue(User user);
}
