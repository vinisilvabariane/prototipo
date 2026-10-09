# IrisWeb.API

API única da plataforma Iris. Absorveu o `iris-core`: catálogo/governança/distribuição
de modelos e reconhecimento facial agora vivem todos aqui. Não existe mais o hop
`IrisWeb.API → Core :5276`.

Absorveu também o **Access Control**: usuários, roles, departamentos, login, refresh token
e reset de senha moram aqui (ver "Controle de acesso" abaixo). O IrisWeb emite o próprio JWT;
não existe mais o hop `IrisWeb.API → Access Control :8083`.

Absorveu também o **MinIO**, pela mesma razão e na mesma direção: os artefatos de modelo
ficam agora numa pasta em disco servida por esta API. Não existe mais o hop
`cliente → MinIO :9000`, nem a URL pré-assinada que ele exigia. **Uma aplicação para
subir, não duas.**

Qualquer app da plataforma (`iris-desktop`, `iris-lab`, `iris-studio`, o site) consome
esta mesma API.

## Organização

| Projeto | Papel |
| --- | --- |
| `IrisWeb.Domain` | Entidades (`Entity<TId>` na base), enums, regras, errors e contratos de repositório. |
| `IrisWeb.Application` | Casos de uso, contratos (`Interfaces/`), DTOs de fio (`Communication/`), validators e opções. Devolve `Result` (FluentResults). |
| `IrisWeb.Infrastructure` | MySQL `iris` (catálogo, embeddings, vínculo de aparelho, presença), storage de artefatos em disco, identidade (BCrypt, refresh token), e-mail SMTP, health checks. |
| `IrisWeb.API` | Controllers finos, autenticação JWT, CORS, tradução `Result` → HTTP. |
| `IrisWeb.Tests` | Testes de domínio, aplicação e infraestrutura. |

As convenções de código (camadas, nomes, DI por convenção, validação automática) estão no
`AGENTS.md` deste diretório. Ele é a referência normativa; este README descreve o serviço.

## Regra de acesso

O corte é **inferência vs. cadastro**, não "site vs. outros apps":

**Abertos (`AllowAnonymous`)** — é o que um app precisa para rodar inferência:

