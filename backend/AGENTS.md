# Iris Web Backend — Guia de Trabalho

## Escopo

Estas instruções valem para `iris-web/backend`. Quatro projetos:

- `IrisWeb.Api` (pasta `IrisWeb.API`) — controllers, autenticação, Swagger, tradução `Result` → HTTP
- `IrisWeb.Application` — casos de uso, contratos, DTOs de fio, validators, opções
- `IrisWeb.Domain` — entidades, enums, regras, errors, contratos de repositório
- `IrisWeb.Infrastructure` — `DbContext`, mappings EF, repositories, integrações, health checks

O alvo declarado é **Clean Architecture**. O backend do **Q-Zero** (`C:\Repositories\sync\backend`,
projetos `QZero.*`) segue sendo a referência para tudo que este arquivo não responder — mas a
estrutura da Application se afastou dele de propósito, e as divergências estão marcadas abaixo
como **Diverge do Q-Zero**.

## Stack

- ASP.NET Core Web API em `.NET 10`
- EF Core `9.0.x` com `Pomelo.EntityFrameworkCore.MySql` (o provider depende de EF Core 9;
  os pacotes 9.0.x rodam sem problema no `net10.0`)
- `FluentResults` para retorno de regra de negócio
- `FluentValidation` para validação de request
- `MySqlConnector` para as health checks que precisam abrir conexão crua
- `JWT Bearer` HS256 — **esta API emite e valida** (o Access Control foi absorvido)
- `BCrypt.Net-Next` (custo 12) para senha e `MailKit` para o e-mail de reset

Não há pacote de storage. Os artefatos de modelo ficam numa pasta em disco (`ModelStorage:Path`),
servida pela própria API — ver "O storage de artefatos" abaixo.

## Banco: um só, e o schema não é nosso

Tudo vive no MySQL `iris` — catálogo de modelos, embeddings faciais, estações, log de
presença e controle de acesso (usuários, roles, departamentos, refresh tokens). Uma connection
string: `ConnectionStrings:Iris`.

**Não há migrations, e não deve haver.** As tabelas `face_detector_*` são de produção e
nasceram no app desktop (`iris-desktop`), que continua lendo e escrevendo nelas. Uma
migration proporia `ALTER`/`DROP` na primeira divergência entre o modelo e o banco real.
Não há DDL versionado aqui para o catálogo: as tabelas foram criadas à mão e o script que as
descrevia foi removido. A exceção são as tabelas do controle de acesso, vindas do banco
`access_control`, cujo DDL está no `README.md` ("Controle de acesso → Schema"). Para recriar, extraia do banco com `SHOW CREATE TABLE`. O DDL das
tabelas do desktop vive nos docstrings dele.

Consequência prática: **o mapping é o único elo entre código e schema**. Errar um nome de
coluna não quebra a compilação — quebra na primeira query. Depois de mexer num mapping, suba
a API e chame o endpoint que lê aquela tabela.

Não existe provider local (SQLite/InMemory) e isso é deliberado. Havia um SQLite de
desenvolvimento para o catálogo; ele ficava vazio, o cliente nunca achava versão aprovada e
caía no artefato embarcado, então o caminho de distribuição nunca era exercitado. O sintoma
aparecia como "o badge diz embarcado", não como "o catálogo está vazio".

## Fluxo padrão

Há **um** fluxo, e não dois:

```
Controller → I*UseCase (Application) → I*Repository (Domain) → *Repository (Infrastructure)
```

O controller é fino: chama, checa `IsFailed`, traduz com `ApiResults`. Regra de negócio no caso
de uso ou na entidade. Query no repository, nunca acima dele.

**Um caso de uso é uma operação.** Tem exatamente um `ExecuteAsync` e declara no construtor só
o que aquela operação usa — `ClosePresenceUseCase` recebe o repositório de presença e mais
nada. Não existe mais a alternativa "reunir as operações do agregado num service".

O que fez o padrão virar único foi custo de dublê. O catálogo de modelos era um service de nove
métodos, e **testar um único endpoint obrigava a montar nove stubs** — repositório, storage e
ator, todos, para exercitar um caminho que usava dois. O resultado não foi teste ruim, foi teste
nenhum: o catálogo ficou sem cobertura enquanto era um service. Depois de virar nove casos de
uso, o teste do download monta o repositório e o storage e mais nada. Estação, reconhecimento e
autenticação seguiram o mesmo caminho; hoje são **vinte e dois** casos de uso, e cada teste monta só o
que o caminho exige.

