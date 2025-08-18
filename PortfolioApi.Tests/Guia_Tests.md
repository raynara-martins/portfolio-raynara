# Guia Completo dos Testes (Unit & Integração)

**Projeto:** `PortfolioApi.Tests`  
**Frameworks:**  
- NUnit (test runner)  
- FluentAssertions (asserts fluentes)  
- Microsoft.AspNetCore.Mvc.Testing (host in-memory)  
- EF Core InMemory (banco isolado nos testes)  

---

## Por que separar Unit e Integration?

- **Unit:** testa uma unidade isolada (ex.: `AuthService`), usando dublês (mocks/fakes). Sem rede, sem banco real.  
- **Integration:** testa o sistema real “de fora pra dentro”: sobe a API em memória, usa `HttpClient`, chama endpoints reais, DB InMemory, middleware (auth/CORS), controllers, services e repositories — fluxo completo.

---

## WebApplicationFactory & ambiente “Testing”

**Arquivo:** `Integration/CustomWebAppFactory.cs`

**O que é?**  
`WebApplicationFactory<Program>` inicializa a API em processo local (sem abrir porta), devolvendo um `HttpClient` que interage com a pipeline real (middlewares, controllers, DI etc.). É a base dos testes de integração.

### Como funciona no nosso projeto

1. **Ambiente de teste e JWT**  
   - Forçamos `builder.UseEnvironment("Testing")` para carregar configurações específicas desse ambiente.  
   - Configuramos `Issuer`, `Audience` e `Secret` do JWT usando `AddInMemoryCollection`.  
   - Isso evita depender de `appsettings.json` e garante que todos os tokens gerados no teste sejam válidos para os endpoints protegidos.

2. **Banco de dados em memória**  
   - Removemos o `DbContext` original (Npgsql) e registramos o EF Core InMemory (`UseInMemoryDatabase("tests-db")`).  
   - O banco é recriado e povoado (seed) a cada teste com um usuário de teste (email/senha conhecidos).  
   - Isso garante isolamento entre cenários e elimina dependência de infraestrutura externa.

**Por que InMemory?**  
- Mais rápido e previsível.  
- Evita erros por indisponibilidade de serviços como Postgres/Docker.  
- Mantém o foco no comportamento da API sem gargalos de rede.



---

## Auth end-to-end nos testes

Fluxo coberto em `AuthControllerTests.cs`:

1. **Login** via `POST /auth/login` (body JSON com email/senha)  
   - Espera **200 OK** e um JWT válido no corpo.
2. **/me** via `GET /me` com `Authorization: Bearer <token>`  
   - Espera **200 OK** com os dados do usuário.

Esse fluxo prova: controller + service + repo + autenticação JWT + claims + `[Authorize]` no endpoint.

---

## Setup & Teardown (NUnit)

No arquivo `AuthControllerTests.cs`:

```csharp
[SetUp]
public void Setup() {
    _factory = new CustomWebAppFactory();
    _client  = _factory.CreateClient();
}

[TearDown]
public void Teardown() {
    _client?.Dispose();
    _factory?.Dispose();
}
```

- **[SetUp]**: roda antes de cada teste — cada teste começa “limpo”, com `HttpClient` novinho e DB InMemory re-semeado.  
- **[TearDown]**: roda depois de cada teste — libera recursos, evitando vazamento de conexões/memória.

Esse ciclo garante independência entre testes.

---

## Por que os métodos de teste são `public async Task`?

- **public**: o runner (NUnit) precisa acessar o método.  
- **async Task**: testes chamam endpoints HTTP assíncronos (`await httpClient.PostAsync(...)`).  
  Isso evita deadlocks e garante que o NUnit espere a conclusão.

Padrão:
```csharp
[Test]
public async Task NomeDoCenario_Deve_FazerAlgo() { ... }
```

---

## Padrão AAA (Arrange–Act–Assert)

