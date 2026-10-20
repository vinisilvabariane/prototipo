# MeOne Backend — Guia de Trabalho

## Escopo

Estas instruções valem para `meone/backend`. Quatro projetos de código e um de teste:

- `MeOne.Api` (pasta `MeOne.API`) — controllers, autenticação (emissão e validação de JWT), Swagger, tradução `Result` → HTTP
- `MeOne.Application` — casos de uso, contratos, DTOs de fio, validators
- `MeOne.Domain` — entidades, regras, errors, contratos de repositório
- `MeOne.Infrastructure` — `DbContext`, mappings EF, repositories, identidade, e-mail, health checks
- `MeOne.Tests` — xUnit, espelhando as camadas

O alvo é **Clean Architecture**. O escopo funcional é o controle de acesso (autenticação e CRUD de
usuários, roles, departamentos e escopos), o módulo **Workflow** (fluxo de aprovação), o
**Organograma** (cargos em árvore), as **Instruções de trabalho** (documentos de processo com PDF), os
**Fornecedores** (cadastro e qualificação), as **Linhas e postos** (layout da linha, alocação de pessoas e produtividade por posto) e o **OEE** (eficiência por linha e posto, com fórmulas configuráveis) e o **Help desk** (setores com formulário montável e chamados). Módulo novo segue a mesma estrutura descrita aqui.

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
Entities/       Entity.cs (base), User, Role, RolePermission, Permission, UserRole, Scope,
                Department, ClientApplication, UserApplication, RefreshToken, PasswordResetToken,
                Workflow (raiz) + WorkflowAssignee, WorkflowStep, WorkflowAttachment, WorkflowMessage,
                OrgPosition, WorkInstruction + WorkInstructionStep,
                Supplier (raiz) + SupplierTool, SupplierMaterial, SupplierSelfAssessment, SupplierAudit, SupplierDocument,
                ProductionLine, ProductionShift, ProductionProduct, LineLayout (raiz) + LayoutPost, LayoutDecoration,
                ProductionRun (raiz) + RunAllocation, RunOutput, RunSnapshot
Enums/          ScopeType, WorkflowStatus/Priority/Category/Action, ReviewStage/ReviewRole,
                WorkflowListFilter, WorkflowRefusalKind
Errors/         constantes de mensagem por agregado + DomainRuleException
Interfaces/     IRepository<T,TId> + contratos de repositório por agregado
Rules/          regras e catálogos sem estado (Permissions, RoleCatalog, AccessProfile,
                ScopeHierarchy, SessionPolicy, LoginLockoutPolicy, PasswordResetPolicy,
                TemporaryPasswordPolicy, AccessRoles, WorkflowRefusal, WorkflowHistoryTexts,
                WorkflowOutcomeTexts, OrgChartHierarchy, FileSignatures,
                WorkInstructionCodes, QualificationRules, SupplierQualification, SupplierDocumentRules, Cnpj, LayoutRules, RunProductivity)
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
Storage/        LocalFileStorage (base) + LocalWorkflowFileStorage / LocalWorkInstructionFileStorage /
                LocalSupplierFileStorage e as opções de cada uma (portas IWorkflowFileStorage,
                IWorkInstructionFileStorage e ISupplierFileStorage)
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
  Users/ Roles/ Departments/ Scopes/ Workflows/ OrgChart/ WorkInstructions/ Suppliers/ Production/ I*UseCase do CRUD; Users tem a porta IPasswordResetNotifier,
                                     Auth tem IAccessProfileCache e ILoadAccessProfileUseCase
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
do módulo em `UseCases/`** — `AuthMapping`, `AccessGuard`, `UsersMapping`, `RolesMapping`,
`DepartmentsMapping`, `ScopesMapping`.
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

Autorização é **permissão**, nunca nome de papel. Papel funcional agrupa permissões; o usuário
recebe papel sem escopo (vale em tudo) ou num nó Planta > Área > Linha > Posto, e o nó vale para
os filhos. Catálogo de permissões em `Domain/Rules/Permissions.cs`; papéis do sistema em
`Domain/Rules/RoleCatalog.cs` (ambos semeados por migration). Permissão nova: acrescente no
catálogo e gere migration.

