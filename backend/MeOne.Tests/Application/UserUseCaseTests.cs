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
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Administrator(userId: 7));

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
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Administrator());

        var result = await useCase.ExecuteAsync(NewUser() with { Department = "rh" }, CancellationToken.None);

        Assert.Equal(UserErrors.DepartmentInvalid, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_RegistrationOfDeactivatedUser_CountsAsTaken()
    {
        var f = new AccessFixture();
        f.AddUser(54321, email: null).Deactivate(modifiedBy: 1);
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Administrator());

        var result = await useCase.ExecuteAsync(NewUser(), CancellationToken.None);

        Assert.Equal(UserErrors.RegistrationTaken, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_DuplicateEmailIgnoringCase_Fails()
    {
        var f = new AccessFixture();
        f.AddUser(11111, email: "jane.roe@grupomultilaser.com.br");
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Administrator());

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
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.Administrator());

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
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.Administrator(userId: 3));

        var result = await useCase.ExecuteAsync(
            user.Id,
            new UpdateUserRequest("John Doe Smith Junior", "john.doe@grupomultilaser.com.br", 12345, "ti"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("John Doe Smith Junior", user.Name);
        Assert.Equal(3UL, user.ModifiedBy);
    }

    [Fact]
    public async Task Update_AccessManagerCannotEditAdministrator()
    {
        var f = new AccessFixture();
        var admin = f.AddAdministrator();
        var manager = f.AddUser(54321, "manager@grupomultilaser.com.br", roles: f.AccessManagerRole);
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.For(manager));

        var result = await useCase.ExecuteAsync(
            admin.Id,
            new UpdateUserRequest("Renamed Administrator", null, admin.Registration, "ti"),
            CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.Equal(UserErrors.TargetHasMorePrivileges, result.Errors[0].Message);
    }

    [Fact]
    public async Task Update_AccessManagerCannotEditUserWithPermissionsItLacks()
    {
        var f = new AccessFixture();
        var operatorUser = f.AddUser(12345, roles: f.OperatorRole);
        var manager = f.AddUser(54321, "manager@grupomultilaser.com.br", roles: f.AccessManagerRole);
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.For(manager));

        var result = await useCase.ExecuteAsync(
            operatorUser.Id,
            new UpdateUserRequest("Operator Renamed Name", null, 12345, "ti"),
            CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task Update_AccessManagerEditsUserWithoutRoles()
    {
        var f = new AccessFixture();
        var plain = f.AddUser(12345);
        var manager = f.AddUser(54321, "manager@grupomultilaser.com.br", roles: f.AccessManagerRole);
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.For(manager));

        var result = await useCase.ExecuteAsync(
            plain.Id,
            new UpdateUserRequest("Plain User Renamed", null, 12345, "ti"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Delete_DeactivatesRevokesSessionsAndInvalidatesAccess()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued");
        var useCase = new DeleteUserUseCase(f.Users, f.Tokens, f.AccessProfiles, FakeCurrentActor.Administrator(userId: 3));

        var result = await useCase.ExecuteAsync(user.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(user.Active);
        Assert.True(token.Revoked);
        Assert.Contains(user.Id, f.AccessProfiles.Invalidated);
    }

    [Fact]
    public async Task Delete_OwnUser_Rejects()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var useCase = new DeleteUserUseCase(f.Users, f.Tokens, f.AccessProfiles, FakeCurrentActor.Administrator(userId: user.Id));

        var result = await useCase.ExecuteAsync(user.Id, CancellationToken.None);

        Assert.Equal(UserErrors.CannotDeleteSelf, Assert.Single(result.Errors).Message);
        Assert.True(user.Active);
    }

    [Fact]
    public async Task Delete_LastAdministrator_Rejects()
    {
        var f = new AccessFixture();
        var admin = f.AddAdministrator();
        var useCase = new DeleteUserUseCase(f.Users, f.Tokens, f.AccessProfiles, FakeCurrentActor.Administrator(userId: 999));

        var result = await useCase.ExecuteAsync(admin.Id, CancellationToken.None);

        Assert.Equal(UserErrors.LastAdministrator, Assert.Single(result.Errors).Message);
        Assert.True(admin.Active);
    }

    private static RequestPasswordResetUseCase RequestReset(AccessFixture f, FakePasswordResetNotifier notifier) =>
        new(f.Users, f.ResetTokens, f.RefreshTokens, notifier);

    private static ConfirmPasswordResetUseCase ConfirmReset(AccessFixture f) =>
        new(f.Users, f.ResetTokens, f.Tokens, f.RefreshTokens, f.Hasher, f.AccessProfiles);

    [Fact]
    public async Task RequestPasswordReset_SendsCodeAndKeepsCurrentPassword()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var hashBefore = user.PasswordHash;
        var notifier = new FakePasswordResetNotifier();

        var result = await RequestReset(f, notifier).ExecuteAsync(new RequestPasswordResetRequest(12345), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var sent = Assert.Single(notifier.Sent);
        Assert.Equal(PasswordResetPolicy.CodeLength, sent.Code.Length);
        Assert.Equal(hashBefore, user.PasswordHash);
        Assert.Equal(f.RefreshTokens.Hash(sent.Code), Assert.Single(f.ResetTokens.Stored).CodeHash);
    }

    [Fact]
    public async Task RequestPasswordReset_UnknownOrWithoutEmail_SucceedsSilently()
    {
        var f = new AccessFixture();
        f.AddUser(12345, email: null);
        var notifier = new FakePasswordResetNotifier();

        var unknown = await RequestReset(f, notifier).ExecuteAsync(new RequestPasswordResetRequest(999), CancellationToken.None);
        var withoutEmail = await RequestReset(f, notifier).ExecuteAsync(new RequestPasswordResetRequest(12345), CancellationToken.None);

        Assert.True(unknown.IsSuccess);
        Assert.True(withoutEmail.IsSuccess);
        Assert.Empty(notifier.Sent);
        Assert.Empty(f.ResetTokens.Stored);
    }

    [Fact]
    public async Task RequestPasswordReset_RepeatedWithinAMinute_DoesNotSendAgain()
    {
        var f = new AccessFixture();
        f.AddUser(12345);
        var notifier = new FakePasswordResetNotifier();

        await RequestReset(f, notifier).ExecuteAsync(new RequestPasswordResetRequest(12345), CancellationToken.None);
        await RequestReset(f, notifier).ExecuteAsync(new RequestPasswordResetRequest(12345), CancellationToken.None);

        Assert.Single(notifier.Sent);
    }

    [Fact]
    public async Task ConfirmPasswordReset_RightCode_ChangesPasswordAndRevokesSessions()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var session = f.AddRefreshToken(user, "issued");
        var notifier = new FakePasswordResetNotifier();
        await RequestReset(f, notifier).ExecuteAsync(new RequestPasswordResetRequest(12345), CancellationToken.None);
        var code = notifier.Sent[0].Code;

        var result = await ConfirmReset(f).ExecuteAsync(
            new ConfirmPasswordResetRequest(12345, code.ToLowerInvariant(), "new-password-9"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(f.Hasher.Hash("new-password-9"), user.PasswordHash);
        Assert.True(session.Revoked);
        Assert.NotNull(f.ResetTokens.Stored[0].UsedAt);
    }

    [Fact]
    public async Task ConfirmPasswordReset_CodeCannotBeUsedTwice()
    {
        var f = new AccessFixture();
        f.AddUser(12345);
        var notifier = new FakePasswordResetNotifier();
        await RequestReset(f, notifier).ExecuteAsync(new RequestPasswordResetRequest(12345), CancellationToken.None);
        var request = new ConfirmPasswordResetRequest(12345, notifier.Sent[0].Code, "new-password-9");

        await ConfirmReset(f).ExecuteAsync(request, CancellationToken.None);
        var second = await ConfirmReset(f).ExecuteAsync(request with { NewPassword = "another-password" }, CancellationToken.None);

        Assert.Equal(AuthErrors.InvalidResetCode, Assert.Single(second.Errors).Message);
    }

    [Fact]
    public async Task ConfirmPasswordReset_TooManyWrongCodes_InvalidatesToken()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var hashBefore = user.PasswordHash;
        var notifier = new FakePasswordResetNotifier();
        await RequestReset(f, notifier).ExecuteAsync(new RequestPasswordResetRequest(12345), CancellationToken.None);

        for (var attempt = 0; attempt < PasswordResetPolicy.MaxFailedAttempts; attempt++)
        {
            await ConfirmReset(f).ExecuteAsync(
                new ConfirmPasswordResetRequest(12345, "ZZZZZZZZ", "new-password-9"),
                CancellationToken.None);
        }

        var right = await ConfirmReset(f).ExecuteAsync(
            new ConfirmPasswordResetRequest(12345, notifier.Sent[0].Code, "new-password-9"),
            CancellationToken.None);

        Assert.True(right.IsFailed);
        Assert.Equal(hashBefore, user.PasswordHash);
    }

    [Fact]
    public async Task ConfirmPasswordReset_UnknownRegistration_SameMessageAsWrongCode()
    {
        var f = new AccessFixture();

        var result = await ConfirmReset(f).ExecuteAsync(
            new ConfirmPasswordResetRequest(999, "ABCDEFGH", "new-password-9"),
            CancellationToken.None);

        Assert.Equal(AuthErrors.InvalidResetCode, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task IssueTemporaryPassword_ReturnsStrongPasswordAndRevokesSessions()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, email: null);
        var session = f.AddRefreshToken(user, "issued");
        var useCase = new IssueTemporaryPasswordUseCase(
            f.Users, f.Tokens, f.Hasher, f.AccessProfiles, FakeCurrentActor.Administrator(userId: 3));

        var result = await useCase.ExecuteAsync(user.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.StartsWith(TemporaryPasswordPolicy.Prefix, result.Value.NewPassword);
        Assert.Equal(TemporaryPasswordPolicy.Prefix.Length + TemporaryPasswordPolicy.RandomLength, result.Value.NewPassword.Length);
        Assert.Equal(f.Hasher.Hash(result.Value.NewPassword), user.PasswordHash);
        Assert.True(session.Revoked);
    }

    [Fact]
    public async Task IssueTemporaryPassword_AccessManagerCannotResetAdministrator()
    {
        var f = new AccessFixture();
        var admin = f.AddAdministrator();
        var manager = f.AddUser(54321, "manager@grupomultilaser.com.br", roles: f.AccessManagerRole);
        var hashBefore = admin.PasswordHash;
        var useCase = new IssueTemporaryPasswordUseCase(
            f.Users, f.Tokens, f.Hasher, f.AccessProfiles, FakeCurrentActor.For(manager));

        var result = await useCase.ExecuteAsync(admin.Id, CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.Equal(hashBefore, admin.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_ChangesAndRevokesSessions()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued");
        var useCase = new ChangePasswordUseCase(f.Users, f.Tokens, f.Hasher, f.AccessProfiles, FakeCurrentActor.Administrator());

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
