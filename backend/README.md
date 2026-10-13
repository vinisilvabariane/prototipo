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

Não há política de fallback: endpoint sem atributo seria anônimo, então **toda action declara
o acesso** (`[AllowAnonymous]`, `[Authorize]` ou `[Authorize(Policy = AuthPolicies.MasterOnly)]`).

- **Anônimo**: só o que a tela de login precisa — login, refresh, `valid`, esqueci a senha,
  autocadastro, consulta de usuário por matrícula, lista de departamentos e `GET /health/live|ready`.
  As rotas de credencial têm limite por IP.
- **Token**: leitura de usuários e roles, logout, `GET /health` (detalhado).
- **`master`**: toda escrita de usuário, role e departamento. Quem não tem a role recebe
  **403** com o envelope de erro padrão.

A tabela completa está abaixo, em "Controle de acesso". `AccessEndpointAuthorizationTests` trava
esse mapa.

## Controle de acesso

Portado do `access-control-api` (que deixa de existir como serviço). Mesmas tabelas, mesmos
claims no JWT e mesmas rotas — só que sob `/api/v1` desta API. Código nos módulos `Auth`,
`Users`, `Roles` e `Departments` de cada camada.

| Endpoint | Acesso | |
| --- | --- | --- |
| `POST /api/v1/auth` | anônimo, limite por IP | login → `{ token, refreshToken, refreshExpires }`. Revoga as sessões anteriores do usuário |
| `POST /api/v1/auth/refresh` | anônimo | rotação: o refresh usado é revogado e um novo é emitido. Falha → **400** (não 401, para o interceptor do front não renovar em laço) |
| `POST /api/v1/auth/logout` | token | `{ userId }`: o próprio usuário, ou qualquer um se for `master` (403 caso contrário) |
| `POST /api/v1/auth/valid` | anônimo, limite por IP | confere matrícula e senha sem abrir sessão |
| `GET /api/v1/users?page=0&pageSize=10&search=` | token | página **começa em 0**; busca por nome/e-mail (contém) ou matrícula (exata) |
| `GET /api/v1/users/{id}` | token | 404 se não existe ou está desativado |
| `GET /api/v1/users/roles/{userId}` | token | roles do usuário (lista vazia se não existe) |
| `POST /api/v1/users` | `master` | cria; e-mail tem que ser `@grupomultilaser.com.br` |
| `PUT /api/v1/users/{id}` | `master` | |
| `DELETE /api/v1/users/{id}` | `master` | soft delete (`active = 0`) + derruba as sessões. Não dá para desativar a si mesmo |
| `PUT /api/v1/users/reset-password-manually` | `master` | `{ userId, newPassword }` → 204, derruba as sessões |
| `POST /api/v1/users/reset-password/{registration}` | anônimo, limite por IP | "esqueci a senha": gera `Multi` + 6 caracteres. Anônimo recebe **só por e-mail** (`newPassword: null`); master recebe na resposta |
| `GET /api/v1/users/get-by-registration/{registration}` | anônimo, limite por IP | 400 se não existe (como no Access Control) |
| `POST /api/v1/users/not-authenticated` | anônimo, limite por IP | autocadastro; nasce sem role |
| `GET /api/v1/roles?page=0&pageSize=10`, `GET /api/v1/roles/all`, `GET /api/v1/roles/{id}` | token | |
| `POST`/`PUT`/`DELETE /api/v1/roles[/{id}]` | `master` | a role `master` não pode ser renomeada nem excluída; role em uso não exclui |
| `POST /api/v1/roles/set-role`, `POST /api/v1/roles/revoke-role` | `master` | `{ userId, roleId }`. Ninguém tira a `master` de si mesmo |
| `GET /api/v1/departments` | anônimo | **`string[]`** só com os nomes — formato que o SYNC/Q-Zero consome, não mudar |
| `GET /api/v1/departments/all` | anônimo | `{ id, name }[]` |
| `POST`/`PUT`/`DELETE /api/v1/departments[/{id}]` | `master` | nome sempre em minúsculas; departamento com usuário (mesmo desativado) não exclui |

