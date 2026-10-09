using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.UseCases.Users;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Rules;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class UserUseCaseTests
{
    private static CreateUserRequest NewUser(uint registration = 54321, string? email = "beltrano@grupomultilaser.com.br") =>
        new(registration, "  Beltrano da Silva ", email, "senha-nova-1", "TI");

    [Fact]
    public async Task Create_ProcuraDepartamentoSemDiferenciarCaixa_EGravaQuemCriou()
    {
        var f = new AccessFixture();
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Master(userId: 7));

        var result = await useCase.ExecuteAsync(NewUser(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Beltrano da Silva", result.Value.Name);
        Assert.Equal("ti", result.Value.Department);

        var stored = Assert.Single(f.Users.Stored);
        Assert.Equal(7UL, stored.ModifiedBy);
        Assert.Equal(f.Hasher.Hash("senha-nova-1"), stored.PasswordHash);
    }

    [Fact]
    public async Task Create_DepartamentoInexistente_Falha()
    {
        var f = new AccessFixture();
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(NewUser() with { Department = "rh" }, CancellationToken.None);

        Assert.Equal(UserErrors.DepartmentInvalid, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_MatriculaDeUsuarioDesativado_ContaComoOcupada()
    {
        var f = new AccessFixture();
        f.AddUser(54321, email: null).Deactivate(modifiedBy: 1);
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(NewUser(), CancellationToken.None);

        Assert.Equal(UserErrors.RegistrationTaken, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_EmailRepetido_IgnorandoCaixa_Falha()
    {
        var f = new AccessFixture();
        f.AddUser(11111, email: "beltrano@grupomultilaser.com.br");
        var useCase = new CreateUserUseCase(f.Users, f.Departments, f.Hasher, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(
            NewUser(email: "BELTRANO@grupomultilaser.com.br"),
            CancellationToken.None);

        Assert.Equal(UserErrors.EmailTaken, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Register_MatriculaForaDoSync_Recusa()
    {
        var f = new AccessFixture();
        var useCase = new RegisterUserUseCase(f.Users, f.Departments, f.Hasher, new FakeSyncMatrixClient());

        var result = await useCase.ExecuteAsync(
            new RegisterUserRequest("Ciclano de Souza", "ciclano@gmail.com", 777, "senha-123", "ti"),
            CancellationToken.None);

        Assert.Equal(UserErrors.NotInCollaboratorBase, Assert.Single(result.Errors).Message);
        Assert.Empty(f.Users.Stored);
    }

    [Fact]
    public async Task Register_SyncForaDoAr_Responde502()
    {
        var f = new AccessFixture();
        var sync = new FakeSyncMatrixClient { Failure = new HttpRequestException("timeout") };
        sync.Collaborators[777] = new SyncCollaborator(1, 777, "CICLANO");
        var useCase = new RegisterUserUseCase(f.Users, f.Departments, f.Hasher, sync);

        var result = await useCase.ExecuteAsync(
            new RegisterUserRequest("Ciclano de Souza", "ciclano@gmail.com", 777, "senha-123", "ti"),
            CancellationToken.None);

        Assert.True(result.HasError<UpstreamGatewayError>());
    }

    [Fact]
    public async Task Register_ColaboradorDoSync_CriaSemRoleEComModifiedByZero()
    {
        var f = new AccessFixture();
        var sync = new FakeSyncMatrixClient();
        sync.Collaborators[777] = new SyncCollaborator(1, 777, "CICLANO");
        var useCase = new RegisterUserUseCase(f.Users, f.Departments, f.Hasher, sync);

        var result = await useCase.ExecuteAsync(
            new RegisterUserRequest("Ciclano de Souza", " Ciclano@Gmail.com ", 777, "senha-123", "TI"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(f.Users.Stored);
        Assert.Equal("ciclano@gmail.com", stored.Email);
        Assert.Equal(0UL, stored.ModifiedBy);
        Assert.Empty(stored.Roles);
    }

    [Fact]
    public async Task Update_UsuarioInexistente_NotFound()
    {
        var f = new AccessFixture();
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(
            99,
            new UpdateUserRequest("Fulano de Tal", null, 12345, "ti"),
            CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Update_MantendoAPropriaMatriculaEEmail_NaoContaComoConflito()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, email: "fulano@grupomultilaser.com.br");
        var useCase = new UpdateUserUseCase(f.Users, f.Departments, FakeCurrentActor.Master(userId: 3));

        var result = await useCase.ExecuteAsync(
            user.Id,
            new UpdateUserRequest("Fulano de Tal Junior", "fulano@grupomultilaser.com.br", 12345, "ti"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Fulano de Tal Junior", user.Name);
        Assert.Equal(3UL, user.ModifiedBy);
    }

    [Fact]
    public async Task Delete_DesativaEDerrubaAsSessoes()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "cru");
        var useCase = new DeleteUserUseCase(f.Users, f.Tokens, FakeCurrentActor.Master(userId: 3));

        var result = await useCase.ExecuteAsync(user.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(user.Active);
        Assert.True(token.Revoked);
        Assert.True((await new GetUserUseCase(f.Users).ExecuteAsync(user.Id, CancellationToken.None))
            .HasError<NotFoundError>());
    }

    [Fact]
    public async Task Delete_OProprioUsuario_Recusa()
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
    public async Task ResetPassword_Anonimo_MandaSoPorEmailENaoDevolveASenha()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "cru");
        var notifier = new FakePasswordResetNotifier();

        var result = await Reset(f, notifier, FakeCurrentActor.Anonymous()).ExecuteAsync(12345, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.NewPassword);
        Assert.True(result.Value.EmailSent);

        var (_, senha) = Assert.Single(notifier.Sent);
        Assert.StartsWith(TemporaryPasswordPolicy.Prefix, senha);
        Assert.Equal(f.Hasher.Hash(senha), user.PasswordHash);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task ResetPassword_AnonimoSemEmail_RecusaEMantemASenha()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, email: null);
        var hashAntes = user.PasswordHash;

        var result = await Reset(f, new FakePasswordResetNotifier(), FakeCurrentActor.Anonymous())
            .ExecuteAsync(12345, CancellationToken.None);

        Assert.Equal(UserErrors.NoEmailForReset, Assert.Single(result.Errors).Message);
        Assert.Equal(hashAntes, user.PasswordHash);
    }

    [Fact]
    public async Task ResetPassword_AnonimoComFalhaNoEmail_VoltaASenhaAntiga()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "cru");
        var hashAntes = user.PasswordHash;

        var result = await Reset(f, new FakePasswordResetNotifier(succeeds: false), FakeCurrentActor.Anonymous())
            .ExecuteAsync(12345, CancellationToken.None);

        Assert.Equal(UserErrors.ResetEmailFailed, Assert.Single(result.Errors).Message);
        Assert.Equal(hashAntes, user.PasswordHash);
        Assert.False(token.Revoked);
    }

    [Fact]
    public async Task ResetPassword_Master_RecebeASenhaMesmoSemEmail()
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
    public async Task ResetPassword_MatriculaInexistente_NotFound()
    {
        var f = new AccessFixture();

        var result = await Reset(f, new FakePasswordResetNotifier(), FakeCurrentActor.Anonymous())
            .ExecuteAsync(1, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task ChangePassword_TrocaODerrubaSessoes()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "cru");
        var useCase = new ChangePasswordUseCase(f.Users, f.Tokens, f.Hasher, FakeCurrentActor.Master());

        var result = await useCase.ExecuteAsync(new ChangePasswordRequest(user.Id, "nova-senha-9"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(f.Hasher.Hash("nova-senha-9"), user.PasswordHash);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task List_PaginaComecaEmZero_ETotalContaTodos()
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
    public async Task List_BuscaPorMatriculaExata()
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
    public async Task ListUserRoles_UsuarioInexistente_ListaVazia()
    {
        var f = new AccessFixture();

        var result = await new ListUserRolesUseCase(f.Users).ExecuteAsync(42, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByRegistration_NaoEncontrado_Responde400ComoNoAccessControl()
    {
        var f = new AccessFixture();

        var result = await new GetUserByRegistrationUseCase(f.Users).ExecuteAsync(1, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.False(result.HasError<NotFoundError>());
    }
}