| Endpoint | |
| --- | --- |
| `POST /api/v1/face-recognition/match` | compara um embedding |
| `GET /api/v1/models` | lista o catálogo |
| `GET /api/v1/models/{modelId}` | detalhe de um modelo |
| `GET /api/v1/models/{modelId}/versions/{versionId}/download-url` | endereço do artefato |
| `GET /api/v1/models/{modelId}/versions/{versionId}/artifact` | baixa o `model-package.zip` |
| `POST /api/v1/auth`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/valid` | login (com limite por IP) |
| `POST /api/v1/users/reset-password/{registration}`, `POST /api/v1/users/not-authenticated`, `GET /api/v1/users/get-by-registration/{registration}`, `GET /api/v1/colaboradores/get-by-registration/{registration}`, `GET /api/v1/departments[/all]` | tela de login: esqueci a senha, autocadastro (ver "Controle de acesso") |
| `GET /api/v1/argos/summary?from=&to=&bucket=&shift=&result=&station=&setup=` | resumo do Crono Argos (totais, turnos, baldes, falhas, estações). Aberto porque a TV da linha não tem login; só agregados, sem nome de pessoa |
| `GET /health/live`, `GET /health/ready` | sondas |

**Autenticados (`Authorize`)** — cadastro, escrita e governança:

| Endpoint | |
| --- | --- |
| `POST /api/v1/face-recognition/enroll` | grava embedding (dado biométrico) |
| `POST /api/v1/face-recognition/enroll/batch` | grava o cadastro inteiro depois da triagem (fotos que não batem, rosto de outra matrícula, parecido com outra pessoa) |
| `GET /api/v1/stations/timeline` | consulta o histórico do posto (presença + posto vazio), agrupado por dia |
| `GET /api/v1/stations/line-timeline?line=&date=&from=&to=` | linha do tempo da linha no turno: posto a posto, quem esteve, se era treinado, vazio e rosto desconhecido |
| `GET /api/v1/argos/cycles?from=&to=&shift=&result=&station=&setup=&page=&pageSize=` | peças do Route Verify, da mais nova à mais antiga, paginadas |
| `GET /api/v1/argos/filters` | estações e setups dos últimos 90 dias, para os filtros da tela |
| `POST /api/v1/stations/presence/{id}/close` | fecha a sessão de presença |
| `POST /api/v1/stations/vacancy/{id}/close` | fecha o período de posto vazio |
| `POST /api/v1/stations/unknown/{id}/close` | fecha o período de rosto desconhecido, com a matrícula mais parecida e a distância |
| `GET /api/v1/matrix/lines` | linhas do Sync (repasse) |
| `GET /api/v1/matrix/products?line=` | produtos com layout salvo na linha, com os turnos (repasse) |
| `GET /api/v1/matrix/stations?line=&product=&shift=` | postos do layout, com geometria e quem está alocado agora (repasse) |
| `GET /api/v1/matrix/stations/{id}/recommendations?line=&product=&shift=` | recomendados do posto, na ordem do Sync (repasse; o id do turno sai de `GET /shifts` do Sync) |
| `GET /api/v1/matrix/stations/{id}/candidates/{matricula}?line=&product=&shift=` | busca o operador pela matrícula, com o nível dele na atividade do posto (repasse) |
| `DELETE /api/v1/matrix/stations/{id}/allocation?line=&product=&shift=` | libera o posto ("Liberar posto selecionado" do Sync): tira quem está alocado só deste posto — grava **no Sync** |
| `POST /api/v1/matrix/allocations` | aloca um colaborador (pela matrícula: recomendado, digitada ou reconhecida pelo rosto) num posto — grava **no Sync** |
| `GET /api/v1/matrix/devices/{deviceId}` | vínculo do aparelho com um posto da matriz (`204` se não houver) |
| `PUT /api/v1/matrix/devices/{deviceId}` | vincula o aparelho a linha + produto + turno + posto |
| `DELETE /api/v1/matrix/devices/{deviceId}` | desvincula |
| `GET /api/v1/matrix/devices/{deviceId}/allocation` | quem o Sync diz que está alocado no posto do aparelho agora |
| `POST /api/v1/matrix/devices/{deviceId}/presence` | abre sessão de presença no posto da matriz vinculado ao aparelho |
| `POST /api/v1/matrix/devices/{deviceId}/vacancy` | abre período de posto vazio no posto da matriz vinculado ao aparelho |
| `POST /api/v1/matrix/devices/{deviceId}/unknown` | abre período de rosto desconhecido no posto da matriz vinculado ao aparelho |
| `POST /api/v1/models` | cria modelo |
| `POST /api/v1/models/{modelId}/versions` | cria versão |
| `POST .../versions/{versionId}/upload-url` | inicia upload do artefato |
| `PUT .../versions/{versionId}/artifact` | envia o `model-package.zip` |
| `POST .../versions/{versionId}/complete` | conclui upload |
| `POST .../versions/{versionId}/approve` | aprova versão — **exige role `master`** |
| `POST .../versions/{versionId}/archive` | arquiva versão — **exige role `master`** |
| `POST .../versions/{versionId}/unarchive` | tira do arquivo, devolvendo ao estado anterior — **exige role `master`** |
| `DELETE .../versions/{versionId}` | apaga a versão e o artefato no storage — **exige role `master`** |
| `PUT /api/v1/models/{modelId}/active-version` | escolhe a versão que os clientes baixam — **exige role `master`** |
| `POST /api/v1/auth/logout` | encerra sessão |
| `GET /health` | relatório detalhado (expõe nomes de dependência e mensagens de erro) |

Ao mexer nas rotas, lembre que `AllowAnonymous` aqui é decisão de arquitetura, não
esquecimento: o catálogo e o `match` são consumidos por apps que não têm sessão de
usuário. O que grava continua exigindo token.

Dentro do que exige token há um segundo corte, por **role**: aprovar, arquivar, desarquivar,
excluir versão e trocar a versão ativa pedem
`master` (`AuthPolicies.MasterOnly`, `RequireRole("master")` sobre o claim `roles` do JWT
emitido no login). A tela de modelos do front só abre para `master`, e esconder a tela sem fechar a API
seria cosmético — quem tem a matrícula errada recebe **403** com o envelope de erro padrão.
Criar modelo, versionar e subir artefato seguem só com `[Authorize]`, porque são os passos da
pipeline de publicação (`iris-studio`), não governança.

`GET .../artifact` é anônimo pelo mesmo motivo do `download-url` que ele completa — todo
app baixa o modelo no boot, sem login. Como o endereço é derivado de ids que o catálogo
anônimo publica, é o backend que barra o que não pode sair: só versão **aprovada** tem
artefato servido. O `PUT` correspondente exige token **e** a versão em `uploading` — a
URL pré-assinada do MinIO expirava sozinha, um endereço fixo não, e sem a checagem de
estado qualquer usuário autenticado sobrescreveria o binário de uma versão já aprovada e
distribuída.

## Controle de acesso

Portado do `access-control-api` (que deixa de existir como serviço). Mesmas tabelas, mesmos
claims no JWT e mesmas rotas — só que sob `/api/v1` desta API. Código nos módulos `Auth`,
`Users`, `Roles`, `Departments` e `Collaborators` de cada camada.

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
| `POST /api/v1/users/not-authenticated` | anônimo, limite por IP | autocadastro: matrícula tem que existir no Sync; nasce sem role |
| `GET /api/v1/roles?page=0&pageSize=10`, `GET /api/v1/roles/all`, `GET /api/v1/roles/{id}` | token | |
| `POST`/`PUT`/`DELETE /api/v1/roles[/{id}]` | `master` | a role `master` não pode ser renomeada nem excluída; role em uso não exclui |
| `POST /api/v1/roles/set-role`, `POST /api/v1/roles/revoke-role` | `master` | `{ userId, roleId }`. Ninguém tira a `master` de si mesmo |
| `GET /api/v1/departments` | anônimo | **`string[]`** só com os nomes — formato que o SYNC/Q-Zero consome, não mudar |
| `GET /api/v1/departments/all` | anônimo | `{ id, name }[]` |
| `POST`/`PUT`/`DELETE /api/v1/departments[/{id}]` | `master` | nome sempre em minúsculas; departamento com usuário (mesmo desativado) não exclui |
| `GET /api/v1/colaboradores/get-by-registration/{registration}` | anônimo, limite por IP | repasse do Sync (`colaborator/get-by-registration`) |

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

Mesmo layout do banco `access_control`, agora no `iris`:

```sql
CREATE TABLE departments (
  id   INT          NOT NULL AUTO_INCREMENT,
  name VARCHAR(50)  NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY ix_departments_name (name)
);

CREATE TABLE roles (
  id          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  name        VARCHAR(100)    NOT NULL,
  description VARCHAR(255)    NOT NULL,
  PRIMARY KEY (id)
);

CREATE TABLE applications (
  id     BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  name   VARCHAR(20)     NOT NULL,
  active TINYINT(1)      NOT NULL DEFAULT 1,
  PRIMARY KEY (id),
  UNIQUE KEY ix_applications_name (name)
);