**JWT** (HS256, `Jwt:Key`): `sub` (id), `name`, `email`, `jti`, `registration`, `department`
(nome), `roles` (array), `applications` (array, só aplicações ativas), `aud` = todos de
`Jwt:Audiences`, `iss` = `Jwt:Issuer`. Role atribuída/revogada só aparece no token no próximo
login ou refresh. `AccessEndpointAuthorizationTests` trava o mapa de acesso acima.

### O que mudou em relação ao Access Control

Bugs e brechas corrigidos no port — quem consumia o serviço antigo precisa saber:

- **Refresh token**: o AC devolvia ao cliente o próprio hash guardado no banco (o hash não
  protegia nada). Agora o cliente recebe o valor cru e o banco guarda só o SHA-256. Validade em
  `Jwt:RefreshTokenExpirationInHours` (o AC usava 1 h no login e 8 h no refresh, fixos no código).
  **Todo mundo precisa logar de novo depois da troca.**
- **`aud`** vem da configuração (era fixo no código).
- **Rotas que estavam abertas**: `set-role`/`revoke-role` (qualquer um, sem login) e
  `reset-password-manually` (anônimo, trocava a senha de qualquer id) agora exigem `master`.
  Criar/editar/desativar usuário exige `master` (antes bastava estar logado). Leitura de roles
  passou a exigir token.
- **Reset anônimo** devolvia a senha nova a quem soubesse a matrícula. Agora a senha só vai por
  e-mail; sem e-mail cadastrado ou com falha no envio, a senha antiga é mantida.
- **`modifiedBy`/`deletedBy`** saem do token, não do corpo (o corpo era confiado cegamente). Os
  campos antigos no JSON são ignorados.
- **Senha**: nova senha tem 8 a 72 caracteres (72 é o limite do BCrypt). O login não exige
  mínimo — o AC cadastrava senha de 6 e barrava login com menos de 8.
- **Paginação** de roles começa em 0, igual à de usuários (era 1), e `totalCount` é o total da
  tabela (era o tamanho da página).
- **Departamento** é normalizado para minúsculas na gravação e na busca (antes dependia de
  alguém cadastrar em minúsculas).
- **Matrícula/e-mail de usuário desativado** contam como ocupados — o índice único do banco
  conta, e o AC estourava erro de banco.
- **Logout** não é mais só de `master`: cada usuário encerra a própria sessão.
- Sem e-mail configurado ou com falha de SMTP, o erro é logado; o AC engolia em silêncio.

### Schema

O schema é definido pelas **migrations do EF** em `MeOne.Infrastructure/Migrations` — ver "Banco de
dados local" e "Migrations" abaixo. A `Initial` cria as 7 tabelas (`users`, `roles`, `user_roles`,
`departments`, `applications`, `user_applications`, `refresh_tokens`) com o mesmo layout do banco
`access_control`, e já insere a role `master` (id 1). Para ver o SQL:

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
DELETE FROM meone.roles; -- tira a master semeada pela migration: as roles vêm do AC
INSERT INTO meone.roles (id, name, description) SELECT id, name, description FROM access_control.roles;
INSERT INTO meone.applications (id, name, active) SELECT id, name, active FROM access_control.applications;
INSERT INTO meone.users (id, registration, name, email, password, active, modified_at, modified_by, fk_department)
  SELECT id, registration, name, email, password, active, modified_at, modified_by, fk_department FROM access_control.users;
INSERT INTO meone.user_roles (id, fk_user, fk_role) SELECT id, fk_user, fk_role FROM access_control.user_roles;
INSERT INTO meone.user_applications (id, fk_user, fk_application) SELECT id, fk_user, fk_application FROM access_control.user_applications;
-- refresh_tokens não migra: os do AC guardam o valor que o cliente tem, e o formato mudou.
```

As senhas são BCrypt custo 12 nos dois lados — continuam valendo.

**Banco que já tem as tabelas** (criado à mão antes das migrations): não rode a `Initial` — ela
tentaria recriar tudo. Marque-a como aplicada e siga com as próximas normalmente:

```sql
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
  `MigrationId` varchar(150) NOT NULL, `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`));
INSERT INTO `__EFMigrationsHistory` VALUES ('20261013131123_Initial', '9.0.10');
```

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

Depois disso, departamentos, roles e usuários se cadastram pela API, como master.

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