**Diverge do Q-Zero:** lá todo agregado é um `*Service` e não existem casos de uso. A
divergência é deliberada e autorizada pelo dono: na Clean Architecture a unidade da camada de
aplicação é o caso de uso, e o gatilho concreto foi o custo de dublê descrito acima. Não é mais
uma divergência localizada — `Services/` não existe na Application do Iris.

## Domain

```
Entities/       Entity.cs (base), AiModel, ModelVersion, DeviceMatrixBinding, StationPresence, StationVacancy, FaceEmbedding
Enums/
Errors/         constantes de mensagem por agregado + DomainRuleException
Interfaces/     IRepository<T,TId> + contratos de repositorio por agregado
StationNaming.cs  regra pura sem estado, na raiz — arquivo unico nao ganha pasta
```

`Errors/` guarda as duas formas que uma recusa de domínio assume: a **constante de mensagem**
(o caso normal, devolvida como `Result.Fail`) e a **`DomainRuleException`** (a rede, para quando
a entidade recusa uma transição que o caso de uso não checou antes). As duas descrevem o mesmo
assunto — "o domínio disse não" — e ficavam em pastas diferentes só por causa do sufixo do tipo.
Não há pasta `Exceptions/`.

**Toda tabela persistida tem entidade.** Não declare `record` de linha de banco dentro do
arquivo da interface do repositório — foi assim que estação, presença e embedding ficaram sem
modelo por meses, e o contrato de persistência acabou definindo o domínio em vez do contrário.

**Mensagem de negócio é constante em `Errors/`**, nunca literal no caso de uso. Enquanto era
literal, a mesma recusa aparecia escrita de dois jeitos e nenhum teste podia afirmar *qual*
erro ocorreu — só que a string batia.

**Diverge do Q-Zero:** o `Entity` de lá tem `int Id` e pronto. Aqui a base é
`Entity<TId>` porque as tabelas do Iris não compartilham um tipo de chave e nenhuma delas é
nossa para mudar: catálogo usa `Guid` (o id sai na URL e o front guarda no cache de
artefato), presença e embeddings usam `bigint auto_increment`, e o vínculo de aparelho com a
matriz é chaveado pelo id do aparelho. `Entity` (sem parâmetro) é o atalho para `Guid`, que é
o caso padrão de coisa criada por nós.

**Diverge do Q-Zero:** lá existem `Domain/Events`, `Domain/Services` e `Domain/Rules`. Aqui
nenhum dos três. Não há evento de domínio porque nada no Iris reage assincronamente a mudança de
estado — o desktop lê as tabelas direto. E a única regra pura sem estado que apareceu até hoje,
`StationNaming`, mora na **raiz** do Domain, como arquivo: pasta para um arquivo é ruído, e foi
esse mesmo critério que matou `Application/Options`. Se algum dia houver três ou quatro dessas,
agrupar passa a valer; com uma, não.

**Diverge do Q-Zero:** lá os contratos de repositório ficam em `Domain/Repositories`. Aqui é
`Domain/Interfaces`, para casar com `Application/Interfaces` — as duas camadas passam a nomear
contrato do mesmo jeito, em vez de uma dizer "repositories" e a outra "interfaces" para a mesma
coisa.

**Diverge do Q-Zero:** lá existe `Domain/DTOs`. Aqui nenhum DTO mora no Domain — todo contrato
de fio está em `Application/Communication`, e o Domain não sabe que existe uma API.

O Domain tem **uma** dependência: `System.Numerics.Tensors`, para a distância entre vetores
faciais. É matemática, não infraestrutura. Nada além disso entra aqui.

## Infrastructure