CREATE TABLE users (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  registration  INT UNSIGNED    NOT NULL,
  name          VARCHAR(255)    NOT NULL,
  email         VARCHAR(100)    NULL,
  password      VARCHAR(200)    NOT NULL,
  active        TINYINT(1)      NOT NULL DEFAULT 1,
  modified_at   DATETIME(6)     NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  modified_by   BIGINT UNSIGNED NOT NULL,
  fk_department INT             NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY ix_users_registration (registration),
  UNIQUE KEY ix_users_email (email),
  KEY ix_users_fk_department (fk_department),
  CONSTRAINT users_fk_department FOREIGN KEY (fk_department) REFERENCES departments (id) ON DELETE RESTRICT
);

CREATE TABLE user_roles (
  id      BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  fk_user BIGINT UNSIGNED NOT NULL,
  fk_role BIGINT UNSIGNED NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY ix_user_roles_fk_user_fk_role (fk_user, fk_role),
  KEY ix_user_roles_fk_role (fk_role),
  CONSTRAINT user_roles_fk_user FOREIGN KEY (fk_user) REFERENCES users (id) ON DELETE CASCADE,
  CONSTRAINT user_roles_fk_role FOREIGN KEY (fk_role) REFERENCES roles (id) ON DELETE CASCADE
);

CREATE TABLE user_applications (
  id             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  fk_user        BIGINT UNSIGNED NOT NULL,
  fk_application BIGINT UNSIGNED NOT NULL,
  PRIMARY KEY (id),
  KEY ix_user_applications_fk_user (fk_user),
  KEY ix_user_applications_fk_application (fk_application),
  CONSTRAINT user_applications_fk_user FOREIGN KEY (fk_user) REFERENCES users (id) ON DELETE CASCADE,
  CONSTRAINT user_applications_fk_application FOREIGN KEY (fk_application) REFERENCES applications (id) ON DELETE CASCADE
);

CREATE TABLE refresh_tokens (
  id      BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  token   VARCHAR(255)    NOT NULL,
  expires DATETIME(6)     NOT NULL,
  revoked TINYINT(1)      NOT NULL DEFAULT 0,
  fk_user BIGINT UNSIGNED NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY ix_refresh_tokens_token (token),
  KEY ix_refresh_tokens_fk_user (fk_user),
  CONSTRAINT refresh_tokens_fk_user FOREIGN KEY (fk_user) REFERENCES users (id) ON DELETE CASCADE
);
```

### Migração dos dados (uma vez)

Com os dois bancos no mesmo servidor (senão, `mysqldump` das tabelas e import no `iris`).
Antes, confira se a normalização de departamento não colide:
`SELECT LOWER(TRIM(name)), COUNT(*) FROM access_control.departments GROUP BY 1 HAVING COUNT(*) > 1;`

```sql
INSERT INTO iris.departments (id, name) SELECT id, LOWER(TRIM(name)) FROM access_control.departments;
INSERT INTO iris.roles (id, name, description) SELECT id, name, description FROM access_control.roles;
INSERT INTO iris.applications (id, name, active) SELECT id, name, active FROM access_control.applications;
INSERT INTO iris.users (id, registration, name, email, password, active, modified_at, modified_by, fk_department)
  SELECT id, registration, name, email, password, active, modified_at, modified_by, fk_department FROM access_control.users;
INSERT INTO iris.user_roles (id, fk_user, fk_role) SELECT id, fk_user, fk_role FROM access_control.user_roles;
INSERT INTO iris.user_applications (id, fk_user, fk_application) SELECT id, fk_user, fk_application FROM access_control.user_applications;
-- refresh_tokens não migra: os do AC guardam o valor que o cliente tem, e o formato mudou.
```

As senhas são BCrypt custo 12 nos dois lados — continuam valendo. Em ambiente novo, sem dados
para migrar, o primeiro `master` nasce por SQL: crie o usuário pelo autocadastro
(`POST /api/v1/users/not-authenticated`) e depois
`INSERT INTO user_roles (fk_user, fk_role) VALUES (<id do usuário>, <id da role master>);`.

## Matriz de Capacitação (Sync)

O tablet (`iris-mobile`) usa a Matriz de Capacitação do Sync de dois jeitos: o módulo **Matrix**
desenha o layout da linha e aloca operadores pelo rosto, e o módulo **Monitoring** confere se a
pessoa reconhecida no posto é a mesma que está alocada. O tablet **só fala com esta API**; quem
fala com o Sync é o `SyncMatrixClient` (porta `ISyncMatrixClient`), em `Sync:BaseUrl`.

- **A alocação mora no Sync, não aqui.** `POST /api/v1/matrix/allocations` relê o layout (para
  tirar `layoutId` e `activityId` do posto, sem confiar no cliente), troca a matrícula pelo id do
  colaborador (`GET colaborator/get-by-registration`) e chama o mesmo
  `POST training-matrix/setup-collaborator` que a tela do Sync usa. É o Sync que valida catraca e
  limite de máquinas, soma score e grava `employee_post_assigment`, histórico e presença.
- **Leitura:** `GET production-line`, `GET production-line-layout/configured-products` e
  `GET production-line-layout/get-by-line-and-product`. Esta última já devolve cada posto com o
  ocupante vigente: o backend do Sync junta, na leitura, o JSON do layout com
  `employee_post_assigment` — o JSON gravado não guarda colaborador.
- Posto sem `width`/`height` no layout sai com 220×140, o mesmo padrão do canvas do Sync.
- **O vínculo é por aparelho**, chaveado pelo `ANDROID_ID` do tablet (IMEI não é legível por
  app comum desde o Android 10). Guarda linha + produto + turno + `stationId`, porque no Sync o
  posto só existe dentro de um layout (linha + SKU + turno) e o id dele só é único ali.
- **Nome: station no Iris, post no Sync.** Código, rotas e JSON desta API usam `station`, como o
  resto do Iris. `post` só aparece onde é referência direta ao Sync: as colunas
  `sync_post_id`/`sync_post_name` guardam o `postId` do layout de lá, e as rotas do Sync
  chamadas pelo adaptador são as dele.
- Sync recusando (400/404, ex.: "Nenhum layout encontrado.") volta como **400** com o texto do
  Sync; Sync fora do ar ou sem resposta volta como **502**.

## Configuração local

Variáveis de ambiente ou `dotnet user-secrets`:

- `ConnectionStrings__Iris` — MySQL `iris`, a **única** base (ver "Base única" abaixo)
- `Sync__BaseUrl` — API do Sync, com o `/api` do nginx dele (padrão `https://sync.grupomultilaser.com.br/api/`)
- `Jwt__Key` — mínimo 32 bytes (HS256). É com ela que o IrisWeb **assina** os tokens; use a
  mesma do Access Control antigo enquanto Q-Zero/master-data validarem tokens com ela
