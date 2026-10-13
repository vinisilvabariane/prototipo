using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Validators.Auth;
using MeOne.Application.Validators.Users;
using MeOne.Domain.Errors;

namespace MeOne.Tests.Application;

public sealed class AccessValidatorTests
{
    [Fact]
    public void Login_ShortLegacyPassword_StillPasses()
    {
        var result = new LoginRequestValidator().Validate(new LoginRequest(12345, "abc123"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1_000_000u)]
    public void Login_RegistrationOutOfRange_Rejects(uint registration)
    {
        var result = new LoginRequestValidator().Validate(new LoginRequest(registration, "password"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == UserErrors.RegistrationOutOfRange);
    }

    [Fact]
    public void CreateUser_EmailOutsideCorporateDomain_Rejects()
    {
        var result = new CreateUserRequestValidator().Validate(
            new CreateUserRequest(12345, "John Doe Smith", "john.doe@gmail.com", "strong-password", "ti"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == UserErrors.EmailNotCorporate);
    }

    [Fact]
    public void CreateUser_WithoutEmail_Passes()
    {
        var result = new CreateUserRequestValidator().Validate(
            new CreateUserRequest(12345, "John Doe Smith", null, "strong-password", "ti"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012")]
    public void CreateUser_PasswordOutside8To72_Rejects(string password)
    {
        var result = new CreateUserRequestValidator().Validate(
            new CreateUserRequest(12345, "John Doe Smith", null, password, "ti"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == UserErrors.PasswordLength);
    }

    [Fact]
    public void CreateUser_NameTooShortAfterTrim_Rejects()
    {
        var result = new CreateUserRequestValidator().Validate(
            new CreateUserRequest(12345, "   John   ", null, "strong-password", "ti"));

        Assert.Contains(result.Errors, error => error.ErrorMessage == UserErrors.NameLength);
    }

    [Fact]
    public void Register_EmailRequiredFromAnyDomain()
    {
        var validator = new RegisterUserRequestValidator();

        var withoutEmail = validator.Validate(new RegisterUserRequest("John Doe Smith", "", 12345, "strong-password", "ti"));
        var gmail = validator.Validate(new RegisterUserRequest("John Doe Smith", "john.doe@gmail.com", 12345, "strong-password", "ti"));
        var invalid = validator.Validate(new RegisterUserRequest("John Doe Smith", "john.doe", 12345, "strong-password", "ti"));

        Assert.Contains(withoutEmail.Errors, error => error.ErrorMessage == UserErrors.EmailRequired);
        Assert.True(gmail.IsValid);
        Assert.Contains(invalid.Errors, error => error.ErrorMessage == UserErrors.EmailInvalid);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    [InlineData(0, 101)]
    public void UsersQuery_PagingOutOfRange_Rejects(int page, int pageSize)
    {
        var result = new UsersQueryValidator().Validate(new UsersQuery { Page = page, PageSize = pageSize });

        Assert.False(result.IsValid);
    }
}