```
Data/Context/   IrisWebDbContext
Mappings/       *Configuration : IEntityTypeConfiguration<T>
Repositories/   DbContextRepository<T,TId,TContext> → Repository<T,TId> → *Repository
Identity/       BCryptPasswordHasher, RefreshTokenGenerator (portas de Application/Interfaces/Auth)
Email/          SmtpPasswordResetNotifier + EmailOptions; template e logo embutidos na DLL
Sync/           SyncMatrixClient (Matriz de Capacitação e base de colaboradores)
Storage/        FileSystemModelArtifactStorage + ModelStorageOptions
HealthChecks/
```

Cada coluna é nomeada à mão com `HasColumnName`: as tabelas são snake_case e o EF, sozinho,
usaria o nome da propriedade em PascalCase.

Sem `DbSet` no contexto por padrão — o `Repository<T,TId>` genérico usa `Set<T>()`. Só exponha
um quando houver necessidade concreta.

**Diverge do Q-Zero:** lá existe `GetAllPaginatedAsync` no repositório genérico. Aqui não —
nenhum endpoint pagina, e abstração sem chamador envelhece errado. Quando aparecer a primeira
tela paginada, ela entra no `IRepository`.

### O storage de artefatos

`FileSystemModelArtifactStorage` é o único adaptador de `IModelArtifactStorage`, e grava o
`model-package.zip` numa pasta (`ModelStorage:Path`, padrão `Data/model-artifacts`). Antes era o
MinIO, e a razão de ele ter saído é a mesma que tirou o `iris-core` daqui: **uma aplicação para
subir, não duas**. O MinIO existia para guardar um zip por versão de modelo, e cobrava por isso um
serviço a mais para provisionar, monitorar e credenciar.

Três coisas nesse arquivo não são detalhe:

1. **Tudo em `Stream`, dos dois lados.** O `facenet.onnx` tem 90 MB dentro do pacote. Um
   `ReadAllBytes` em qualquer ponto da cadeia — porta, caso de uso, controller — materializa o
   artefato inteiro na Large Object Heap, uma vez por upload e uma por download.
2. **A `objectKey` é entrada hostil, mesmo vindo do banco.** Ela é
   `{model.Key}/{version}/model-package.zip`, e `model.Key` é texto que alguém digitou no
   `POST /api/v1/models`. No MinIO uma chave torta era só um nome de objeto torto; aqui ela vira
   caminho de arquivo, e `Path.Combine(raiz, "../../appsettings.json")` devolve alegremente um
   caminho fora da raiz. `ResolveWithinRoot` recusa `..`, caminho absoluto, raiz alternativa e
   separador invertido, e ainda confere o caminho **já resolvido** contra a raiz. Recusa com
   `ArgumentException`, não com `ModelArtifactStorageException`: esta última vira 503, e 503 diz
   ao cliente "tente de novo" — chave torta não melhora com retentativa.
3. **A gravação é atômica**: arquivo `.part` e `File.Move` por cima no fim. Escrita direta no
   destino deixaria um upload de 90 MB interrompido virar um pacote truncado com o nome certo,
   que passa no `File.Exists` do `GetMetadata` e só falha no `sha256` do navegador.

`FileSystemModelArtifactStorageTests` cobre os três. Vale notar o que mudou de possibilidade: o
adaptador do MinIO nunca teve teste nenhum, porque exercitá-lo exigia subir o serviço.

**Consequência operacional, e ela é real:** a pasta é agora o único lugar onde os artefatos
existem. Ela precisa sobreviver a deploy (volume, não diretório do build), o backup passa a
incluí-la, e **duas instâncias atrás de um balanceador teriam cada uma a sua cópia** — aí volta a
ser preciso storage compartilhado, seja um volume de rede, seja um object store de novo. Hoje é
uma instância só, e é por isso que a troca compensa.

### O blob de embedding

`FaceEmbeddingBlob` converte `float[]` ↔ blob. O layout é **contrato com o app desktop**:
float32, little-endian, 4 bytes por posição, sem cabeçalho — o que o numpy grava com
`tobytes()`. Um erro ali não estoura: produz vetores lixo e o sintoma "o reconhecimento parou
de pegar ninguém", que aponta para o modelo e não para a serialização. `FaceEmbeddingBlobTests`
trava o formato, incluindo um buffer real gerado pelo numpy.

## Application

