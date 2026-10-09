using IrisWeb.Domain.Entities;

namespace IrisWeb.Application.Interfaces.Users;

public interface IPasswordResetNotifier
{
    /// <summary>
    /// Envia a senha provisória para o e-mail do usuário. Não lança: devolve false quando o e-mail
    /// não está configurado ou o envio falhou.
    /// </summary>
    Task<bool> NotifyAsync(User user, string temporaryPassword, CancellationToken cancellationToken);
}