Exemplo:
```csharp
// Arrange
var payload = new { email = "ray@teste.com", password = "123456" };
var json    = JsonContent.Create(payload);

// Act 1: login
var login = await _client.PostAsync("/auth/login", json);
login.StatusCode.Should().Be(HttpStatusCode.OK);

var body  = await login.Content.ReadFromJsonAsync<LoginResponse>();
body!.token.Should().NotBeNullOrEmpty();

// Act 2: chama /me autenticado
_client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", body.token);

var me = await _client.GetAsync("/me");

// Assert
me.StatusCode.Should().Be(HttpStatusCode.OK);
var user = await me.Content.ReadFromJsonAsync<UserResponse>();
user!.email.Should().Be("ray@teste.com");
```

- **Arrange**: prepara dados.  
- **Act**: executa requisições.  
- **Assert**: valida resultados.

---

## FluentAssertions — por que usar?

- Sintaxe legível e mensagens claras:
```csharp
login.StatusCode.Should().Be(HttpStatusCode.OK);
body.token.Should().NotBeNullOrEmpty();
```
- Quando falha, mostra o esperado vs. o obtido.

---

## JWT nos testes — detalhe importante

- No login, a API gera JWT com `ClaimTypes.Name = email`.  
- Configuração em `Program.cs`:
```csharp
NameClaimType = ClaimTypes.Name
```
- `/me` usa `User.Identity.Name` para saber quem está logado.  
- Nos testes, após login:
```csharp
_client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
```
Sem isso, `/me` retorna **401 Unauthorized**.

---

## Convenções de nomes de teste

Formato:  
`MetodoOuFluxo_CondicaoEsperada_ResultadoEsperado`

Ex.:  
`Login_Then_Me_Should_Return200_AndUser`

---

## Dicas finais

- Rode `dotnet test -v n` para verbosidade normal.  
- Depure usando breakpoints (o `WebApplicationFactory` funciona bem no Debug).  

## Pacotes Utilizados:
dotnet add PortfolioApi.

- Tests package Microsoft.AspNetCore.Mvc.Testing
- Tests package Microsoft.EntityFrameworkCore.InMemory
- Tests package FluentAssertions
- Tests package System.Net.Http.Json
- Tests package BCrypt.Net-Next
- new tool-manifest
- tool install dotnet-reportgenerator-globaltool
- dotnet add PortfolioApi.Tests package coverlet.collector

---
# Plano de Cobertura de Testes — PortfolioApi

**Meta:** subir de **78% (line)** / **44% (branch)** para **≥ 85% / ≥ 70%**.

> **Padrão de nomes**
> - Unit: `Classe_Metodo_Cenario_Resultado()`
> - Integração: `Rota_Cenario_Status_Efeito()`

---

## 1) AuthService (Unit)

**Arquivo:** `backend/Services/AuthService.cs`  
**Branches:** usuário não encontrado; senha em texto vs hash; geração de JWT.

- [ ] `AuthService_LoginAsync_UserNotFound_ReturnsFalseNull()`
- [ ] `AuthService_LoginAsync_PlainPassword_Match_ReturnsTokenAndUser()`
- [ ] `AuthService_LoginAsync_PlainPassword_Wrong_ReturnsFalseNull()`
- [ ] `AuthService_LoginAsync_HashedPassword_Match_ReturnsTokenAndUser()`  *(seed com BCrypt)*
- [ ] `AuthService_LoginAsync_HashedPassword_Wrong_ReturnsFalseNull()`
- [ ] `AuthService_GenerateJwt_IncludesNameAndEmailClaims()`  *(Name/sub/email)*
- [ ] `AuthService_LoginAsync_TokenHasIssuerAudienceFromConfig()`

> **Ganho de branch:** cobre ambos caminhos de verificação de senha e retornos de erro.

---

## 2) UsersService / UserRepository (Unit)

**Objetivo:** retorno **nulo** e **com dados**.