```
Interfaces/                          contratos, agrupados por módulo
  Auth/                              ILoginUseCase, IRefreshSessionUseCase, ILogoutUseCase,
                                     IValidateCredentialsUseCase, IPasswordHasher,
                                     IAccessTokenIssuer, IRefreshTokenGenerator
  Users/ Roles/ Departments/         CRUD do controle de acesso; Users tem IPasswordResetNotifier
  Collaborators/                     consulta à base de colaboradores do Sync
  FaceRecognition/                   IMatchFaceUseCase, IEnrollFaceUseCase
  ModelCatalog/                      os onze I*UseCase, IModelArtifactStorage,
                                     ModelArtifactStorageException
  Stations/                          os seis I*UseCase
  ICurrentActor.cs                   transversal: não é de módulo nenhum
UseCases/<Módulo>/                   *UseCase + a classe estática compartilhada do módulo
Communication/
  <Módulo>/Requests/                 um record por arquivo
  <Módulo>/Responses/                um record por arquivo
  <Módulo>/Dtos/                     só quando o DTO não é request nem response
  Errors/                            erros de Result que carregam semântica de transporte
Validators/<Módulo>/                 um validator por arquivo
```

**`Interfaces/` guarda contrato, `UseCases/` guarda implementação.** Os dois espelham a mesma
divisão por módulo, então o par de uma operação são dois arquivos com o mesmo nome em duas
árvores. O contrato de porta de infraestrutura (`IPasswordHasher`, `IModelArtifactStorage`)
mora no módulo que o usa, ao lado dos contratos de caso de uso; só `ICurrentActor` fica na
raiz, porque atravessa módulo. `IAccessTokenIssuer` é implementado na **API**
(`JwtAccessTokenIssuer`, registrado no `Program.cs`), ao lado da validação do JWT: emissão e
validação leem o mesmo `JwtOptions`.

**O que várias operações do mesmo módulo compartilham mora numa classe estática interna na
pasta do módulo em `UseCases/`** — `ModelCatalogMapping`, `StationMapping`,
`FaceRecognitionMapping`, `AuthMapping`, `UsersMapping`, `RolesMapping`. Nenhuma termina em `UseCase`, então nenhuma entra no
container. É onde vive entidade → response, resolução do ator, resolução do espaço de embedding
e tradução de falha do upstream: coisas que, duplicadas por operação, divergiriam na primeira
vez que alguém mexesse numa delas.

**`Communication/` é o contrato de fio, e continua separado do caso de uso.** Co-locar
request/response dentro da pasta da operação não funciona aqui porque os responses são
compartilhados: `CatalogModelResponse` sai de três operações, `CatalogModelVersionResponse` de
três, `AuthResponse` de duas. Co-locar exigiria uma pasta `Shared/` ao lado — que é o mesmo
`Communication/` com outro nome, mais a dúvida recorrente de quando promover um DTO para lá.
Request e response são o contrato da API, organizados por módulo; caso de uso é a orquestração.
As duas coisas versionam em ritmos diferentes.

A subpasta `Dtos/` **só existe quando faz falta**: hoje só em `Communication/Matrix/Dtos`, para
os corpos que o Sync devolve — alvo de desserialização do adaptador, não contrato nosso.
Guardá-los em `Responses/` fazia parecer que renomear um campo nosso era seguro.
`Communication/Common/Responses/PagedResponse<T>` é o envelope de paginação compartilhado.

**Diverge do Q-Zero:** lá os contratos ficam todos em `Abstractions/`, numa pasta plana. Aqui é
`Interfaces/<Módulo>/`. Com vinte e dois casos de uso, uma pasta plana de vinte e oito interfaces não
diz a que módulo cada operação pertence, e a resposta para "o que existe em estação?" passa a
depender de ler nome por nome.

**Diverge do Q-Zero:** lá a pasta é `DTOs/`. Aqui é `Communication/`, e o nome é a diferença:
`DTOs` classifica pela forma do tipo (é um objeto de transferência), `Communication` classifica
pelo papel (é o que entra e sai pela borda HTTP). É o mesmo critério que manda `Errors/` para
dentro dela.

