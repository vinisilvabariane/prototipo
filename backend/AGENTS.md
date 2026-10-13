# MeOne Backend — Guia de Trabalho

## Escopo

Estas instruções valem para `meone/backend`. Quatro projetos de código e um de teste:

- `MeOne.Api` (pasta `MeOne.API`) — controllers, autenticação (emissão e validação de JWT), Swagger, tradução `Result` → HTTP
- `MeOne.Application` — casos de uso, contratos, DTOs de fio, validators
- `MeOne.Domain` — entidades, regras, errors, contratos de repositório
- `MeOne.Infrastructure` — `DbContext`, mappings EF, repositories, identidade, e-mail, health checks
- `MeOne.Tests` — xUnit, espelhando as camadas

O alvo é **Clean Architecture**. Hoje o escopo funcional é o controle de acesso: autenticação e
CRUD de usuários, roles e departamentos. Módulo novo segue a mesma estrutura descrita aqui.

## Idioma

**Código em inglês**: nomes de tipo, método, variável, teste, log, exceção técnica (boot,
configuração) e dados de teste. **Texto que chega ao usuário em pt-BR**: mensagens em
`Domain/Errors`, títulos de erro dos controllers, validações, respostas 401/403/429/500 e o
e-mail de reset. Teste que confere uma mensagem ao usuário compara o texto em pt-BR — isso é
dado, não nome. Não há comentários no código.

## Stack

- ASP.NET Core Web API em `.NET 10`
- EF Core `9.0.x` com `Pomelo.EntityFrameworkCore.MySql` (o provider depende de EF Core 9;
  os pacotes 9.0.x rodam sem problema no `net10.0`)
- `FluentResults` para retorno de regra de negócio
- `FluentValidation` para validação de request
- `MySqlConnector` para a health check que abre conexão crua
- `JWT Bearer` HS256 — **esta API emite e valida**
- `BCrypt.Net-Next` (custo 12) para senha e `MailKit` para o e-mail de reset

## Banco: um só, versionado por migrations

Tudo vive no MySQL `meone`. Uma connection string: `ConnectionStrings:MeOne`.

**O schema é das migrations do EF Core** (`MeOne.Infrastructure/Migrations`). Mudou entidade ou
mapping, gera migration no mesmo PR:

```
dotnet ef migrations add <NomeDaMudanca> --project MeOne.Infrastructure --startup-project MeOne.API
```

- O `dotnet ef` monta o contexto pelo `MeOneDbContextFactory` (sem subir a API); sem
  `ConnectionStrings__MeOne` ele aponta para o MySQL do `compose.yaml`.
- `MeOneDbContext.ServerVersion` é fixo (8.0), sem AutoDetect: gerar migration não depende de
  banco no ar. A DI usa o mesmo valor.
- A API **não** chama `Migrate()` no boot. Em produção a migration sai como script idempotente
  (`dotnet ef migrations script --idempotent`) e roda no deploy.
- Dado obrigatório para o sistema funcionar entra por `HasData` no mapping (hoje: a role `master`,
  id 1, em `RoleConfiguration`). Dado de exemplo não entra em migration.
- `MigrationTests` falha se o modelo mudar sem migration (`HasPendingModelChanges`). Não apague nem
  edite migration já aplicada em algum ambiente — crie outra.

O mapping continua sendo o elo entre código e schema: cada coluna é nomeada à mão com
`HasColumnName` (tabelas snake_case), e `AccessMappingTests` confere tabela e coluna sem banco
(o provider monta o modelo e o SQL com `ToQueryString()` sem conectar). Tabela nova ganha teste
igual.

Não existe provider local (SQLite/InMemory) para a API, e isso é deliberado: um banco de
brinquedo esconde exatamente os erros de mapeamento que importam. Para desenvolver, use o MySQL
do `compose.yaml` (passo a passo no `README.md`, "Banco de dados local").

