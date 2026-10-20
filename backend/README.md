# MeOne.API

API do MeOne. Hoje cobre **controle de acesso**: autenticação (login, refresh token, logout,
reset de senha) e CRUD de usuários, roles e departamentos. O MeOne emite o próprio JWT.

## Organização

| Projeto | Papel |
| --- | --- |
| `MeOne.Domain` | Entidades (`Entity<TId>` na base), regras, errors e contratos de repositório. |
| `MeOne.Application` | Casos de uso, contratos (`Interfaces/`), DTOs de fio (`Communication/`) e validators. Devolve `Result` (FluentResults). |
| `MeOne.Infrastructure` | MySQL `meone`, identidade (BCrypt, refresh token), e-mail SMTP, health checks. |
| `MeOne.API` | Controllers finos, emissão e validação de JWT, CORS, tradução `Result` → HTTP. |
| `MeOne.Tests` | Testes de domínio, aplicação, infraestrutura e API. |

As convenções de código (camadas, nomes, DI por convenção, validação automática) estão no
`AGENTS.md` deste diretório. Ele é a referência normativa; este README descreve o serviço.

## Regra de acesso

A autorização é por **papel funcional** (o que a pessoa faz) com **escopo** (onde ela atua) como
segundo eixo. Papéis agrupam permissões; o usuário recebe papéis, cada um sem escopo (vale em
tudo) ou num nó da hierarquia **Planta > Área > Linha > Posto**. Uma atribuição vale para o nó e
para tudo abaixo dele: supervisor da Área A vale em todas as linhas e postos da Área A.

Não há política de fallback: endpoint sem atributo seria anônimo, então **toda action declara o
acesso**: `[AllowAnonymous]`, `[Authorize]` (só estar logado) ou `[RequirePermission(...)]`.

- **Anônimo**: só o que a tela de login precisa — login, refresh, esqueci a senha (pedido e
  confirmação), autocadastro, lista de departamentos e `GET /health/live|ready`. As rotas de
  credencial têm limite por IP.
- **Logado**: logout, `auth/valid`, `auth/me` e leitura de escopos.
- **Por permissão**: todo o resto (ver tabela em "Controle de acesso"). Sem a permissão a resposta
  é **403** com o envelope de erro padrão.

`AccessEndpointAuthorizationTests` trava esse mapa.

## Controle de acesso

Portado do `access-control-api` e redesenhado para MES. Código nos módulos `Auth`, `Users`,
`Roles`, `Departments` e `Scopes` de cada camada.

### Permissões, papéis e escopo

As permissões são um **catálogo fixo no código** (`Domain/Rules/Permissions.cs`), espelhado na
tabela `permissions` pela migration. As de administração só valem **sem escopo** (`global_only`):

| Permissão | Para quê | Só sem escopo |
| --- | --- | --- |
| `users.read` | consultar usuários | sim |
| `users.manage` | cadastrar, editar e desativar usuários | sim |
| `users.reset-password` | redefinir a senha de usuários | sim |
| `roles.read` | consultar papéis e permissões | sim |
| `roles.manage` | criar e editar papéis e suas permissões | sim |
| `roles.assign` | atribuir e revogar papéis | sim |
| `departments.manage` | cadastrar e editar departamentos | sim |
| `scopes.manage` | cadastrar plantas, áreas, linhas e postos | sim |
| `system.health` | ver a saúde detalhada (`GET /health`) | sim |
| `production.read` / `.execute` / `.supervise` | consultar, apontar, supervisionar produção | não |
| `quality.inspect` / `.release` | inspecionar, liberar/bloquear material | não |
| `maintenance.request` / `.execute` | abrir e atender chamados | não |
| `engineering.manage` | roteiros, setups e parâmetros | não |
| `planning.manage` | planejar e sequenciar ordens | não |

Os papéis que nascem na migration (`Domain/Rules/RoleCatalog.cs`):

| Papel | Permissões |
| --- | --- |
| `master` (id 1) | **todas**, inclusive as que forem criadas depois. Só sem escopo. Não pode ser renomeado, excluído nem ter permissões editadas |
| `gestor-acessos` | `users.*`, `roles.read`, `roles.assign`, `departments.manage` |
| `supervisor-producao` | `production.*`, `quality.inspect`, `maintenance.request` |
| `operador` | `production.read`, `production.execute`, `maintenance.request` |
| `qualidade` | `production.read`, `quality.inspect`, `quality.release` |
| `manutencao` | `production.read`, `maintenance.request`, `maintenance.execute` |
| `engenharia-processos` | `production.read`, `engineering.manage`, `quality.inspect` |
| `pcp` | `production.read`, `planning.manage` |

