namespace IrisWeb.Application.Communication.Users.Requests;

/// <summary>Autocadastro pela tela pública: a matrícula precisa existir no Sync.</summary>
public sealed record RegisterUserRequest(
    string Name,
    string Email,
    uint Registration,
    string Password,
    string Department);
