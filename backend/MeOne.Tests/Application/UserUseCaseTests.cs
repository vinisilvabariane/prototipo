using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Users;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class UserUseCaseTests
{
    private static CreateUserRequest NewUser(uint registration = 54321, string? email = "jane.roe@grupomultilaser.com.br") =>
        new(registration, "  Jane Roe Miller ", email, "new-password-1", "TI");

    [Fact]
    public async Task Create_FindsDepartmentIgnoringCase_AndStoresCreator()
    {
        var f = new AccessFixture();
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Master(userId: 7));

        var result = await useCase.ExecuteAsync(NewUser(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Jane Roe Miller", result.Value.Name);
        Assert.Equal("ti", result.Value.Department);

        var stored = Assert.Single(f.Users.Stored);
        Assert.Equal(7UL, stored.ModifiedBy);
        Assert.Equal(f.Hasher.Hash("new-password-1"), stored.PasswordHash);
    }

    [Fact]
    public async Task Create_UnknownDepartment_Fails()
    {
        var f = new AccessFixture();
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(NewUser() with { Department = "rh" }, CancellationToken.None);

        Assert.Equal(UserErrors.DepartmentInvalid, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_RegistrationOfDeactivatedUser_CountsAsTaken()
    {
        var f = new AccessFixture();
        f.AddUser(54321, email: null).Deactivate(modifiedBy: 1);
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(NewUser(), CancellationToken.None);

        Assert.Equal(UserErrors.RegistrationTaken, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_DuplicateEmailIgnoringCase_Fails()
    {
        var f = new AccessFixture();
        f.AddUser(11111, email: "jane.roe@grupomultilaser.com.br");
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(
            NewUser(email: "JANE.ROE@grupomultilaser.com.br"),
            CancellationToken.None);

        Assert.Equal(UserErrors.EmailTaken, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Register_CreatesWithoutRoleAndModifiedByZero()
    {
        var f = new AccessFixture();
        var useCase = new RegisterUserUseCase(f.Users, f.Departments, f.Hasher);

        var result = await useCase.ExecuteAsync(
            new RegisterUserRequest("Richard Roe Brown", " Richard.Roe@Gmail.com ", 777, "password-123", "TI"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(f.Users.Stored);
        Assert.Equal("richard.roe@gmail.com", stored.Email);
        Assert.Equal(0UL, stored.ModifiedBy);
        Assert.Empty(stored.Roles);
    }

    [Fact]
    public async Task Register_RegistrationAlreadyTaken_Rejects()
    {
        var f = new AccessFixture();
        f.AddUser(777);
        var useCase = new RegisterUserUseCase(f.Users, f.Departments, f.Hasher);

        var result = await useCase.ExecuteAsync(
            new RegisterUserRequest("Richard Roe Brown", "richard.roe@gmail.com", 777, "password-123", "ti"),
            CancellationToken.None);

        Assert.Equal(UserErrors.RegistrationTaken, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Update_UnknownUser_NotFound()
    {
        var f = new AccessFixture();
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(
            99,
            new UpdateUserRequest("John Doe Smith", null, 12345, "ti"),
            CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Update_KeepingOwnRegistrationAndEmail_IsNotConflict()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, email: "john.doe@grupomultilaser.com.br");
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.Master(userId: 3));

        var result = await useCase.ExecuteAsync(
            user.Id,
            new UpdateUserRequest("John Doe Smith Junior", "john.doe@grupomultilaser.com.br", 12345, "ti"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("John Doe Smith Junior", user.Name);
        Assert.Equal(3UL, user.ModifiedBy);
    }

    [Fact]
    public async Task Delete_DeactivatesAndRevokesSessions()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued");
        var useCase = new DeleteUserUseCase(f.Users, f.Tokens, FakeCurrentActor.Master(userId: 3));

        var result = await useCase.ExecuteAsync(user.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(user.Active);
        Assert.True(token.Revoked);
        Assert.True((await new GetUserUseCase(f.Users).ExecuteAsync(user.Id, CancellationToken.None))
            .HasError<NotFoundError>());
    }

    [Fact]
    public async Task Delete_OwnUser_Rejects()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var useCase = new DeleteUserUseCase(f.Users, f.Tokens, FakeCurrentActor.Master(userId: user.Id));

        var result = await useCase.ExecuteAsync(user.Id, CancellationToken.None);

        Assert.Equal(UserErrors.CannotDeleteSelf, Assert.Single(result.Errors).Message);
        Assert.True(user.Active);
    }

    private static ResetPasswordUseCase Reset(AccessFixture f, FakePasswordResetNotifier notifier, FakeCurrentActor actor) =>
        new(f.Users, f.Tokens, f.Hasher, notifier, actor);

    [Fact]
    public async Task ResetPassword_Anonymous_SendsOnlyByEmailAndDoesNotReturnPassword()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued");
        var notifier = new FakePasswordResetNotifier();

        var result = await Reset(f, notifier, FakeCurrentActor.Anonymous()).ExecuteAsync(12345, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.NewPassword);
        Assert.True(result.Value.EmailSent);

        var (_, password) = Assert.Single(notifier.Sent);
        Assert.StartsWith(TemporaryPasswordPolicy.Prefix, password);
        Assert.Equal(f.Hasher.Hash(password), user.PasswordHash);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task ResetPassword_AnonymousWithoutEmail_RejectsAndKeepsPassword()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, email: null);
        var hashBefore = user.PasswordHash;

        var result = await Reset(f, new FakePasswordResetNotifier(), FakeCurrentActor.Anonymous())
            .ExecuteAsync(12345, CancellationToken.None);

        Assert.Equal(UserErrors.NoEmailForReset, Assert.Single(result.Errors).Message);
        Assert.Equal(hashBefore, user.PasswordHash);
    }

    [Fact]
    public async Task ResetPassword_AnonymousWithEmailFailure_RestoresOldPassword()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued");
        var hashBefore = user.PasswordHash;

        var result = await Reset(f, new FakePasswordResetNotifier(succeeds: false), FakeCurrentActor.Anonymous())
            .ExecuteAsync(12345, CancellationToken.None);

        Assert.Equal(UserErrors.ResetEmailFailed, Assert.Single(result.Errors).Message);
        Assert.Equal(hashBefore, user.PasswordHash);
        Assert.False(token.Revoked);
    }

    [Fact]
    public async Task ResetPassword_Master_ReceivesPasswordEvenWithoutEmail()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, email: null);

        var result = await Reset(f, new FakePasswordResetNotifier(), FakeCurrentActor.Master(userId: 3))
            .ExecuteAsync(12345, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.EmailSent);
        Assert.NotNull(result.Value.NewPassword);
        Assert.Equal(f.Hasher.Hash(result.Value.NewPassword!), user.PasswordHash);
        Assert.Equal(3UL, user.ModifiedBy);
    }

    [Fact]
    public async Task ResetPassword_UnknownRegistration_NotFound()
    {
        var f = new AccessFixture();

        var result = await Reset(f, new FakePasswordResetNotifier(), FakeCurrentActor.Anonymous())
            .ExecuteAsync(1, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task ChangePassword_ChangesAndRevokesSessions()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued");
        var useCase = new ChangePasswordUseCase(f.Users, f.Tokens, f.Hasher, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(new ChangePasswordRequest(user.Id, "new-password-9"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(f.Hasher.Hash("new-password-9"), user.PasswordHash);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task List_PageStartsAtZero_AndTotalCountsAll()
    {
        var f = new AccessFixture();
        for (uint registration = 1; registration <= 5; registration++)
            f.AddUser(registration, email: $"u{registration}@grupomultilaser.com.br");

        var result = await new ListUsersUseCase(f.Users).ExecuteAsync(
            new UsersQuery { Page = 1, PageSize = 2 },
            CancellationToken.None);

        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal([3u, 4u], result.Value.Items.Select(user => user.Registration));
    }

    [Fact]
    public async Task List_SearchesByExactRegistration()
    {
        var f = new AccessFixture();
        f.AddUser(123, email: "a@grupomultilaser.com.br");
        f.AddUser(1234, email: "b@grupomultilaser.com.br");

        var result = await new ListUsersUseCase(f.Users).ExecuteAsync(
            new UsersQuery { Search = " 123 " },
            CancellationToken.None);

        Assert.Equal(123u, Assert.Single(result.Value.Items).Registration);
    }

    [Fact]
    public async Task ListUserRoles_UnknownUser_EmptyList()
    {
        var f = new AccessFixture();

        var result = await new ListUserRolesUseCase(f.Users).ExecuteAsync(42, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByRegistration_NotFound_Returns400LikeAccessControl()
    {
        var f = new AccessFixture();

        var result = await new GetUserByRegistrationUseCase(f.Users).ExecuteAsync(1, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.False(result.HasError<NotFoundError>());
    }
}