- `Jwt__Issuer` (padrão `access-control`), `Jwt__Audiences__0..n` (padrão `q-zero`, `master-data`)
  — vão no token emitido e são os únicos aceitos na validação
- `Jwt__ExpirationInMinutes` (padrão 15), `Jwt__RefreshTokenExpirationInHours` (padrão 8)
- `Email__SmtpServer`, `Email__Port`, `Email__FromAddress`, `Email__FromName`, `Email__User`,
  `Email__Password` — SMTP do "esqueci a senha". Sem eles a API sobe, mas o reset anônimo
  recusa (a senha só seria entregue por e-mail)
- `RateLimiting__AnonymousCredentialsPerMinute` (padrão 30) — limite por IP nas rotas anônimas
  de credencial. Atrás de proxy sem `X-Forwarded-For` tratado, todo mundo vira o mesmo IP
- `ModelStorage__Path` — pasta dos artefatos (padrão `Data/model-artifacts`); **opcional**
- `Cors__Origins__0` — origens liberadas fora de Development (em Development qualquer origem passa)

O storage não tem mais credencial. Se você configurou `Minio__AccessKey`/`Minio__SecretKey`
no `dotnet user-secrets` deste projeto, eles ficaram **órfãos**: ninguém os lê, e podem ser
removidos quando der.

Não há banco local em nenhum ambiente. A API recusa subir sem `Jwt:Key` e sem
`ConnectionStrings:Iris`.

## Servindo a SPA (frontend)

Não existe mais processo Node em produção. `npm run build` do `../frontend` escreve direto
em `IrisWeb.API/wwwroot/` (`vite.config.ts`), e é esta API quem serve esse build: estáticos
via `UseStaticFiles`, e um `MapFallback` em `Program.cs` que devolve `wwwroot/index.html` para
`/` e qualquer rota de cliente (roteamento do React Router). Um runtime só, um processo só.

- **HTTPS em produção é desta API**, não mais de um servidor Node separado — a câmera
  (`getUserMedia`) exige contexto seguro. **Não existe `appsettings.Production.json`** — só um
  `appsettings.json`, igual em todo ambiente. O bind HTTPS/certificado de produção entra por
  variável de ambiente, no mesmo padrão de `Jwt:Key`/`ConnectionStrings:Iris`:

  ```
  Kestrel__Endpoints__Https__Url=https://0.0.0.0:8443
  Kestrel__Certificates__Default__Path=cert.pem
  Kestrel__Certificates__Default__KeyPath=key.pem
  ```

  Gere o par de certificado para o IP/host real do tablet (não versionado, gitignored):

  ```
  openssl req -x509 -newkey rsa:2048 -nodes -keyout key.pem -out cert.pem -days 825 \
    -subj "/CN=iris-pilot" -addext "subjectAltName=IP:<IP_DO_HOST>,IP:127.0.0.1,DNS:localhost"
  ```

  Rode dentro de `IrisWeb.API/` (ou aponte `Path`/`KeyPath` para caminho absoluto). Precisa
  instalar o certificado CA nos tablets também (ver `../frontend/CLAUDE.md`).
- Em `Development`, ninguém define essas três variáveis, então o Kestrel não tenta abrir
  `:8443` nem procurar `cert.pem` — `dotnet run --launch-profile http` continua servindo só a
  API (Swagger em `/`) na porta do `launchSettings.json`; a SPA nesse modo vem do Vite dev
  server (`:5173`), não daqui. `ASPNETCORE_ENVIRONMENT=Production` sozinho não muda o bind —
  quem muda são essas variáveis, e elas só devem existir no ambiente de produção.

### Base única

Uma base para tudo: `iris` no MySQL `10.1.5.70`. Catálogo de modelos
(`system_models`, `system_model_versions` — catálogo geral, não é so do face_detector, por isso
o nome não leva o prefixo do módulo), embeddings faciais (`face_detector_embeddings`),
configuração de estação e log de presença.

Os embeddings vivem em `iris.face_detector_embeddings` — **a mesma
tabela que o app Iris desktop usa** (`src/infrastructure/database/face_embedding_repository.py`,
credenciais nas variáveis `IRIS_DB_*` do `.env` dele).

Não existe modo local/SQLite para essa base, e isso é deliberado: um banco de teste separado
dividiria os cadastros em dois conjuntos que nunca se comparam — alguém cadastrado pelo web
não seria reconhecido pelo desktop, e o sintoma apareceria como "o reconhecimento não pega",
não como "estou no banco errado". Se `ConnectionStrings:Iris` não estiver configurado, a API
falha no boot em vez de degradar silenciosamente.