Não há política de fallback: endpoint sem atributo é anônimo. Toda action declara o acesso:

- `[AllowAnonymous]`: só a tela de login (login, refresh, esqueci a senha, autocadastro, lista
  de departamentos), com limite por IP (`AuthPolicies.AnonymousRateLimit`).
- `[Authorize]`: o que só exige estar logado (logout, `auth/valid`, `auth/me`, leitura de escopo).
- `[RequirePermission(Permissions.X)]`: o resto. Endpoint novo entra assim. A policy é montada
  por `PermissionPolicyProvider`; `PermissionAuthorizationHandler` consulta o perfil de acesso.
  Permissão `GlobalOnly` só conta se concedida sem escopo; as operacionais passam no endpoint se
  concedidas em qualquer escopo — **o caso de uso confere o escopo do recurso** com
  `AccessProfile.HasIn(permission, scopeChain)` (`IScopeRepository.GetChainAsync`).

`AccessEndpointAuthorizationTests` trava o mapa action → permissão.

**Perfil de acesso** (`Domain/Rules/AccessProfile`): montado do usuário com papéis, permissões e
escopo (`IUserRepository.GetWithAccess*`). Na API fica em `AccessProfileCache` (memória, 30 s),
que implementa a porta `IAccessProfileCache`. Caso de uso que muda papel, permissão, senha,
desativação ou sessão chama `Invalidate(userId)` (ou `InvalidateAll()` quando muda as
permissões de um papel). `ICurrentActor.GetAccessProfileAsync` entrega o perfil do ator.

**Regras contra escalonamento** ficam nos casos de uso, via `UseCases/Auth/AccessGuard` e
`RolesMapping.EnsureCanHandle`: ninguém concede/retira/inclui permissão que não tem no escopo;
ninguém altera usuário que tenha permissão que o ator não tem (`AccessProfile.Dominates`); o
último administrador ativo não sai; papel com permissão administrativa só sem escopo. Mexeu em
caso de uso que altera usuário ou papel, mantenha essas checagens e o teste correspondente.

**Sessão:** o JWT leva `sub`, `registration`, `name`, `email`, `department`, `roles` (só nomes,
para Q-Zero/master-data), `applications`, `aud` = `Jwt:Audiences` e `sstamp`. Em
`OnTokenValidated` a API confere usuário ativo e `sstamp` igual ao do banco: rotacionar o stamp
(`User.RotateSecurityStamp`, chamado em troca de senha, desativação, logout e reuso de refresh)
derruba todos os access tokens do usuário. Refresh é rotacionado e o reuso de um já usado revoga
tudo. Não renomeie claim nem troque o formato de `GET /api/v1/departments` (`string[]`) sem
combinar com os outros sistemas.

**Não vazar informação:** login, refresh, confirmação de reset e pedido de reset respondem igual
para usuário inexistente e para credencial errada; pedido de reset sempre 202. Erro de binding
responde mensagem genérica. Não devolva detalhe de exceção, hash, código de reset ou senha em
resposta (a única exceção é a senha provisória, para quem tem `users.reset-password`).

Usuário tem soft delete (`HasQueryFilter(Active)` em `UserConfiguration`). Toda consulta de
usuário esconde o desativado, **exceto** as checagens de unicidade (`RegistrationExistsAsync`,
`EmailExistsAsync`) e "departamento em uso", que usam `IgnoreQueryFilters()` porque o índice
único e a FK do banco continuam contando a linha. A exceção documentada a essa regra é
`GetByIdsIncludingInactiveAsync`: o histórico do Workflow precisa mostrar o nome de quem já foi
desativado.

## Workflow

Fluxo de aprovação portado do SPM (engenharia). O agregado é `Workflow`; ele guarda
responsáveis, etapas (histórico), anexos e mensagens, e **toda regra mora nele**:
`Check*` devolve `WorkflowRefusal?` (`Invalid` → 400, `Forbidden` → 403, `NotFound` → 404) e a
mutação correspondente lança `DomainRuleException` se alguém pular o `Check`. O caso de uso chama o
`Check`, traduz a recusa (`WorkflowsMapping.Fail`) e só então muta.

