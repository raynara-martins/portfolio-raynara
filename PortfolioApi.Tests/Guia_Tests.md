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
