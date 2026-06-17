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
GET  /api/animais/buscar?q=&modo=textual|semantica|hibrida&limite=
GET  /api/animais/{id}
GET  /api/animais?pagina=&tamanho=
POST /api/animais/popular
POST /api/animais/embeddings/gerar
```

> **Estado (Fase 7 concluída):** os 5 endpoints vivem em `Endpoints/EndpointsAnimais.cs` (extensão
> `MapearEndpointsAnimais`), chamada pelo `Program.cs` (só wiring). Tratamento de erro global, validação
> (q vazio → 400), Scalar e CORS (origem em `Cors:AllowedOrigin`, default `localhost:3000`) já habilitados.

## Estrutura de pastas
```
Endpoints/
  EndpointsAnimais.cs   → extensão MapearEndpointsAnimais(this WebApplication app)
TratamentoErros/
  ManipuladorGlobalExcecoes.cs  → IExceptionHandler (ValidationException→400, resto→500)
Program.cs              → wiring: AdicionarAplicacao, AdicionarInfraestrutura, CORS, MapearEndpointsAnimais
appsettings.json        → ConnectionStrings:Postgres, Ollama:BaseUrl, Cors:AllowedOrigin
```

## Scalar (OpenAPI)
- `AddOpenApi()` + `MapOpenApi()` + `MapScalarApiReference()`
- UI disponível em `/scalar` quando a Api está rodando

## Tratamento de erros
- `ManipuladorGlobalExcecoes` com `IExceptionHandler`
- `ValidationException` do FluentValidation → HTTP 400 com detalhes
- Exceções não tratadas → HTTP 500 com ProblemDetails