- [ ] `UserService_GetByEmailAsync_NotFound_ReturnsNull()`
- [ ] `UserService_GetByEmailAsync_Found_ReturnsUser()`

---

## 3) CertificatesController / CertificateRepository

### Unit (mock de repo)
- [ ] `CertificatesController_GetMine_UserHasNone_ReturnsEmptyList()`
- [ ] `CertificatesController_GetMine_UserHasMany_ReturnsList()`

### Integração (pipeline real)
- [ ] `GET_/certificates/mine_Unauthorized_401()`
- [ ] `GET_/certificates/mine_Authorized_NoData_200EmptyArray()`
- [ ] `GET_/certificates/mine_Authorized_WithData_200AndItems()`

> **Ganho de branch:** cobre `if` interno para 0 itens vs N itens.

---

## 4) AuthController (Integração)

- [x] `POST_/auth/login_ValidCredentials_200_ReturnsTokenAndUser()` **(já coberto)**
- [x] `POST_/auth/login_InvalidPassword_401()`
- [x] `POST_/auth/login_UnknownEmail_401()`
- [ ] `POST_/auth/login_MissingFields_400_FromValidator()` *(email/senha vazios)*

---

## 5) /me (UsersController — Integração)

- [ ] `GET_/me_NoToken_401()`
- [ ] `GET_/me_InvalidToken_401()`
- [x] `GET_/me_ValidToken_200_ReturnsUser()` **(já coberto)**  
- [ ] `GET_/me_ValidToken_ButUserNotFound_404()` *(token com email inexistente)*

> **Ganho de branch:** cobre `if`s de `Name` vazio e `user == null`.

---

## 6) Validators (Unit)

**LoginRequestValidator**
- [ ] `LoginRequestValidator_EmailEmpty_Invalid()`
- [ ] `LoginRequestValidator_PasswordEmpty_Invalid()`
- [ ] `LoginRequestValidator_EmailMalformed_Invalid()`
- [ ] `LoginRequestValidator_Valid_OK()`

---

## 7) Program/Auth Config (Integração negativa)

**Cenários de bearer/token malformado**
- [ ] `Any_Endpoint_BearerMissingScheme_401()` *(ex.: `Authorization: Token x`)*
- [ ] `Any_Endpoint_BearerButEmptyToken_401()`
- [ ] `Any_Endpoint_TokenWithWrongAudience_401()` *(aud/iss/secret divergentes)*

> **Dica:** gere token inválido com secret diferente ou `aud` errado via helper no teste.

---

## 8) Repositórios (Unit — opcional)

- [ ] `CertificateRepository_GetByUserEmail_NoData_ReturnsEmpty()`
- [ ] `CertificateRepository_GetByUserEmail_WithData_ReturnsList()`

*(A camada já é exercitada nos testes de integração com EF InMemory; unit aqui é bônus.)*

---

## 9) Gaps apontados pelo Report

Focar nos pontos com **maior Crap Score** / **baixa branch**:
- [ ] `CertificatesController.GetMine()` — adicionar cenários vazio/erro.
- [ ] Classes com **Line < 90%** e **Branch < 70%** no relatório.

---

## 10) Metas & Acompanhamento

- [ ] **Meta 1:** Branch ≥ **55%**
- [ ] **Meta 2:** Branch ≥ **65%**
- [ ] **Meta final:** Branch ≥ **70%** e Line ≥ **85%**

**Como medir:**
```bash
# Coletar cobertura (Coverlet collector)
dotnet test PortfolioApi.Tests `
  --collect:"XPlat Code Coverage" `
  --results-directory "PortfolioApi.Tests/TestResults/coverage"

# Gerar relatório HTML
dotnet tool run reportgenerator `
  -reports:"PortfolioApi.Tests/TestResults/coverage/**/coverage.cobertura.xml" `
  -targetdir:"PortfolioApi.Tests/TestResults/coverage-report"

# Abrir
start PortfolioApi.Tests/TestResults/coverage-report/index.html