Outros papéis se criam pela API (`POST /roles` com a lista de permissões).

**Regras que impedem escalonamento:**

- Ninguém concede, retira nem inclui num papel uma permissão que não tem — no escopo em questão.
  Um supervisor da Área A não consegue dar um papel válido na Área B.
- Ninguém edita, desativa, troca a senha ou derruba a sessão de um usuário que tenha alguma
  permissão que o ator não tenha (sem escopo). Gestor de acessos não mexe no administrador.
- O último administrador ativo não pode ser desativado nem perder o `master`, e ninguém tira o
  `master` de si mesmo.
- Papel com permissão administrativa só pode ser atribuído sem escopo.

**Como a permissão é resolvida:** o JWT carrega só identidade (`sub`, `registration`, `name`,
`email`, `department`), o claim `roles` (nomes dos papéis, compatível com Q-Zero/master-data) e o
`sstamp`. A cada requisição a API carrega o perfil de acesso do banco (cache de 30 s por usuário).
Mudança de papel ou permissão vale em segundos, sem esperar o token expirar.

### Sessão e senha

- **Login**: `{ registration, password }` → `{ token, refreshToken, refreshExpires }`. Matrícula
  inexistente, senha errada e conta bloqueada respondem igual (401, mesma mensagem, mesmo tempo).
  **5 erros seguidos bloqueiam a conta por 15 min.** O login derruba as sessões anteriores.
- **Access token**: 15 min (`Jwt:ExpirationInMinutes`), HS256 (outros algoritmos recusados),
  tolerância de relógio de 30 s. Leva o `sstamp` do usuário; **trocar a senha, desativar, fazer
  logout ou detectar roubo de refresh muda o stamp e derruba na hora todos os access tokens** dele.
- **Refresh token**: rotação a cada uso; o banco guarda só o SHA-256. **Reusar um refresh já
  usado é tratado como roubo**: todas as sessões do usuário caem. Refresh inválido responde 400.
- **Esqueci a senha** (anônimo):
  1. `POST /users/password-reset { registration }` → sempre **202**, exista o usuário ou não. Se
     ele tem e-mail, recebe um código de 8 caracteres válido por 15 min (no máximo um pedido por
     minuto). A senha atual continua valendo.
  2. `POST /users/password-reset/confirm { registration, code, newPassword }` → 204. Código errado
     5 vezes invalida o pedido. Matrícula inexistente e código errado respondem igual.
- **Senha provisória** (`POST /users/{id}/temporary-password`, `users.reset-password`): devolve
  `Multi` + 12 caracteres aleatórios para o administrador repassar.
- Senha nova: 8 a 72 **bytes** (limite do BCrypt). Login aceita senha antiga mais curta.

### Endpoints

| Endpoint | Acesso |
| --- | --- |
| `POST /api/v1/auth`, `POST /api/v1/auth/refresh` | anônimo, limite por IP |
| `POST /api/v1/auth/logout` `{ userId }` | logado; outro usuário exige `users.manage` |
| `POST /api/v1/auth/valid` `{ registration, password }` | logado; só a própria matrícula |
| `GET /api/v1/auth/me` | logado — identidade, papéis com escopo e permissões efetivas |
| `GET /api/v1/users`, `GET /users/{id}`, `GET /users/get-by-registration/{registration}`, `GET /users/roles/{userId}` | `users.read` |
| `POST /users`, `PUT /users/{id}`, `DELETE /users/{id}` | `users.manage` |
| `PUT /users/reset-password-manually`, `POST /users/{id}/temporary-password` | `users.reset-password` |
| `POST /users/password-reset`, `POST /users/password-reset/confirm`, `POST /users/not-authenticated` | anônimo, limite por IP |
| `GET /api/v1/roles`, `GET /roles/all`, `GET /roles/{id}`, `GET /api/v1/permissions` | `roles.read` |
| `POST /roles`, `PUT /roles/{id}`, `PUT /roles/{id}/permissions`, `DELETE /roles/{id}` | `roles.manage` |
| `POST /roles/set-role`, `POST /roles/revoke-role` `{ userId, roleId, scopeId? }` | `roles.assign` |
| `GET /api/v1/departments`, `GET /departments/all` | anônimo (`string[]` / `{ id, name }[]`) |
| `POST/PUT/DELETE /departments` | `departments.manage` |
| `GET /api/v1/scopes`, `GET /scopes/{id}` | logado |
| `POST/PUT/DELETE /scopes` `{ type, code, name, parentId }` | `scopes.manage` |
| `GET /health` (detalhado) | `system.health` |

