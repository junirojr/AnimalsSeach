# Backend — Buscador (Deep Sparrow)

## Regra de dependência (NUNCA viole)
```
Domain  ←  Application  ←  Infrastructure  ←  Api
```
- `Domain`: zero dependências externas. Só regras de negócio.
- `Application`: depende só de `Domain`. Interfaces, MediatR, FluentValidation.
- `Infrastructure`: implementa interfaces. EF Core, Npgsql, pgvector, Ollama.
- `Api`: camada HTTP. Endpoints Minimal API, sem lógica de negócio.

## Estrutura de projetos
```
src/
  Buscador.Domain/          # Entidades, VOs, interfaces de repositório
  Buscador.Application/     # CQRS (Queries + Commands + Handlers), DTOs
  Buscador.Infrastructure/  # EF Core, migrations, repositórios, serviços
  Buscador.Api/             # Program.cs, Endpoints/, Contracts/
tests/
  Buscador.Domain.Tests/        # xUnit + FluentAssertions
  Buscador.Application.Tests/   # xUnit + Moq + FluentAssertions
  Buscador.Api.Tests/           # Testcontainers + WebApplicationFactory
```

## Pacotes principais
| Camada | Pacotes |
|--------|---------|
| Infrastructure | EF Core 10, Npgsql.EF 10, Pgvector, Microsoft.Extensions.AI.Ollama |
| Application | MediatR, FluentValidation |
| Api | Scalar.AspNetCore |
| Testes | xUnit, FluentAssertions, Moq, Testcontainers.PostgreSql |

## Comandos (rodar em `backend/`)
```powershell
dotnet build
dotnet test
dotnet ef migrations add <Nome> --project src/Buscador.Infrastructure --startup-project src/Buscador.Api
dotnet ef database update --project src/Buscador.Infrastructure --startup-project src/Buscador.Api
```

## Padrão de nomenclatura de testes
`Método_Cenário_ResultadoEsperado`
Ex.: `Create_WithEmptyName_ThrowsArgumentException`