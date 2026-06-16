# Buscador.Infrastructure

## Responsabilidade
Implementar os contratos definidos no Domain e Application.
Aqui ficam banco de dados, migrations, repositórios e serviços externos (Ollama).

## EF Core + PostgreSQL
- `AppDbContext` com `DbSet<Animal> Animals`
- `AnimalConfiguration` (Fluent API): mapeia tabela `animals`, shadow properties
- **Shadow properties**: `search_vector` (tsvector) e `embedding` (vector(768))
  ficam AQUI — o Domain não sabe da existência delas
- Migrations geradas com `dotnet ef migrations add` (ver backend/CLAUDE.md)

## pgvector
- `UseVector()` no `AppDbContext`
- `HasPostgresExtension("vector")` no `OnModelCreating`
- Índice HNSW para busca por cosine distance (`<=>`)

## Ollama / Embeddings
- `OllamaEmbeddingService` via `Microsoft.Extensions.AI.Ollama`
- Modelo: `bge-m3` (1024 dimensões, multilíngue)
- Endpoint configurável via `appsettings.json`

## Estrutura de pastas
```
Persistence/
  AppDbContext.cs
  Configurations/AnimalConfiguration.cs
  Migrations/           ← gerado por dotnet ef migrations add
  AnimalRepository.cs
Search/
  FullTextSearchService.cs
  SemanticSearchService.cs
  HybridSearchService.cs
Embeddings/
  OllamaEmbeddingService.cs
DependencyInjection.cs  → AddInfrastructure(services, config)
```

## Registro de DI
`AddInfrastructure` registra: `AppDbContext`, `IAnimalRepository`, todos os serviços de busca e embeddings.