`GET /users/roles/{userId}` devolve `{ id, name, description, scopeId, scopeCode, scopeName }` por
atribuição. `type` de escopo: `plant`, `area`, `line`, `workstation`; o pai tem que ser do nível
imediatamente acima.

### Outras proteções

- Erros de corpo inválido (JSON malformado, tipo errado) respondem uma mensagem genérica, sem
  detalhe interno do parser. Erro 500 fora de Development não expõe a exceção.
- Headers em toda resposta: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`,
  `Referrer-Policy: no-referrer`, `Cross-Origin-Opener-Policy: same-origin`; nas rotas `/api`,
  `Cache-Control: no-store`. O Kestrel não envia o header `Server`.
- Login falho fica no log (matrícula e IP, nunca a senha).

### O que mudou em relação ao Access Control

- **Refresh token**: o AC devolvia ao cliente o próprio hash guardado no banco. Agora o cliente
  recebe o valor cru e o banco guarda só o SHA-256. **Todo mundo precisa logar de novo.**
- **Autorização**: saiu o "é master ou não"; entrou permissão por papel e escopo. `set-role` e
  `revoke-role` aceitam `scopeId` (opcional — sem ele, vale em tudo).
- **Esqueci a senha**: o AC gerava a senha nova e devolvia a quem soubesse a matrícula. Agora é
  código por e-mail e confirmação; a senha só muda quando o dono confirma.
- **Rotas que estavam abertas** (`set-role`, `revoke-role`, `reset-password-manually`,
  `get-by-registration`, `auth/valid`) agora exigem login e permissão.
- **`modifiedBy`/`deletedBy`** saem do token, não do corpo.
- Paginação de roles começa em 0 e `totalCount` é o total da tabela; departamento normalizado em
  minúsculas; matrícula/e-mail de usuário desativado contam como ocupados.

### Schema

O schema é definido pelas **migrations do EF** em `MeOne.Infrastructure/Migrations`:
`Initial` (tabelas do Access Control) e `PermissionsAndScopes` (permissões, papéis do catálogo,
escopos, códigos de reset, colunas de bloqueio e `security_stamp`). Para ver o SQL:

```
dotnet ef migrations script --project MeOne.Infrastructure --startup-project MeOne.API
```

### Migração dos dados (uma vez)

Crie o banco `meone` vazio e aplique as migrations (`dotnet ef database update`, abaixo). Depois,
com os dois bancos no mesmo servidor (senão, `mysqldump` das tabelas e import no `meone`), copie
os dados. Antes, confira se a normalização de departamento não colide:
`SELECT LOWER(TRIM(name)), COUNT(*) FROM access_control.departments GROUP BY 1 HAVING COUNT(*) > 1;`

```sql
INSERT INTO meone.departments (id, name) SELECT id, LOWER(TRIM(name)) FROM access_control.departments;
INSERT INTO meone.applications (id, name, active) SELECT id, name, active FROM access_control.applications;
INSERT INTO meone.users (id, registration, name, email, password, active, modified_at, modified_by, fk_department)
  SELECT id, registration, name, email, password, active, modified_at, modified_by, fk_department FROM access_control.users;
UPDATE meone.users SET security_stamp = REPLACE(UUID(), '-', '');
INSERT INTO meone.roles (id, name, description)
  SELECT id + 1000, name, description FROM access_control.roles WHERE name <> 'master';
INSERT INTO meone.user_roles (fk_user, fk_role)
  SELECT ur.fk_user, IF(r.name = 'master', 1, r.id + 1000)
  FROM access_control.user_roles ur JOIN access_control.roles r ON r.id = ur.fk_role;
