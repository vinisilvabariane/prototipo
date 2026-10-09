using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Validators.Auth;
using IrisWeb.Application.Validators.Users;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Tests.Application;

public sealed class AccessValidatorTests
{
    [Fact]
    public void Login_SenhaCurtaDeCadastroAntigo_AindaPassa()
    {
        // O Access Control cadastrava senha de 6 e barrava login com menos de 8.
        var result = new LoginRequestValidator().Validate(new LoginRequest(12345, "abc123"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1_000_000u)]
    public void Login_MatriculaForaDaFaixa_Recusa(uint registration)
    {
        var result = new LoginRequestValidator().Validate(new LoginRequest(registration, "senha"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == UserErrors.RegistrationOutOfRange);
    }

    [Fact]
    public void CreateUser_EmailForaDoDominioCorporativo_Recusa()
    {
        var result = new CreateUserRequestValidator().Validate(
            new CreateUserRequest(12345, "Fulano de Tal", "fulano@gmail.com", "senha-forte", "ti"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == UserErrors.EmailNotCorporate);
    }

    [Fact]
    public void CreateUser_SemEmail_Passa()
    {
        var result = new CreateUserRequestValidator().Validate(
            new CreateUserRequest(12345, "Fulano de Tal", null, "senha-forte", "ti"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012")]
    public void CreateUser_SenhaForaDe8a72_Recusa(string password)
    {
        var result = new CreateUserRequestValidator().Validate(
            new CreateUserRequest(12345, "Fulano de Tal", null, password, "ti"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == UserErrors.PasswordLength);
    }

    [Fact]
    public void CreateUser_NomeCurtoDepoisDoTrim_Recusa()
    {
        var result = new CreateUserRequestValidator().Validate(
            new CreateUserRequest(12345, "   Fulano   ", null, "senha-forte", "ti"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == UserErrors.NameLength);
    }

    [Fact]
    public void Register_EmailObrigatorioDeQualquerDominio()
    {
        var validator = new RegisterUserRequestValidator();

        var semEmail = validator.Validate(new RegisterUserRequest("Fulano de Tal", "", 12345, "senha-forte", "ti"));
        var gmail = validator.Validate(new RegisterUserRequest("Fulano de Tal", "fulano@gmail.com", 12345, "senha-forte", "ti"));
        var invalido = validator.Validate(new RegisterUserRequest("Fulano de Tal", "fulano", 12345, "senha-forte", "ti"));

        Assert.Contains(semEmail.Errors, error => error.ErrorMessage == UserErrors.EmailRequired);
        Assert.True(gmail.IsValid);
        Assert.Contains(invalido.Errors, error => error.ErrorMessage == UserErrors.EmailInvalid);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    [InlineData(0, 101)]
    public void UsersQuery_PaginacaoForaDaFaixa_Recusa(int page, int pageSize)
    {
        var result = new UsersQueryValidator().Validate(new UsersQuery { Page = page, PageSize = pageSize });

        Assert.False(result.IsValid);
    }
}
