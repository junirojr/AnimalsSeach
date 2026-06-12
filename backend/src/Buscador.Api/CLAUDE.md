# Buscador.Api

## Responsabilidade
Camada HTTP. Mapeia rotas, recebe inputs, delega ao MediatR e retorna respostas.
NUNCA coloque lógica de negócio aqui — tudo vai para Application.

## Endpoints (Minimal API)
Cada endpoint faz exatamente 3 coisas:
1. Ler o input da requisição
2. `await mediator.Send(new MinhaQuery(...))`
3. Retornar `Results.Ok(...)` / `Results.NotFound()` / etc.

```
GET  /api/animals/search?q=&mode=fulltext|semantic|hybrid&limit=
GET  /api/animals/{id}
GET  /api/animals?page=&size=
POST /api/animals/seed
POST /api/animals/embeddings/generate
```

## Estrutura de pastas
```
Endpoints/
  AnimalEndpoints.cs   → extensão MapAnimalEndpoints(this WebApplication app)
Contracts/             → requests/responses HTTP (se diferirem dos DTOs da Application)
ExceptionHandling/
  GlobalExceptionHandler.cs  → IExceptionHandler (ValidationException→400, resto→500)
Program.cs             → wiring: AddApplication, AddInfrastructure, MapAnimalEndpoints
appsettings.json       → ConnectionStrings:Postgres, Ollama:BaseUrl, Cors:AllowedOrigin
```

## Scalar (OpenAPI)
- `AddOpenApi()` + `MapOpenApi()` + `MapScalarApiReference()`
- UI disponível em `/scalar` quando a Api está rodando

## Tratamento de erros
- `GlobalExceptionHandler` com `IExceptionHandler`
- `ValidationException` do FluentValidation → HTTP 400 com detalhes
- Exceções não tratadas → HTTP 500 com ProblemDetails