INSERT INTO meone.user_applications (id, fk_user, fk_application) SELECT id, fk_user, fk_application FROM access_control.user_applications;
```

Os papéis antigos do AC entram com `id + 1000` e **sem permissões**: eles eram só nomes. Depois da
cópia, dê a cada usuário um papel do catálogo (com escopo, quando fizer sentido) e exclua os papéis
antigos que ficarem sem uso. `refresh_tokens` não migra: o formato mudou.

As senhas são BCrypt custo 12 nos dois lados — continuam valendo.

### Primeiro master (banco novo)

A role `master` já vem da migration; falta um usuário com ela. Crie um departamento por SQL, faça
o autocadastro e dê a role:

```sql
INSERT INTO departments (name) VALUES ('ti');
```

```
POST /api/v1/users/not-authenticated
{ "name": "Nome Completo Aqui", "email": "voce@empresa.com", "registration": 1001,
  "password": "uma-senha-forte", "department": "ti" }
```

```sql
INSERT INTO user_roles (fk_user, fk_role)
SELECT id, 1 FROM users WHERE registration = 1001;
```

Depois disso, escopos, departamentos, papéis e usuários se cadastram pela API, como master.

## Workflow

Fluxo de aprovação (portado do SPM): criar com responsáveis e anexos, aprovar, rejeitar com
negociação entre quem rejeitou e o criador, substituir responsáveis e acompanhar o histórico.
Endpoints em `api/v1/workflows` (`WorkflowsController`), todos exigem a permissão `workflow.use`,
concedida pelo papel `workflow`. As regras estão descritas no `AGENTS.md`. Os avisos por e-mail de
prazo ainda não foram portados.

## Organograma

Árvore de cargos da empresa (cargo, departamento, cargo superior e ocupante). Todo usuário logado consulta em
`GET api/v1/org-chart`; criar, mover, definir ocupante e excluir cargos exige a permissão `orgchart.manage`,
concedida pelo papel `organograma`. Regras no `AGENTS.md`.

## Instruções de trabalho

Cadastro de instruções com PDF e passos ilustrados (portado do SPM). Qualquer usuário logado consulta em
`api/v1/work-instructions`; criar, editar e excluir exige `engineering.manage`. Regras no `AGENTS.md`.

## Fornecedores

Cadastro e qualificação (portado do SPM-Qualidade) em `api/v1/suppliers`. Qualquer usuário logado consulta; escrever exige
`suppliers.register` e, em fornecedor de outra pessoa, `suppliers.manage`. Regras no `AGENTS.md`.

## Linhas e postos

Mapa da linha, alocação de pessoas, início e encerramento da produção e produtividade por posto, em `api/v1/production`
(`lines`, `shifts`, `products`, `layouts`, `runs`). Qualquer usuário logado consulta; cadastros e layouts exigem `engineering.manage`,
alocar e iniciar exigem `production.supervise` e lançar produção exige `production.execute`. Regras no `AGENTS.md`.

## OEE

Disponibilidade, desempenho e qualidade por posto e por linha, calculados sobre as produções, em `api/v1/oee` (`runs`, `config`, `formulas`).
Qualquer usuário logado consulta e testa fórmulas; criar, ativar e excluir conjuntos de fórmulas exige `engineering.manage`.
Paradas e defeitos vêm do help desk (`IOeeLossSource`). Regras no `AGENTS.md`.

## Help desk

Setores com formulário montável (árvore de perguntas com ramificação), formulário de resposta do técnico, abertura e atendimento de chamados, em `api/v1/helpdesk` (`sectors`, `field-templates`, `roles`, `locations`, `tickets`). Qualquer usuário logado abre e acompanha chamados; montar setores exige `helpdesk.manage` (perfil `gestor-helpdesk`); atender depende dos papéis do setor. Chamado com linha parada e defeitos alimentam o OEE. Regras no `AGENTS.md`.

## Banco de dados local

Pré-requisitos: Docker e .NET 10 SDK. Na pasta `backend/`:

```
docker compose up -d
dotnet tool restore
dotnet ef database update --project MeOne.Infrastructure --startup-project MeOne.API
```

1. `compose.yaml` sobe um MySQL 8.0 em `localhost:3306` (banco/usuário/senha `meone`, volume
   `meone-mysql`). Credenciais de desenvolvimento, não de produção.
2. `dotnet tool restore` instala o `dotnet-ef` na versão fixada em `dotnet-tools.json`.
3. `database update` aplica as migrations. Sem `ConnectionStrings__MeOne` definido, o `dotnet ef`
   usa o MySQL do compose (`MeOneDbContextFactory`); para outro banco, defina a variável antes.

Depois crie o primeiro master ("Controle de acesso → Primeiro master") e rode a API. Em
`Development`, o `appsettings.Development.json` já aponta para o banco do compose e define o
audience `meone`; só falta a chave do JWT:

```
dotnet user-secrets set "Jwt:Key" "<pelo-menos-32-caracteres>" --project MeOne.API
dotnet run --project MeOne.API --launch-profile http
```

Para zerar o banco: `docker compose down -v` e repita os passos.

## Configuração local

O `appsettings.json` versionado só lista as chaves obrigatórias, em branco; os demais valores
têm padrão no código. Preencha por variáveis de ambiente ou `dotnet user-secrets`:

- `ConnectionStrings__MeOne` — MySQL `meone`, a **única** base
- `Jwt__Key` — mínimo 32 bytes (HS256). É com ela que o MeOne **assina** os tokens
- `Jwt__Audiences__0..n` — **obrigatório**; vão no token emitido e são os únicos aceitos na validação
- `Jwt__Issuer` (padrão `access-control`), `Jwt__ExpirationInMinutes` (padrão 15),
  `Jwt__RefreshTokenExpirationInHours` (padrão 8)
- `Email__SmtpServer`, `Email__Port`, `Email__FromAddress`, `Email__FromName`, `Email__User`,
  `Email__Password` — SMTP do "esqueci a senha". Sem eles a API sobe, mas o reset anônimo
  recusa (a senha só seria entregue por e-mail)
- `RateLimiting__AnonymousCredentialsPerMinute` (padrão 30) — limite por IP nas rotas anônimas
  de credencial. Atrás de proxy sem `X-Forwarded-For` tratado, todo mundo vira o mesmo IP
- `Cors__Origins__0` — origens liberadas fora de Development (em Development qualquer origem passa)
- `Swagger__Enabled` — liga o Swagger fora de Development
- `Supplier__Storage__Path` — pasta dos documentos de fornecedores (padrão `uploads/suppliers`)
- `WorkInstruction__Storage__Path` — pasta dos PDFs e imagens das instruções (padrão `uploads/work-instructions`)
- `Workflow__Storage__Path` — pasta dos anexos do Workflow (padrão `uploads/workflows`, relativa ao
  diretório de execução). Em contêiner, monte um volume nela

A API recusa subir sem `Jwt:Key`, `Jwt:Audiences` e `ConnectionStrings:MeOne`.

Comandos:

```
dotnet run --project MeOne.API --launch-profile http
dotnet test MeOne.slnx
```

## Servindo a SPA (frontend)

Se houver um build de frontend em `MeOne.API/wwwroot/`, esta API o serve: estáticos via
`UseStaticFiles` e um `MapFallback` em `Program.cs` que devolve `wwwroot/index.html` para `/` e
qualquer rota de cliente. Sem `index.html`, o fallback responde 404.

HTTPS em produção entra por variável de ambiente:

```
Kestrel__Endpoints__Https__Url=https://0.0.0.0:8443
Kestrel__Certificates__Default__Path=cert.pem
Kestrel__Certificates__Default__KeyPath=key.pem
```

## Migrations

O schema é versionado por migrations do EF Core, em `MeOne.Infrastructure/Migrations`. Fluxo para
mudar o banco:

1. Altere a entidade e o mapping (`MeOne.Infrastructure/Mappings/*Configuration`).
2. Gere a migration:
   `dotnet ef migrations add <NomeDaMudanca> --project MeOne.Infrastructure --startup-project MeOne.API`
3. Revise o `Up`/`Down` gerado e o SQL (`dotnet ef migrations script <anterior> <nova> ...`).
4. Aplique: `dotnet ef database update --project MeOne.Infrastructure --startup-project MeOne.API`.

A API **não** aplica migration sozinha no boot: em produção, gere o script
(`dotnet ef migrations script --idempotent ...`) e rode no deploy, para a mudança de schema ser
explícita. `MigrationTests` falha se o mapeamento mudar sem migration correspondente
(`HasPendingModelChanges`), e `AccessMappingTests` confere os nomes de tabela e coluna.

Cada coluna é nomeada à mão com `HasColumnName`, porque as tabelas são snake_case.

O stack EF fica em `9.0.x` porque o provider MySQL (Pomelo 9.0.0) depende de EF Core 9; os
pacotes rodam sem problema no `net10.0`.