O `embedding` é o vetor float32 em bytes crus, little-endian, 4 bytes por posição (512
posições, L2-normalizado) — exatamente o que o numpy do desktop grava com `tobytes()` e lê com
`frombuffer`. `Buffer.BlockCopy` no C# produz o mesmo layout. Ao mexer nessa serialização,
os dois lados têm que mudar juntos.

### Triagem do cadastro (`enroll/batch`)

O cadastro em lote recebe todas as fotos de uma vez e confere antes de gravar, para não criar
cadastro que depois confunde uma pessoa com outra. Só `enrolled` grava; os outros resultados
voltam 200 sem gravar nada:

| `outcome` | quando | saída |
| --- | --- | --- |
| `inconsistent` | as fotos do lote ficam longe entre si (> `MaxSpread`, 0.7) | refazer as fotos |
| `same_person` | o rosto fica a menos de `SamePersonDistance` (0.5) de outra matrícula | bloqueado: conferir a matrícula; se for recontratação, apagar o cadastro antigo |
| `registration_mismatch` | a matrícula já tem cadastro e o rosto novo não bate com ele (> `DistanceThreshold`) | bloqueado: conferir a matrícula |
| `similar` | outra matrícula a menos de `SimilarDistance` (1.0) | refazer as fotos; se continuar, reenviar com `confirmSimilar: true` |
| `enrolled` | passou | — |

A distância para outra matrícula segue a regra do match (média das `MatchTopK` amostras mais
próximas) e fica com a pior foto do lote. Os limites ficam em `FaceRecognition:Enrollment`.
Cadastro parecido confirmado grava quem confirmou em `face_detector_enroll_confirmation`.
A rota antiga `POST /face-recognition/enroll` (uma foto por vez, sem triagem) continua para o
web e o desktop.

```
dotnet run --project IrisWeb.API --launch-profile http    # :5156, dev — SPA nao servida daqui
dotnet test IrisWeb.slnx
```

Para tablet/produção (SPA + HTTPS + API, tudo neste processo):

```
cd ../frontend && npm run build          # gera IrisWeb.API/wwwroot/
cd IrisWeb.API
ASPNETCORE_ENVIRONMENT=Production dotnet run     # :8443 https, ver "Servindo a SPA" acima
```

## Contratos e falhas

- `POST /api/v1/face-recognition/match` devolve `score` = distância euclidiana ao
  candidato mais próximo (menor = mais parecido) e **`null`** quando não existe nenhum
  embedding cadastrado para aquela `modelVersion`. Não use infinito: JSON não representa,
  e a serialização estourava 500.
- O corpo de sucesso é o **DTO cru** (`MatchFaceResponse`), nunca o `Result<T>` do
  FluentResults. `Ok(result)` em vez de `Ok(result.Value)` serializa
  `{ isSuccess, value: { ... } }`, o cliente lê `score`/`registration` como `undefined` e
  quebra na formatação. `FaceRecognitionControllerTests` trava esse contrato.
- `modelVersion` particiona os cadastros — só embeddings do mesmo valor são comparáveis.
  Quem gera o embedding declara esse valor (no web vem de `manifest.embeddingSpace`),
  então trocar o identificador órfã os cadastros existentes.
  Valor atual: `inception_resnet_v1_aligned` (pré-processamento `(pixel - 127.5) / 128` do
  facenet-pytorch + recorte quadrado com margem 0.2 e olhos nivelados). Anteriores:
  `inception_resnet_v1` usava normalização ImageNet por engano e colapsava o espaço;
  `inception_resnet_v1_fixed_std` corrigia a normalização mas usava a caixa crua do
  BlazeFace. Cadastros de versões antigas ficam órfãos de propósito e precisam ser refeitos.
  `DistanceThreshold` padrão é 0.85 (confiança 64%); 1.0 deixava estranhos parecidos passarem.
- Enums saem como string camelCase (`"faceRecognition"`, `"approved"`). É o formato que
  os clientes já consumiam quando o catálogo vivia no iris-core.
- Falha de dependência externa (disco cheio, volume de artefatos fora, timeout) responde
  **503** via `ApiResults.Failure`; `400` fica reservado para recusa de negócio. O cliente
  precisa dessa diferença para saber se pode tentar de novo ou cair em fallback.
- Erro sempre no formato `{ "error": "...", "details": "..." }`.
- `upload-url` e `download-url` devolvem **URL relativa** (`/api/v1/models/.../artifact`),
  não mais URL assinada de terceiro. O campo `expiresAtUtc` continua no corpo porque os
  clientes já o consomem, mas o que fecha a janela deixou de ser a URL: é o estado da
  versão. O corpo dos dois responses não mudou em mais nada.
- `enroll` grava direto na tabela de produção, com o mesmo `INSERT` do desktop (incluindo
  `embedding_dim`). Não há staging: um cadastro pelo web já vale para o desktop.

## Posto e log de presença

**O posto é o posto da Matriz de Capacitação do Sync.** Não existe mais "posto Iris": a
tabela `face_detector_station_config` deixou de ser mapeada, e as rotas que a usavam
(`GET/POST /api/v1/stations`, `POST /stations/presence`, `POST /stations/vacancy`) saíram.
A tabela continua no banco — ela nasceu no desktop, e apagar ou alterar tabela de outro app
não é chamada daqui.

Cada cliente (tablet ou navegador) é vinculado a um posto da matriz em
`face_detector_device_matrix` (ver "Matriz de Capacitação" acima), e a presença é aberta
**pelo aparelho**: `POST /api/v1/matrix/devices/{deviceId}/presence` e `/vacancy`.