- Quem cria é responsável e aprova automaticamente. Se for o único, o workflow nasce aprovado.
- Aprovado quando **todos os responsáveis atuais** têm uma aprovação; aí entra a etapa
  `FinalApproval`.
- Responsável rejeita → etapa `Rejection` em `ReviewStage.CreatorReview`. O criador **devolve**
  (vai para `AssigneeReview`, com mensagem no chat da etapa) ou **aceita a rejeição** (workflow
  `Rejected`, etapa `FinalRejection`). Quem rejeitou pode aprovar ou devolver de volta. A
  justificativa original da rejeição nunca é sobrescrita; a conversa fica em `WorkflowMessage`.
- Só o criador edita, adiciona e substitui responsáveis. Substituir é recusado se o responsável já
  aprovou, e limpa a revisão pendente dele.
- Workflow encerrado não aceita nenhuma ação.
- Concorrência: `Workflow.Version` é token de concorrência e sobe a cada mutação.
  `WorkflowRepository.SaveChangesAsync` devolve `0` no conflito (o caso de uso responde
  `NothingPersisted`) — sem isso duas últimas aprovações simultâneas deixariam o workflow parado.
- Anexos: até 5 por workflow, 10 MB cada, só na criação. Vão para `IWorkflowFileStorage`
  (disco, `Workflow:Storage:Path`, padrão `uploads/workflows`) com nome gerado; o download passa
  pela API autenticada e sai como anexo. Falhou o banco, o caso de uso apaga os arquivos.
- Permissão única `workflow.use` (não global) e papel de sistema `workflow` (id 9, anexado ao fim do
  `RoleCatalog` para não deslocar os ids de `role_permissions` já semeados).
- **Fora do escopo por ora:** e-mails de prazo próximo/vencido (o `cron` do SPM).
`WorkflowEndpointTests` sobe Kestrel real com fakes e trava binding multipart, JSON e status.

## Organograma

Árvore de **cargos** (`OrgPosition`): cada cargo tem título, departamento, cargo superior opcional e
ocupante opcional (cargo vago é válido). Pode haver mais de uma raiz.

- Um colaborador ocupa no máximo **um** cargo (índice único em `fk_holder`); o ocupante precisa estar ativo.
  Desativar o usuário não desocupa o cargo: o organograma continua mostrando quem ocupa, marcado como inativo.
- Mover um cargo é recusado se o destino for ele mesmo ou um subordinado (`OrgChartHierarchy.IsSelfOrBelow`)
  ou se a árvore passar de `OrgChartHierarchy.MaxDepth` (12) níveis. Excluir só é possível sem subordinados.
- Leitura (`GET api/v1/org-chart`) é `[Authorize]`, para qualquer pessoa logada. Escrever exige
  `orgchart.manage` (global, papel de sistema `organograma`, id 10). A resposta traz `canManage` para a tela.

## Instruções de trabalho

Documento de processo portado do SPM (`WorkInstruction`): código único, título, nº do documento, produto,
elaborado/aprovado por, EPIs, especificações, observações, **PDF obrigatório** e passos opcionais (texto + imagem).

- O PDF pode vir pronto (upload) ou ser gerado pelo frontend a partir dos passos; para o servidor é sempre um arquivo.
  O conteúdo é conferido pela assinatura (`FileSignatures`), não pelo MIME declarado: PDF até 10 MB, imagem de passo
  (PNG, JPEG, GIF ou WebP) até 5 MB, no máximo 50 passos.
- Criar e editar usam o mesmo corpo multipart (`SaveWorkInstructionRequest` + arquivo `pdf` + `stepImages[i]`). Na edição,
  cada passo com `id` é mantido e editado; sem `id` é novo; passo que não vem é removido. `removeImage` limpa a imagem.
  Arquivo substituído ou removido só é apagado do disco depois do commit; falhou o banco, apaga o que acabou de gravar.
- Leitura (lista, detalhe, PDF inline, imagem do passo) é `[Authorize]` para qualquer pessoa logada, porque o chão de
  fábrica consulta. Escrita e geração de código exigem `engineering.manage` (reaproveitada; a descrição foi ampliada).
- `GET next-code` sugere o código `IT-yyyyMMdd-XXXXX` do original; a unicidade é checada na criação e na edição.
- Arquivos em `WorkInstruction:Storage:Path` (padrão `uploads/work-instructions`).