**Diverge do Q-Zero:** lá integração externa aparece como `HttpClient` direto no service. Aqui
o storage de artefatos, o Sync, o hasher de senha, o emissor de token e o e-mail têm porta de verdade, com exceção própria já traduzida
para a linguagem da aplicação. Foi a porta que tornou barata a saída do MinIO: trocar o storage
custou um adaptador novo e nenhuma linha de regra de negócio. É uma das coisas que o Iris faz melhor que o Q-Zero — não desfaça isso ao
aproximar os dois. O que mudou é só o endereço: as portas deixaram de ter pasta `Ports/` e
foram para o módulo que as usa dentro de `Interfaces/`.

**Diverge do Q-Zero:** lá existem `Application/Extensions` e `Application/Options`. Aqui nenhuma
das duas, e pela mesma razão: as duas faziam a camada mais interna encostar em framework.

`Extensions/` guardava o registro por convenção, e era a **única** razão de a Application
referenciar `Microsoft.Extensions.DependencyInjection` — registrar dependência é decisão de
composição, não regra de negócio. As duas varreduras foram para a Infrastructure.

`Options/` guardava `FaceRecognitionOptions`, um POCO mutável recebido pelos casos de uso como
`IOptions<T>`. Os quatro números que ele carregava decidem **quem a catraca libera**: são regra
de negócio, não ajuste técnico, e a palavra "Options" os fazia parecer configuração. Viraram
`Domain/FaceMatchingPolicy` — imutável, com o significado de cada número documentado. Quem lê o
`appsettings` é a Infrastructure, que valida e entrega a política pronta; o caso de uso não sabe
que existe arquivo de configuração.

O resultado é medível: a Application ficou com **três** pacotes — `FluentResults`,
`FluentValidation` e `System.Numerics.Tensors` — e nenhum deles é framework de aplicação.

## DI: convenção, não lista

`AddApplicationUseCases()` e `AddInfrastructureRepositories()`, **as duas na Infrastructure**, registram por nome: todo
`*UseCase` com `I*UseCase` correspondente, e toda `*Repository` com `I*Repository`. As duas são
a mesma varredura (`AddByConvention`) com sufixo diferente, e as duas resolvem o assembly alvo
por `typeof(X).Assembly` — referência que o compilador verifica. A primeira versão vivia na
Application e alcançava a Infrastructure por `Assembly.Load("IrisWeb.Infrastructure")`: uma
dependência escrita em string, invisível ao build, apontando de dentro para fora da camada.
Renomear o assembly deixava build e testes verdes e derrubava o container só na primeira
requisição.

As duas são chamadas explicitamente no `Program.cs`, antes de `AddInfrastructure`, que só cuida
do que precisa de configuração. Escondidas dentro dele, chamar `AddInfrastructureRepositories`
de fora registrava tudo duas vezes sem nenhum sinal.

O sufixo `Service` saiu da varredura junto com a pasta `Services/`. Uma varredura para um
sufixo que não existe mais no assembly não protege nada, e registraria calada a primeira
`*Service` que alguém reintroduzisse fora do padrão documentado aqui.

**A convenção é o contrato.** Se a classe fugir do par nome/interface, o registro
silenciosamente não acontece e o erro só aparece na primeira requisição que resolve aquele
caso de uso. Fora da convenção ficam as dependências que precisam de configuração: `DbContext`,
`HttpClient` tipado, storage de artefatos, opções — essas continuam explícitas em
`DependencyInjection.cs`.

`DependencyInjectionConventionTests` resolve por reflexão **todo** `I*UseCase` e
`I*Repository`, e é a única rede contra o silêncio da convenção: um
`ApproveModelVersionHandler` que deveria se chamar `ApproveModelVersionUseCase` compila, passa
por revisão e só morre no primeiro request. Duas coisas importam nesse teste:

- Se acrescentar um sufixo à convenção, acrescente-o à varredura — senão a rede fica com um
  buraco do tamanho exato da coisa nova.
- **Cada varredura tem um guarda anti-vazio** (`Assert.NotEmpty`), e ele não é decoração: uma
  varredura que não encontra nada passa verde e não protege nada. Foi o que ia acontecer com a
  varredura de `I*Service` quando `Services/` deixou de existir.

## Validação

`AddValidatorsFromAssemblyContaining` + `AddFluentValidationAutoValidation`: validator novo
passa a valer sozinho, e o controller **não** injeta `IValidator<T>` nem chama `ValidateAsync`.

