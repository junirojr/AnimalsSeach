# Testes — Backend (Buscador)

## Padrão de nomenclatura
`Método_Cenário_ResultadoEsperado`
```csharp
// Correto
public void Create_WithValidData_ReturnsAnimal() { }
public void Search_WithEmptyQuery_ThrowsValidationException() { }

// Errado
public void TestCreate() { }
public void ShouldWork() { }
```

## Projetos e ferramentas

| Projeto | Tipo | Ferramentas |
|---------|------|-------------|
| `Buscador.Domain.Tests` | Unitário | xUnit + FluentAssertions |
| `Buscador.Application.Tests` | Unitário | xUnit + FluentAssertions + Moq |
| `Buscador.Api.Tests` | Integração | xUnit + Testcontainers.PostgreSql + WebApplicationFactory |

## Testcontainers
- `ApiTestFixture` sobe container `pgvector/pgvector:pg16` real (não mock)
- Aplica migrations automaticamente antes dos testes
- Expõe `HttpClient` via `WebApplicationFactory` apontando para o container
- **Pré-requisito**: Docker deve estar rodando (`docker compose up -d`)

## Moq (Application.Tests)
- Mockar `IAnimalRepository`, `IFullTextSearchService`, etc.
- Nunca mockar o banco — use Testcontainers para isso

## Cobertura esperada
- Domain: regras de negócio (Create válido/inválido, VO equality)
- Application: handlers com repositório mockado
- Api: endpoints (seed, search, get by id, erros de validação)