## Fornecedores

Cadastro e qualificação de fornecedores portados do SPM-Qualidade (`Supplier`): código SAP único por filial Multi,
nome fantasia, origem (nacional ou internacional), CNPJ (validado pelos dígitos quando nacional), criticidade, endereço e
contato. O agregado guarda ferramentais, materiais, auto avaliações, auditorias e documentos.

- **Qualificação vigente** é derivada, não armazenada: vale a auditoria in loco mais recente; sem auditoria, a auto avaliação
  mais recente (com `AuditPending` quando ela pede auditoria). Vencimento é calculado: auto avaliação = data + 1 ano + 1 dia;
  auditoria = data + 3, 6 ou 12 meses. Vencido nunca é "aprovado" nos filtros de status (`SupplierQualification.Matches`).
- Ferramental, material, auto avaliação e auditoria são sub-recursos REST (`POST/PUT/DELETE api/v1/suppliers/{id}/tools|materials|self-assessments|audits`).
  Remover é lógico (`Active = false`), como o `ativo=0` do original. O responsável pela validação precisa ser usuário ativo.
- **Documentos** são uma entidade só (`SupplierDocument`) com `Kind` e `OwnerId` opcional: apresentação, certificados (do fornecedor);
  desenho de ferramental, documento de material, anexo da auto avaliação, declaração e relatório de auditoria (de um item, `OwnerId` obrigatório).
  Apresentação, anexo, declaração e relatório substituem o anterior do mesmo dono. Só PDF, JPG e PNG, conferidos pela assinatura, até 10 MB.
  Arquivo substituído ou removido só é apagado do disco depois do commit.
- **Quem edita:** quem cadastrou o fornecedor ou quem tem `suppliers.manage`. Toda escrita exige `suppliers.register`; o dono é conferido no caso de uso
  (403 para os demais). Leitura (lista, detalhe, arquivo) é `[Authorize]`. Papéis de sistema: `fornecedores` (id 11, registra) e
  `gestor-fornecedores` (id 12, registra e gerencia). Ambas as permissões são não globais.
- Lista filtra por busca, origem, filial, criticidade e status (inclusive `notEvaluated` e `expired`); o status é calculado em memória sobre
  as avaliações carregadas, então a paginação acontece depois do filtro.
- Arquivos em `Supplier:Storage:Path` (padrão `uploads/suppliers`).
- Fora do port: e-mail no cadastro, gráficos IQC/TAF (dependem de outros módulos) e consulta ao catálogo de SKUs
  (SKU e BU são texto livre).

## Linhas e postos (layout, alocação e produtividade)

Portado do módulo Matriz de capacitação do Sync, trocando o foco: em vez de treinar pessoas, o mapa serve para
**alocar pessoas nos postos, iniciar a produção pelo sistema e medir a produtividade de cada posto**.
Treinamentos, ranks, scores e relatório de experiência ficaram de fora.

- **Cadastros próprios** (`ProductionLine`, `ProductionShift`, `ProductionProduct`): o Sync traz linhas, produtos e turnos de SAP e Bifrost; aqui são telas simples.
  Pessoas são os usuários do MeOne. Desativar é o jeito de "remover": layouts e produções antigas continuam apontando para o cadastro.
- **Layout** (`LineLayout`) é o mapa de uma combinação linha + produto + turno (única). Os postos (`LayoutPost`: comum, máquina ou qualidade,
  crítico, meta por hora, operação) e as decorações (esteira, área, seta, equipamento, atenção, estante, bancada, pilar) são linhas
  normalizadas, não JSON como no Sync. Salvar usa o mesmo corpo do editor: posto com `id > 0` é atualizado, `id <= 0` é novo e o que
  não vem é removido (`LineLayout.Replace`). Nome de posto é único no layout, sem diferenciar maiúsculas. Copiar para outros destinos
  sobrescreve o conteúdo do destino que já existir.
- **Produção** (`ProductionRun`) nasce de um layout em `Preparing` com um **snapshot** do mapa (JSON em `layout_snapshot`). Editar o layout depois não
  muda uma produção em curso nem o histórico. Fluxo: preparar → alocar pessoas → iniciar (`Running`) → lançar produção → encerrar (`Closed`).
  Só uma produção ativa por linha. Cancelar só vale em preparo.