Enquanto era manual, `ClosePresenceRequest` simplesmente não tinha validator e ninguém
percebeu. Não volte a validar à mão.

Divisão: forma e comprimento no validator; regra que precisa de dado ou do conjunto no caso de
uso. Comprimento é medido no valor **já normalizado**, que é o que vai para a coluna — e
segue a **menor** coluna da cadeia (`workstation` é 64 na presença; a
constante vale 64, senão o MySQL truncaria em silêncio ao gravar o log).

## Erro e retorno

`Result` da Application, traduzido em `IrisWeb.API/Errors`:

```json
{ "error": "rótulo curto", "details": "texto pronto para a tela" }
```

`ApiResults.Failure` decide o status: **401** para credencial recusada
(`CredentialRejectedError`), **403** para ação proibida a quem pediu (`ForbiddenError`),
**404** para recurso inexistente (`NotFoundError`), **502** para gateway externo que não respondeu
(`UpstreamGatewayError`), **503** para dependência externa fora do ar
(`UpstreamUnavailableError`), **400** para recusa de negócio. A diferença não é cosmética —
é ela que diz ao cliente se vale tentar de novo.

### Por que `Communication/Errors/` existe

Os tipos ali dentro parecem redundantes com `ErrorResponse` e não são. **`ErrorResponse` é
só o formato do corpo** — `{ error, details }`, sem nenhuma noção de status. **Quem decide o
status é o tipo do erro**, e a decisão acontece em `ApiResults.Failure`, que faz `HasError<T>`
sobre exatamente esses tipos (`CredentialRejectedError`, `ForbiddenError`, `NotFoundError`, `UpstreamGatewayError`, `UpstreamUnavailableError`).

Apagá-los (ou trocá-los por `Error` cru com a mensagem certa) não muda o corpo em nada e por
isso parece seguro. O efeito é **colapsar todos os status em 400**:

- credencial recusada deixaria de ser 401, e a tela de login não conseguiria mais separar
  "confira usuário e senha" de "revise o formulário" — os dois casos chegariam no mesmo código;
- Sync fora do ar deixaria de ser 502, e o usuário leria "dados inválidos" para um
  problema que não é dele nem da senha dele;
- **disco cheio ou volume de artefatos fora do ar deixaria de ser 503 e passaria a ser tratado
  pelo cliente como pedido inválido** — o desktop não tentaria de novo, porque 400 significa
  "não insista, o pedido está errado".

`AuthErrorContractTests` e `ApiResultsTests` travam esses valores. Eles são teste de
caracterização: se uma asserção de status precisar mudar para voltar a passar, isso não é
manutenção de teste, é regressão na tela de login.

O login passa por aqui como todo o resto: `AuthController` → `ILoginUseCase` → `Result`.
Matrícula inexistente e senha errada devolvem o mesmo `CredentialRejectedError` com o mesmo
texto, e o hasher gasta o mesmo tempo nos dois casos (`IPasswordHasher.Verify` com hash nulo).
Refresh inválido é **400**, não 401: é o que o Access Control devolvia e o que o interceptor do
front espera para não renovar em laço.

O corpo de sucesso é o **DTO cru**, nunca o `Result<T>`. `Ok(result)` em vez de
`Ok(result.Value)` serializa `{ isSuccess, value: {...} }`, e o cliente lê os campos como
`undefined`. Já aconteceu; `FaceRecognitionControllerTests` trava o contrato.

Não lance exceção para erro esperado de negócio — o fluxo já usa `Result`.

## Testes

`IrisWeb.Tests`, xUnit, espelhando as camadas: `Domain/`, `Application/`, `Infrastructure/`,
`API/`, mais `Fakes/`.

**Diverge do Q-Zero:** o Q-Zero não tem projeto de teste. Este tem, e é para continuar tendo —
aproximar os dois não significa abrir mão disto.

`InMemoryRepository<T,TId>` é o dublê padrão para teste de aplicação. Ele simula
`AUTO_INCREMENT` e devolve contagem real em `SaveChangesAsync`, porque os casos de uso tratam
`<= 0` como "nada foi persistido" — um dublê ingênuo faria todo teste de escrita passar pelo
ramo de erro.