## Fluxo padrão

Há **um** fluxo:

```
Controller → I*UseCase (Application) → I*Repository (Domain) → *Repository (Infrastructure)
```

O controller é fino: chama, checa `IsFailed`, traduz com `ApiResults`. Regra de negócio no caso
de uso ou na entidade. Query no repository, nunca acima dele.

**Um caso de uso é uma operação.** Tem exatamente um `ExecuteAsync` e declara no construtor só
o que aquela operação usa. Não existe `Services/` nem classe `*Service` na Application: um
service de N métodos obriga o teste de um endpoint a montar N dublês.

## Domain

```
Entities/       Entity.cs (base), User, Role, UserRole, Department, ClientApplication,
                UserApplication, RefreshToken
Errors/         constantes de mensagem por agregado + DomainRuleException
Interfaces/     IRepository<T,TId> + contratos de repositório por agregado
Rules/          regras puras sem estado (AccessRoles, SessionPolicy, TemporaryPasswordPolicy)
```

`Errors/` guarda as duas formas que uma recusa de domínio assume: a **constante de mensagem**
(o caso normal, devolvida como `Result.Fail`) e a **`DomainRuleException`** (a rede, para quando
a entidade recusa uma transição que o caso de uso não checou antes). Não há pasta `Exceptions/`.

**Toda tabela persistida tem entidade.** Não declare `record` de linha de banco dentro do
arquivo da interface do repositório.

**Mensagem de negócio é constante em `Errors/`**, nunca literal no caso de uso — senão nenhum
teste consegue afirmar *qual* erro ocorreu.

A base é `Entity<TId>` porque as tabelas não compartilham tipo de chave (`users`/`roles` são
`bigint unsigned`, `departments` é `int`). `Entity` sem parâmetro é o atalho para `Guid`.

Entidade não expõe setter público: estado muda por método (`User.Update`, `User.Deactivate`,
`User.GrantRole`, `RefreshToken.Revoke`). `ClientApplication` não se chama `Application` para
não colidir com o namespace `MeOne.Application`.

Nenhum DTO mora no Domain, e o Domain não tem dependência de pacote.

## Infrastructure

```
Data/Context/   MeOneDbContext + MeOneDbContextFactory (design-time, para o dotnet ef)
Migrations/     migrations do EF + snapshot do modelo (gerados pelo dotnet ef)
Mappings/       *Configuration : IEntityTypeConfiguration<T>
Repositories/   DbContextRepository<T,TId,TContext> → Repository<T,TId> → *Repository
Identity/       BCryptPasswordHasher, RefreshTokenGenerator (portas de Application/Interfaces/Auth)
Email/          SmtpPasswordResetNotifier + EmailOptions; template e logo embutidos na DLL
HealthChecks/   MeOneDatabaseHealthCheck
```

Cada coluna é nomeada à mão com `HasColumnName`: as tabelas são snake_case e o EF, sozinho,
usaria o nome da propriedade em PascalCase.

Sem `DbSet` no contexto — o `Repository<T,TId>` genérico usa `Set<T>()`.

Repositório de agregado com entidade rastreada sobrescreve `Update` para só chamar `DbSet.Update`
quando a entidade está `Detached`: `Update` numa entidade já rastreada marca o grafo inteiro
(departamento, roles) como modificado.

## Application

```
Interfaces/                          contratos, agrupados por módulo
  Auth/                              I*UseCase de sessão + portas IPasswordHasher,
                                     IAccessTokenIssuer, IRefreshTokenGenerator
  Users/ Roles/ Departments/         I*UseCase do CRUD; Users tem a porta IPasswordResetNotifier
  ICurrentActor.cs                   transversal: não é de módulo nenhum
UseCases/<Módulo>/                   *UseCase + a classe estática compartilhada do módulo
Communication/
  <Módulo>/Requests/                 um record por arquivo
  <Módulo>/Responses/                um record por arquivo
  <Módulo>/Dtos/                     só quando o DTO não é request nem response
  Common/Responses/                  PagedResponse<T>
Errors/                              erros de Result que carregam semântica de transporte
Validators/<Módulo>/                 um validator por arquivo
```

