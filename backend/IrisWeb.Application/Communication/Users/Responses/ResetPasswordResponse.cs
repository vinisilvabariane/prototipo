namespace IrisWeb.Application.Communication.Users.Responses;

/// <summary>
/// <c>NewPassword</c> só vem preenchida quando quem pediu o reset é master; para o pedido anônimo
/// a senha vai apenas por e-mail.
/// </summary>
public sealed record ResetPasswordResponse(string? NewPassword, bool EmailSent);