- `face_detector_station_presence` — uma linha por período contínuo em que uma matrícula
  ficou no posto. Quando a presença abre, o backend grava um **retrato** tirado do Sync:
  `collaborator_name`, `training_level` (o nível na atividade do posto, como o Sync mostra) e
  `allocated_registration` (quem o Sync tinha alocado ali). O retrato é o que permite dizer
  depois "treinado ou não" e "era a pessoa alocada ou não": o nível muda com o tempo e a API do
  Sync não devolve o nível histórico. A consulta ao Sync tem teto de 3 s; passou disso, ou o
  Sync falhou, a presença abre sem retrato e aparece como "nível não registrado".

  As três colunas são criadas à mão (sem migration) e precisam existir antes do deploy: o
  mapeamento lê essas colunas, e sem elas toda leitura e gravação de presença falha.

  `GET /api/v1/stations/line-timeline?line=&date=&from=&to=` monta a linha do tempo da linha
  num dia (ou turno, com `from`/`to` em `HH:mm`; `to` ≤ `from` atravessa a meia-noite): posto a
  posto, os trechos de presença (com nome, nível, treinado/não treinado/não registrado e se
  batia com a alocação), posto vazio e rosto desconhecido, recortados na janela. Os totais da
  raiz usam tempo-posto (janela × postos). Treinado = Qualificado ou Multiplicador
  (`TrainingLevelPolicy`).
- `face_detector_station_vacancy` — uma linha por período de posto vazio.
- `face_detector_station_unknown` — uma linha por período em que **havia um rosto no posto, mas
  ninguém foi reconhecido**. Separa "posto vazio" de "tem gente sem cadastro" ou "o cadastro de
  alguém não está reconhecendo". Guarda o diagnóstico: `candidate_registration` e
  `candidate_distance` são a matrícula mais parecida e a menor distância vistas no período
  (o `match` passou a devolver `candidate` mesmo quando recusa). Distância perto do
  `DistanceThreshold` com o mesmo candidato repetido é sinal de cadastro ruim dessa matrícula.

A timeline devolve os três tipos (`presence`, `vacancy`, `unknown`) e soma cada um por dia
(`occupiedSeconds`, `vacantSeconds`, `unknownSeconds`). Filtrar por matrícula traz só presença.

As duas guardam o posto como texto, e continuam guardando: `line` = linha do Sync
(`N111`) e `workstation` = `#<id do posto no Sync> <NOME>` (`#14 PRENSA BATERIA`), os dois
normalizados (`StationNamingPolicy.NormalizeField`: sem borda, espaços colapsados, caixa
alta, cortado em 64 — a largura da coluna). Timeline e relatórios agrupam por esse par.

Duas decisões que valem a pena não desfazer:

1. **O cliente nunca manda posto/linha ao abrir presença** — manda o aparelho, e o backend
   tira os dois do vínculo com a matriz. Aceitar os campos do cliente deixaria cada tela
   gravar um rótulo diferente para o mesmo posto.
2. **Fechar manda uma duração, não um timestamp.** `ClosePresenceRequest.GraceSeconds` é há
   quantos segundos a pessoa foi vista pela última vez; a sessão fecha no *último visto* e a
   duração não incha com a tolerância de ausência.

Fechar uma sessão já fechada **é sucesso**, não erro: o cliente reenvia depois de recarregar,
e um `400` viraria alarme falso por algo que já tinha dado certo.

## Um formato por alvo: web e mobile são ONNX, desktop é IR

Todo modelo declara `platform` na criação (`web`, `mobile` ou `desktop`), e o alvo fixa o
formato do artefato. É padrão de arquitetura da plataforma, não preferência: o navegador só
executa ONNX (ONNX Runtime Web) e o desktop só executa IR (OpenVINO). A pipeline de treino
exporta para o formato do alvo; o catálogo recusa o que não casa.

| `platform` | runtime da versão | `manifest.format` | `manifest.entrypoint` |
| --- | --- | --- | --- |
| `web` | `onnxruntime-web` | `onnx` | termina em `.onnx` |
| `mobile` | `onnxruntime-web` | `onnx` | termina em `.onnx` |
| `desktop` | `openvino` | `ir` | termina em `.xml` (o `.bin` vai junto no pacote) |

**`mobile` é o navegador do tablet e compartilha tudo com `web` de propósito.** O que o separa não
é formato, é memória: o ONNX Runtime Web copia o modelo inteiro para o heap do WASM antes de criar
a sessão, e o FaceNet fp32 de 94 MB estoura em tablet (`failed to allocate a buffer of size
93959246`). O alvo existe para publicar a variante quantizada (int8, ~24 MB) sob chave própria
(`facenet-embeddings-mobile`), com o **mesmo `embeddingSpace`** do web e do desktop — os três
gravam e leem a mesma tabela de embeddings. O módulo face-detector do front consome a chave
mobile em todo navegador; `facenet-embeddings` (web, fp32) segue `approved` no catálogo, sem
consumidor. `ModelPlatformPolicyTests.MobileCompartilhaMotorEFormatoComWeb` trava essa igualdade.

**Um valor por alvo, não um conjunto de sinônimos.** Houve uma versão desta regra que aceitava
`onnx`, `onnxruntime` e `onnxruntime-web` como equivalentes, para não recusar o que já estava
gravado. O efeito foi o oposto do pretendido: três grafias para um motor só, e a tela do
catálogo mostrando dois nomes diferentes para o mesmo ONNX Runtime Web. As linhas foram
normalizadas à mão e a regra voltou a ter um valor só.