**`Interfaces/` guarda contrato, `UseCases/` guarda implementação**, espelhando a mesma divisão
por módulo. Porta de infraestrutura mora no módulo que a usa, ao lado dos contratos de caso de
uso; só `ICurrentActor` fica na raiz, porque atravessa módulo. `IAccessTokenIssuer` é
implementado na **API** (`JwtAccessTokenIssuer`, registrado no `Program.cs`), ao lado da
validação do JWT: emissão e validação leem o mesmo `JwtOptions`.

**O que várias operações do mesmo módulo compartilham mora numa classe estática interna na pasta
do módulo em `UseCases/`** — `AuthMapping`, `UsersMapping`, `RolesMapping`, `DepartmentsMapping`.
Nenhuma termina em `UseCase`, então nenhuma entra no container.

**`Communication/` é o contrato de fio**, separado do caso de uso, porque responses são
compartilhados entre operações (`AuthResponse` sai do login e do refresh).

Integração externa entra por **porta** (interface em `Interfaces/<Módulo>/`, adaptador na
Infrastructure), nunca `HttpClient` direto no caso de uso. Port tem exceção/retorno próprio já
traduzido para a linguagem da aplicação.

A Application não referencia pacote de framework: só `FluentResults` e `FluentValidation`. Não
há `Extensions/` nem `Options/` aqui — registrar dependência e ler configuração são decisões de
composição, e moram na Infrastructure/API. Configuração que é regra de negócio vira política
imutável no Domain (`SessionPolicy`), montada e validada pela Infrastructure.

## DI: convenção, não lista

`AddApplicationUseCases()` e `AddInfrastructureRepositories()`, **as duas na Infrastructure**,
registram por nome: todo `*UseCase` com `I*UseCase` correspondente, e toda `*Repository` com
`I*Repository`, como scoped. As duas são a mesma varredura (`AddByConvention`) e resolvem o
assembly alvo por `typeof(X).Assembly` — referência que o compilador verifica. São chamadas
explicitamente no `Program.cs`, antes de `AddInfrastructure`, que só cuida do que precisa de
configuração (`DbContext`, opções, portas de identidade e e-mail, health checks).

**A convenção é o contrato.** Se a classe fugir do par nome/interface, o registro silenciosamente
não acontece e o erro só aparece na primeira requisição. `DependencyInjectionConventionTests`
resolve por reflexão **todo** `I*UseCase` e `I*Repository`, e cada varredura tem um guarda
anti-vazio (`Assert.NotEmpty`) — uma varredura que não encontra nada passa verde e não protege
nada. Se acrescentar um sufixo à convenção, acrescente-o à varredura.

## Validação

`AddValidatorsFromAssemblyContaining` registra os validators e o `FluentValidationFilter`
(`MeOne.API/Validation`, filtro global de MVC) roda o `IValidator<T>` de cada argumento antes da
action, respondendo 400 com o envelope padrão. Validator novo passa a valer sozinho, e o
controller **não** injeta `IValidator<T>` nem chama `ValidateAsync`. Validator precisa ser
`public`: a varredura ignora tipo interno.

Não volte ao `FluentValidation.AspNetCore`: está depreciado e prende a FluentValidation na 11.
`FluentValidation` e `FluentValidation.DependencyInjectionExtensions` andam na mesma versão
principal. Como o filtro valida com `ValidateAsync`, regra assíncrona (`MustAsync`) funciona.
`FluentValidationFilterTests` trava o comportamento.