- **Alocação**: posto comum é exclusivo e a pessoa não acumula outro posto; **máquina** deixa a mesma pessoa em até 5 (constante
  `ProductionMapping.MaxMachinePostsPerPerson`); misturar máquina e comum é recusado; quem está em outra produção ativa não é alocado. Trocar o ocupante
  fecha a alocação anterior (o histórico de `RunAllocation` fica, com `ReleasedAt`). Encerrar libera todos. Iniciar exige alguém alocado e todos os
  postos críticos ocupados. "Repetir alocação anterior" casa os postos pelo nome com a última produção encerrada do mesmo layout.
- **Produtividade** (`RunProductivity`): lançamento manual por posto e hora (`RunOutput`, único por run+posto+hora, o mesmo lançamento substitui o anterior), creditado a
  quem ocupava o posto naquela hora. Esperado = meta por hora × tempo ocupado dentro da produção; atingimento = produzido ÷ esperado.
  Tempo por pessoa é a união dos intervalos, então quem cuida de duas máquinas ao mesmo tempo não conta em dobro.
- **Permissões** (todas já existentes, não globais): `engineering.manage` mantém cadastros e layouts; `production.supervise` prepara, aloca, inicia, encerra e
  cancela; `production.execute` lança produção. Leitura é `[Authorize]`. As respostas trazem `canManage`, `canSupervise` e `canRecord` para a tela.
- Fora do port: treinamentos, ranks e scores, alocação rápida por QR Code, integração com SAP, MQTT e catraca (a quantidade é lançada à mão), copiar
  alocação de um dia qualquer e a edição de um layout compartilhado por vários produtos ao mesmo tempo.

## OEE (eficiência por linha e posto)

Portado do módulo OEE do Bifrost, calculado em cima das produções de **Linhas e postos** (`ProductionRun`) em `api/v1/oee`.
O posto do mapa faz o papel da máquina do Bifrost e a produção (run) faz o papel do turno da linha.

- **Cálculo** (`OeeCalculator`, Domain, puro e sem I/O): por posto, disponibilidade × desempenho × qualidade; por linha, cada pilar é uma agregação
  dos postos **medidos** (os que têm meta por hora). Padrões: disponibilidade = `(elapsed_seconds - downtime_availability) / elapsed_seconds`,
  desempenho = `real_production / theoretical_production`, qualidade = `(real_production - defects) / real_production` (1 se não produziu), OEE do posto = A × P × Q,
  linha: disponibilidade `availability_average`, desempenho `performance_last_post`, qualidade `quality_product`, OEE = A × P × Q. Pilares nunca ficam abaixo de 0 e saem em % com 2 casas.
  "Último posto" é o medido de maior `X` do mapa. Há detalhe hora a hora da linha e de cada posto.
- **Variáveis** (`OeeCatalog`): produzido (lançamentos), teórica (meta/h × tempo decorrido), esperada (meta/h × tempo com gente), tempo decorrido/ocupado, paradas, defeitos e as
  agregações de cada pilar (`<pilar>_average|sum|product|last_post`). Cada variável declara em quais campos de fórmula pode aparecer (`OeeScope`).
- **Fórmulas** (`OeeFormula`): expressão de texto, sem dependência externa, com `+ - * /`, menos unário, parênteses, comparações (`< > <= >= == !=`) e as funções
  `min`, `max`, `abs`, `clamp`, `if`, `round`. Divisão por zero e resultado não finito viram 0. Variável desconhecida vale 0; o validador recusa a que não pertence ao campo.
  Parse devolve mensagem em pt-BR e a posição do erro. Fórmula salva que deixa de compilar cai no padrão do campo em vez de derrubar a leitura.
- **Conjuntos de fórmulas** (`OeeFormulaSet`, tabela `oee_formula_sets`): 8 campos (3 pilares + OEE do posto, 3 pilares + OEE da linha). No máximo um ativo; nenhum ativo = padrão do sistema.
  O ativo não é excluído. Gerenciar exige `engineering.manage`; consultar e testar é `[Authorize]`. `POST formulas/test` avalia uma expressão com valores informados ou com os números reais de uma produção.
