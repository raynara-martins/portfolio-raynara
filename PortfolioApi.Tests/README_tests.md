# Testes Automatizados – PortfolioApi

Módulo responsável pelos testes automatizados do projeto **PortfolioApi**, garantindo a qualidade e confiabilidade da API.

Inclui testes **unitários** e **de integração**, validados com NUnit e executados via `dotnet test`.  

---

## Tecnologias Utilizadas

| Tipo de Teste     | Tecnologia/Ferramenta        |
| ----------------- | ---------------------------- |
| Framework de Teste| NUnit                        |
| Asserts           | FluentAssertions             |
| Mock de Dados     | Banco InMemory (EF Core)     |
| Cobertura         | Microsoft.NET.Test.Sdk       |

---

## Estrutura de Pastas

PortfolioApi.Tests/
├── Unit/ # Testes unitários (Services, Repositories, etc.)

├── Integration/ # Testes de integração (Controllers e fluxo real da API)

│ └── Api/ # Testes para endpoints específicos

│ ├── AuthControllerTests.cs

│ ├── UserControllerTests.cs

│ └── CertificateControllerTests.cs

├── appsettings.Testing.json # Configurações para rodar em ambiente de teste

└── Usings.cs # Imports globais para testes



---

## Como Executar os Testes

### Pré-requisitos
- **.NET SDK 8.0 ou superior** instalado
- Ambiente configurado para rodar a API com **banco InMemory** no modo `Testing`

### Comandos

No terminal, dentro da pasta raiz do projeto (`portfolio-raynara`):

```bash
dotnet clean
dotnet build
dotnet test