Divisão: forma e comprimento no validator; regra que precisa de dado (unicidade, existência) no
caso de uso. Comprimento é medido no valor **já normalizado** (trim), que é o que vai para a
coluna, e segue a menor coluna da cadeia. Regras de campo repetidas entre requests ficam numa
classe interna de extensões no módulo (`UserFieldRules`, `RoleFieldRules`).

## Erro e retorno

`Result` da Application, traduzido em `MeOne.API/Errors`:

```json
{ "error": "rótulo curto", "details": "texto pronto para a tela" }
```

`ApiResults.Failure` decide o status pelo **tipo** do erro: **401** `CredentialRejectedError`,
**403** `ForbiddenError`, **404** `NotFoundError`, **502** `UpstreamGatewayError`, **503**
`UpstreamUnavailableError`, **400** para recusa de negócio (`Error` cru). Não troque esses tipos
por `Error` com a mensagem certa: o corpo não muda, mas todo status colapsa em 400 e o cliente
perde a informação de "confira a senha" vs. "revise o formulário" vs. "tente de novo".
`AuthErrorContractTests` e `ApiResultsTests` travam esses valores — se uma asserção de status
precisar mudar para passar, é regressão, não manutenção de teste.

Login: matrícula inexistente e senha errada devolvem o mesmo `CredentialRejectedError` com o
mesmo texto, e o hasher gasta o mesmo tempo nos dois casos (`IPasswordHasher.Verify` com hash
nulo). Refresh inválido é **400**, não 401, para o interceptor do front não renovar em laço.

O corpo de sucesso é o **DTO cru** (`Ok(result.Value)`), nunca o `Result<T>` — `Ok(result)`
serializa `{ isSuccess, value }` e o cliente lê os campos como `undefined`.

Não lance exceção para erro esperado de negócio.

## Testes

`MeOne.Tests`, xUnit, espelhando as camadas: `Application/`, `Infrastructure/`, `API/`, mais
`Fakes/`. Sem biblioteca de mock: dublês escritos à mão.

`InMemoryRepository<T,TId>` é o dublê padrão. `Insert` simula `AUTO_INCREMENT` (`int`, `long`,
`ulong`) e `SaveChangesAsync` devolve contagem real, porque os casos de uso tratam `<= 0` como
"nada foi persistido". `AccessFixture` monta usuários, roles, departamentos e tokens já ligados.
Os fakes de repositório imitam o filtro de soft delete do usuário.

Não há teste de integração contra MySQL no projeto; mapping é coberto por `AccessMappingTests`.

## Acesso

Não há política de fallback: endpoint sem atributo é anônimo. Toda action declara o acesso.

- Anônimo: só a tela de login (login, refresh, valid, esqueci a senha, autocadastro, consulta
  por matrícula, lista de departamentos), com limite por IP (`AuthPolicies.AnonymousRateLimit`).
- Token (`[Authorize]`): leitura e logout.
- `master` (`[Authorize(Policy = AuthPolicies.MasterOnly)]`): toda escrita de usuário, role e
  departamento. Endpoint novo de governança entra assim, não com `[Authorize]` cru.

`AccessEndpointAuthorizationTests` trava esse mapa. O claim `roles` do token vira
`ClaimTypes.Role` em `RolesClaimsTransformation`; é isso que `RequireRole` lê.

O token é emitido em `JwtAccessTokenIssuer` com os claims `sub`, `registration`, `department`,
`roles`, `applications` e `aud` = `Jwt:Audiences`. Outros sistemas que validem com a mesma chave
dependem desses nomes e do formato `string[]` de `GET /api/v1/departments` — não renomeie sem
combinar.

Usuário tem soft delete (`HasQueryFilter(Active)` em `UserConfiguration`). Toda consulta de
usuário esconde o desativado, **exceto** as checagens de unicidade (`RegistrationExistsAsync`,
`EmailExistsAsync`) e "departamento em uso", que usam `IgnoreQueryFilters()` porque o índice
único e a FK do banco continuam contando a linha.