`runtime` nomeia o **motor** e `format` nomeia o **arquivo**. Por isso web é `onnxruntime-web` e
não `onnx`: o formato é que é `onnx`, e são colunas distintas. Web e desktop não compartilham
valor nenhum, porque um ONNX nunca serve ao desktop e um IR nunca serve ao navegador.
`ModelPlatformPolicyTests` trava os valores e essa disjunção — mexer neles é mudar contrato com
quem publica e com o que está no banco, não manutenção de teste.

Hoje **todo modelo do catálogo é `web`**; não há desktop publicado. O ramo desktop existe na
regra, mas ainda não foi exercitado por dado real.

A regra vive em `IrisWeb.Domain/Rules/ModelPlatformPolicy.cs` e é aplicada em dois pontos:
`POST .../versions` recusa runtime de outro alvo (400), e `POST .../complete` recusa manifesto
com `format` ou `entrypoint` de outro alvo (400) — antes de a versão virar candidata, não depois
de alguém aprovar. Campo ausente no manifesto não é violação; declarado errado é. Comparação
ignora caixa e espaço nas bordas.

Um mesmo modelo lógico que precise rodar nos dois alvos é **dois modelos** no catálogo (duas
chaves, uma por `platform`), porque os clientes resolvem por chave e um cliente web nunca pode
receber um IR. O `ModelRegistry` do front recusa carregar modelo que não seja `web` ou `mobile`.

A coluna `platform` é `VARCHAR(16)` e grava o nome do enum (`Web`, `Desktop`, `Mobile`); o valor
novo cabe sem alteração de schema.

A coluna é nova e criada à mão, como o resto do schema (ver "Schema" abaixo):

```sql
ALTER TABLE system_models
  ADD COLUMN platform VARCHAR(16) NOT NULL DEFAULT 'Web' AFTER task_type;

-- As versões existentes usavam `onnxruntime-web` e `onnxruntime` para o mesmo motor.
UPDATE system_model_versions v
  JOIN system_models m ON m.id = v.model_id
   SET v.runtime = 'onnxruntime-web'
 WHERE m.platform = 'Web';
```

O `DEFAULT 'Web'` só serve para preencher as linhas que já existem — o EF sempre grava o
valor. Modelo antigo que na verdade é do desktop precisa de
`UPDATE system_models SET platform = 'Desktop' WHERE model_key = ...`. Sem a coluna,
`GET /api/v1/models` cai em 500 na primeira consulta.

A escolha de versão ativa (ver "Ciclo de vida da versão") também é coluna criada à mão:

```sql
ALTER TABLE system_models
  ADD COLUMN active_version_id CHAR(36) NULL AFTER platform;
```

Nula é estado válido — significa "ninguém escolheu ainda", e o cliente cai na aprovada mais
recente. Sem FK de propósito: `system_model_versions` já referencia `system_models` com
`ON DELETE CASCADE`, e uma FK de volta criaria ciclo; quem garante que o ponteiro aponta para
uma versão aprovada do próprio modelo são os use cases. Sem a coluna,
`GET /api/v1/models` cai em 500 na primeira consulta.

## Fluxo de publicação de modelo

Continuam sendo sete passos; o que mudou é para onde vai o passo 4.

1. criar um modelo;
2. criar uma versão em `draft`;
3. solicitar uma URL de upload, movendo-a para `uploading`;
4. enviar `model-package.zip` **para a própria API**, com `PUT` autenticado no endereço
   devolvido no passo 3 (era um `PUT` sem `Authorization` direto no MinIO);
5. confirmar o upload, movendo-a para `candidate`;
6. aprovar a versão;
7. solicitar o endereço de download para distribuição.

**Quem executa esses sete passos é o `iris-studio`**, o app que treina e publica. Este
backend não publica nada: ele guarda o catálogo, serve os artefatos e expõe a API que o
Studio consome. Hoje o Studio treina e exporta para disco, mas ainda não publica — essa
ponte está por fazer, do lado dele.

Divisão de responsabilidade da plataforma, para não procurar o código no lugar errado:

| app | papel |
| --- | --- |
| `iris-studio` | treina e **publica** modelo |
| `iris-web` (este) | **gerencia** o catálogo, serve os artefatos, roda inferência no navegador |
| `iris-desktop` | inferência com OpenVINO na linha; baixa modelo daqui para se atualizar |

O token dos passos autenticados sai do login desta API (`POST /api/v1/auth` com matrícula e
senha). Não existe atalho, e não deve existir: houve um `mint_dev_jwt.py` que assinava o
próprio JWT com a chave de produção, e a API o aceitava por causa de um audience que só servia
para isso. Assinar token fora do fluxo de login é backdoor, não ferramenta.

`Jwt:Audiences` continua sendo **exatamente** `q-zero` e `master-data`: o token emitido aqui
precisa continuar valendo nesses sistemas, que validam com a mesma chave. Audience a mais só
aumenta o que a API aceita.

Duas coisas mudaram para quem escreve cliente:

- **A URL dos passos 3 e 7 é relativa.** Resolva-a contra a base da API (`api_url + url`)
  quando ela não for absoluta.
- **O `PUT` do passo 4 precisa de `Authorization`.** Antes a assinatura da URL *era* a
  credencial, e mandar o token quebrava a assinatura; agora o endereço é fixo e público, e
  quem autoriza é o token.

## Ciclo de vida da versão: aprovar não é ativar

Publicar é do Studio; **escolher o que roda é daqui**. São coisas separadas de propósito:

