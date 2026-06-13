# Buscador.Application

## Responsabilidade
Orquestrar casos de uso via CQRS (MediatR). Depende só de `Domain`.
Nunca referencia EF Core, Npgsql ou qualquer detalhe de infraestrutura.

## Padrão: Vertical Slices com MediatR
Cada feature é uma pasta autônoma. Não existe pasta `Queries/` ou `Commands/` global.

- **Query**: leitura, retorna dados — ex.: `GetAnimalByIdQuery`
- **Command**: escrita/ação — ex.: `SeedAnimalsCommand`
- Handler implementa `IRequestHandler<TRequest, TResponse>`
- Validação via `AbstractValidator<T>` dentro da própria feature

## Estrutura de pastas
```
Features/
  GetAnimalById/
    GetAnimalByIdQuery.cs
    GetAnimalByIdQueryHandler.cs
  GetAnimals/
    GetAnimalsQuery.cs
    GetAnimalsQueryHandler.cs
    GetAnimalsQueryValidator.cs
  SearchAnimals/
    SearchMode.cs
    SearchAnimalsQuery.cs
    SearchAnimalsQueryHandler.cs
    SearchAnimalsQueryValidator.cs
  SeedAnimals/
    SeedAnimalsCommand.cs
    SeedAnimalsCommandHandler.cs
    AnimalSeedData.cs              → dados de seed em português (sem acento)
  GenerateEmbeddings/
    GenerateEmbeddingsCommand.cs
    GenerateEmbeddingsCommandHandler.cs
Shared/                            → só o que é usado por 2+ features
  AnimalDto.cs
  SearchResultDto.cs
  IFullTextSearchService.cs
  ISemanticSearchService.cs
  IHybridSearchService.cs
  IEmbeddingService.cs
  IEmbeddingPersistenceService.cs  → Infrastructure orquestra: fetch + gerar + salvar
  ValidationBehavior.cs            → pipeline MediatR para auto-validação
GlobalUsings.cs                    → MediatR, FluentValidation, Domain, Shared
DependencyInjection.cs             → AddApplication()
```

## Regras de Vertical Slices
- Cada feature define seus próprios contratos quando necessário
- Mover para `Shared/` **somente** quando 2+ features precisarem do mesmo tipo
- Handlers NÃO referenciam outros handlers — usam interfaces de serviço

## Interfaces aqui, implementações na Infrastructure
- `IAnimalRepository` — definida no Domain
- `IFullTextSearchService`, `ISemanticSearchService`, `IHybridSearchService` — em Shared/
- `IEmbeddingService`, `IEmbeddingPersistenceService` — em Shared/