- **Paradas e defeitos**: o Bifrost lê chamados e paradas do MES. Aqui `IOeeLossSource` é a porta e o **help desk** a implementa (`HelpdeskOeeLossSource`): chamado com linha parada vira perda de disponibilidade e defeito informado vira perda de qualidade. `OeeLosses` carrega paradas (por posto ou da linha toda) e defeitos; defeito sem posto conta no último posto medido. Sem a fonte, `lossSourceAvailable` vem falso e a tela avisa.
- **Telas**: OEE da produção (equação, cascata de perdas, hora a hora, mapa de calor posto × hora e tabela por posto), comparativo por período (`GET runs`, até 92 dias e 60 produções),
  tempo real (mesmo endpoint filtrado por `running`, a tela atualiza sozinha) e exportação em CSV feita no frontend. Fora do port: MQTT, intervalos de turno e tabela de predefinições do Bifrost.

## Help desk (setores, formulários e chamados)

Portado do sync-tickets (departamentos com formulário server-driven), com `api/v1/helpdesk`. "Departamento" do sync vira **setor** aqui, para não colidir com o `Department` organizacional.

- **Setor** (`HelpdeskSector`): identidade (nome único, cor #RRGGBB, ícone de uma lista fixa `HelpdeskIcons`, até 5 etiquetas, ativo), formulário de abertura, formulário de resposta do técnico (opcional), até 3 campos de resumo do card e os **papéis que atendem** (`AttendantRoleIds`). Setor com `ExternalUrl` só redireciona: zera formulários, resumo e papéis. Gerenciar (setores e modelos de campo) exige `helpdesk.manage` (perfil `gestor-helpdesk`); listar e abrir chamado é `[Authorize]`.
- **Formulário** (`HelpdeskForm`, JSON em `form_schema`): lista **plana** de campos que formam uma árvore por `DependsOn` (campo pai de lista/cards + valores). `HelpdeskFormRules.Check` garante ids únicos `[a-z][a-z0-9_]*`, opções únicas, pai antes do filho, valores existentes e no máximo 60 campos. Tipos: texto, texto longo, número, lista, cards e três **especiais**, no máximo um de cada e só no formulário de abertura: `ProductionSpot` (linha + posto), `LineStop` (parada e desde quando) e `Defects` (peças com defeito); parada e defeitos exigem a localização.
- **Respostas** (`HelpdeskAnswers.Normalize`): chaves `id`, `id_post` e `id_time`. Só vale o ramo visível: campo escondido por condição não é exigido nem guardado. Parada sem horário usa agora; horário no futuro (>5 min) ou com mais de 7 dias é recusado.
- **Chamado** (`HelpdeskTicket`): `Open → InProgress → Resolved`, ou `Open → Rejected`. Guarda **snapshots** do formulário de abertura e do de resposta, então editar o setor depois não muda o histórico. Assumir e recusar exigem atender o setor e o chamado estar aberto; só quem assumiu finaliza; recusar pede motivo. O formulário de resposta do setor, se existir, é obrigatório para finalizar. Quem abre vê o próprio chamado; quem atende vê os do setor; administrador vê tudo. Atender = administrador ou ter um dos `AttendantRoleIds`.
- **Localização**: `GET locations` devolve as linhas ativas com os postos da produção em andamento (snapshot do mapa). O posto escolhido precisa pertencer a essa produção.
- **Modelos de campo** (`HelpdeskFieldTemplate`): presets (texto, texto longo, número, lista, cards) copiados para o formulário; mudar o modelo não altera formulários prontos.
- **Ligação com o OEE**: `HelpdeskOeeLossSource` implementa `IOeeLossSource` com os chamados da linha. Chamado com linha parada (não recusado) vira parada de **disponibilidade** do `StoppedAt` até finalizar (ou até agora); sem posto, vale para todos os postos. Defeitos entram na qualidade do posto escolhido; sem posto, no último posto medido do fluxo. `lineId = 0` só informa que a fonte existe.
- Fora do port por enquanto: checklists com gatilho, analytics de downtime, tempo real por SignalR, quiosque/Andon e aviso no Google Chat.
