Portfólio Profissional com API, Login e Certificados

Projeto pessoal, com foco em autenticação, exibição de informações profissionais e certificados. Inclui backend em ASP.NET Core 8, frontend em React 18, banco de dados PostgreSQL, testes automatizados e containerização com Docker Compose.

Tecnologias Utilizadas

| Camada         | Tecnologia                    |
| -------------- | ----------------------------- |
| Backend        | ASP.NET Core 8 (C#)           |
| Frontend       | React 18 + Vite + TailwindCSS |
| Banco de Dados | PostgreSQL 16                 |
| Autenticação   | JWT (Json Web Token)          |
| Testes de API  | NUnit                         |
| Testes E2E     | Cypress                       |
| Containers     | Docker + Docker Compose       |

--------------

portfolio-raynara/

├── backend/         #API em .NET

├── frontend/        #frontend em React

├── database/        # Script init.sql com estrutura do banco

├── docker-compose.yml

└── README.md

--------------
-Como Executar o Projeto Localmente

Pré-requisitos:

Docker Desktop instalado

.NET SDK 8.0+

Node.js 18+ (quando formos criar o frontend)

(Opcional) Visual Studio Code

--------------

Para iniciar os containers:

No terminal, dentro da pasta do projeto:

docker-compose up --build

A API estará disponível em: http://localhost:5000

O Frontend estará disponível em: http://localhost:3000

O banco PostgreSQL estará escutando na porta: 5432

--------------
Acesso de Teste (inicial)

Os dados abaixo são criados pelo script init.sql no container do PostgreSQL.

Campo	Valor
Email	ray@teste.com
Senha	123456