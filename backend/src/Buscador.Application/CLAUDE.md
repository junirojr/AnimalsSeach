# Buscador.Application

## Responsabilidade
Orquestrar casos de uso via CQRS (MediatR). Depende só de `Domain`.
Nunca referencia EF Core, Npgsql ou qualquer detalhe de infraestrutura.

## Padrão CQRS com MediatR
- Cada operação = uma classe `Query` ou `Command` + um `Handler`
- **Query**: leitura, retorna dados (ex.: `GetAnimalByIdQuery`)
- **Command**: escrita/ação, pode retornar resultado (ex.: `SeedAnimalsCommand`)
- Handler implementa `IRequestHandler<TRequest, TResponse>`

## Estrutura de pastas
```
Animals/
  Queries/
    GetAnimalById/   → Query + Handler
    GetAnimals/      → Query + Handler + Validator
    SearchAnimals/   → Query + Handler + Validator + enum SearchMode
  Commands/
    SeedAnimals/     → Command + Handler
    GenerateEmbeddings/ → Command + Handler
  AnimalDto.cs           → DTO de saída (record)
  Seed/AnimalSeedData.cs → Lista estática dos animais
  Search/
    SearchResultDto.cs
    IFullTextSearchService.cs
    ISemanticSearchService.cs
    IHybridSearchService.cs
  Embeddings/
    IEmbeddingService.cs
DependencyInjection.cs   → AddApplication()
```

## Validação
- FluentValidation: um `AbstractValidator<T>` por command/query que precisa validar
- Registrar via `AddValidatorsFromAssembly` no `DependencyInjection.cs`

## Interfaces aqui, implementações na Infrastructure
- `IAnimalRepository` definida no Domain
- `IFullTextSearchService`, `ISemanticSearchService`, `IEmbeddingService` definidas aqui