Não há teste de integração contra o MySQL. O que o EF faz de verdade — nome de coluna,
conversor, tipo — só é verificado subindo a API e chamando o endpoint.

## Acesso

O corte é **inferência vs. cadastro**, não "site vs. outros apps". Leitura de catálogo,
`match` e **download do artefato** são anônimos; escrever qualquer coisa, incluindo listar
estações, exige token. A tabela completa está no `README.md`.

`GET .../artifact` é anônimo porque o `download-url` sempre foi: desktop, lab, studio e o site
baixam o modelo no boot, sem sessão. Em compensação `DownloadModelArtifactUseCase` é hoje o
**único** ponto que impede um artefato não aprovado de sair — o endereço é derivado de ids que o
catálogo anônimo já publica. `PUT .../artifact` exige token e, além dele, a versão em
`Uploading`: a URL pré-assinada expirava sozinha, o endereço fixo não, e sem a checagem de estado
qualquer usuário autenticado sobrescreveria o binário de uma versão já aprovada.

`AllowAnonymous` aqui é decisão de arquitetura, não esquecimento.

Há um segundo corte, por role: aprovar e arquivar versão, e **toda escrita de usuário, role e
departamento**, exigem `master` (`API/Auth/AuthPolicies.cs`, política `MasterOnly` registrada
no `Program.cs`). As rotas anônimas do controle de acesso são só as da tela de login (login,
refresh, valid, esqueci a senha, autocadastro, consultas por matrícula, lista de departamentos)
e têm limite por IP (`AuthPolicies.AnonymousRateLimit`). `AccessEndpointAuthorizationTests`
trava esse mapa: endpoint novo nesses controllers sem atributo de acesso, ou escrita sem
`MasterOnly`, quebra o teste. O claim `roles`
do token vira `ClaimTypes.Role` em `RolesClaimsTransformation`; é isso que `RequireRole` lê.
Endpoint novo de governança entra com `[Authorize(Policy = AuthPolicies.MasterOnly)]`, não com
`[Authorize]` cru.

O token é emitido aqui (`JwtAccessTokenIssuer`) com os claims do Access Control antigo
(`registration`, `department`, `roles`, `applications`) e `aud` = `Jwt:Audiences`, porque Q-Zero
e master-data validam o mesmo token com a mesma chave. Não renomeie claim nem troque o formato
de `GET /api/v1/departments` (`string[]`) sem combinar com esses consumidores.

Usuário tem soft delete (`HasQueryFilter(Active)` em `UserConfiguration`). Toda consulta de
usuário esconde o desativado, **exceto** as checagens de unicidade (`RegistrationExistsAsync`,
`EmailExistsAsync`) e "departamento em uso", que usam `IgnoreQueryFilters()` porque o índice
único e a FK do banco continuam contando a linha.

## Plataforma do modelo

`AiModel.Platform` (`Web` | `Desktop`) fixa o formato: web é ONNX, desktop é IR (OpenVINO). A
regra é `Domain/Rules/ModelPlatformPolicy`, e `ModelCatalogMapping.RuntimeViolation` /
`ManifestViolation` a aplicam em `CreateModelVersionUseCase` e `CompleteUploadUseCase`. O
Domain não lê JSON — o manifesto é aberto na Application e a política só compara strings. A
coluna `platform` foi criada à mão (DDL no `README.md`).

**Um valor por alvo, e não um conjunto de sinônimos.** Web é `onnxruntime-web`, desktop é
`openvino`. Houve uma versão intermediária desta regra que aceitava `onnx`, `onnxruntime` e
`onnxruntime-web` como equivalentes, para acomodar o que estava gravado — e o resultado foi três
grafias para um motor só, com a tela do catálogo mostrando dois nomes para o mesmo ONNX Runtime
Web. O banco foi normalizado à mão e a regra voltou a ter um valor. Se aparecer uma grafia nova,
normalize o dado; não alargue a política.

`runtime` nomeia o motor, `format` nomeia o arquivo. É por isso que web é `onnxruntime-web` e o
formato é `onnx` — dois campos, dois significados. Nada no código despacha sobre `runtime`: quem
decide o motor é `platform`, e o campo existe para inventário e para o contrato com os outros
apps da plataforma.