| ação | o que significa |
| --- | --- |
| aprovar | a versão passou na revisão e **pode** ser escolhida |
| **ativar** (`PUT .../active-version`) | é **esta** que os clientes baixam |
| arquivar | sai da prateleira do que pode ser escolhido; o artefato continua no servidor |
| desarquivar | volta para a prateleira, no estado que tinha antes (`Approved`, `Candidate` ou `Draft`) |
| excluir versão | apaga o registro **e o arquivo**; não tem volta |

Antes, "em uso" era um efeito colateral: o cliente pegava a **aprovada mais recente**, então
aprovar já trocava a produção e a única forma de voltar era arquivar ou republicar o mesmo
artefato com outro número. Agora `system_models.active_version_id` guarda a escolha, e ela é
explícita.

Três regras que o domínio garante, e que existem para não derrubar produção sem querer:

- **só versão `Approved` do próprio modelo** pode ser ativada — arquivada nunca, desarquive
  antes (`AiModel.SetActiveVersion`);
- **a versão ativa não pode ser arquivada nem excluída**. Tirar algo de produção é ativar
  outra coisa, não apagar o que está rodando;
- **sem escolha, vale o comportamento antigo**: `active_version_id` nulo faz o cliente cair na
  aprovada mais recente. É o que mantém funcionando o que foi publicado antes desta coluna.

`CoreModelCatalog.latestApprovedVersion` (front) e `approvedVersionOf` (tela) implementam
essa mesma ordem — escolha explícita, senão a última aprovada. Se as duas divergirem, a tela
mostra "em uso" uma versão que o app não usa.

O `DELETE` de versão apaga o arquivo **antes** da linha. Se o storage falhar, a linha fica e a
resposta é 503: um registro apontando para um arquivo que ainda existe é recuperável; um
arquivo órfão, não — o storage só é enumerado pelo banco.

## Onde os artefatos ficam

Numa pasta: `ModelStorage:Path`, padrão `Data/model-artifacts`, criada no boot se não
existir. Um arquivo por versão, em `{model.Key}/{version}/model-package.zip`. A gravação
passa por um `.part` e só move para o nome final quando o corpo inteiro chegou — um upload
de 90 MB interrompido não pode deixar um pacote truncado que engana o `complete`.

Isso substituiu o MinIO, e a razão é a mesma que trouxe o `iris-core` para cá: o MinIO era
uma segunda aplicação para subir, monitorar e credenciar, e o que ela guardava era um zip
por versão de modelo.

**O que isso cobra, e é honesto dizer antes de descobrir:**

- a pasta precisa **sobreviver a deploy** — volume montado, não diretório do build;
- o **backup passa a ser dela**. Os artefatos não estão em mais lugar nenhum, e o banco
  guarda só o caminho e o `sha256`;
- **duas instâncias atrás de um balanceador teriam cada uma a sua cópia**: a que recebeu o
  `PUT` tem o pacote, a outra responde "artefato não encontrado no storage" no download. Se
  um dia for preciso escalar horizontalmente, volta a ser preciso storage compartilhado
  (volume de rede ou object store). Hoje é uma instância só, e é por isso que a troca
  compensa.

**O efeito colateral bom** está documentado como problema aberto no `frontend/CLAUDE.md`:
com o artefato saindo da mesma origem do site, o download deixa de ser cross-origin. Some o
bloqueio de conteúdo misto (página em HTTPS baixando de um MinIO em HTTP) e, principalmente,
**COOP/COEP deixam de ter impedimento** — `Cross-Origin-Embedder-Policy: require-corp`
bloquearia o download do MinIO enquanto ele não mandasse `Cross-Origin-Resource-Policy`, e
era esse o motivo de os headers nunca terem sido ligados. Ligá-los destrava
`crossOriginIsolated`, que é o que o wasm com threads do ONNX Runtime precisa.

## Schema: criado à mão, sem migrations

Não há migrations neste repositório, e isso é deliberado. O banco `iris` é de produção e
nasceu fora daqui — o app desktop é o dono das tabelas `face_detector_*`. Uma migration
tentaria versionar um schema que não é nosso e, na primeira divergência, proporia
`DROP`/`ALTER` em tabelas de produção.

As tabelas do controle de acesso (`users`, `roles`, `user_roles`, `departments`,
`applications`, `user_applications`, `refresh_tokens`) vieram do banco `access_control` e
**têm DDL aqui** — ver "Controle de acesso → Schema".

As tabelas do catálogo (`system_models`, `system_model_versions`) foram
criadas à mão e **não há DDL versionado neste repositório** — houve um
`scripts/catalog_mysql.sql`, removido junto com a pasta. Para recriar em outro ambiente,
extraia do banco com `SHOW CREATE TABLE`. O DDL das tabelas do desktop vive nos docstrings
dele (`station_config_repository.py`, `station_presence_repository.py`).

Como consequência, **o mapeamento EF é o único elo entre código e schema**: cada coluna é
nomeada à mão com `HasColumnName` nas `IEntityTypeConfiguration`, porque a tabela é
snake_case e o EF sozinho usaria PascalCase. Errar um nome só aparece em runtime — depois de
mexer no mapeamento, suba a API e chame `GET /api/v1/models`, que faz um SELECT de todas as
colunas mapeadas das duas tabelas.

Houve um catálogo em Postgres (herdado do iris-core) e um SQLite de desenvolvimento. Os dois
foram removidos: o Postgres nunca teve dados e o SQLite era pior que inútil — ficava vazio, o
`ModelRegistry` do front nunca achava versão aprovada e caía no artefato embarcado, então o
caminho de distribuição de modelo nunca era exercitado. O sintoma aparecia como "o badge diz
embarcado", não como "o catálogo está vazio".

O stack EF fica em `9.0.x` porque o provider MySQL (Pomelo 9.0.0) depende de EF Core 9; os
pacotes rodam sem problema no `net10.0`.
