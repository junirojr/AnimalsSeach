# Como Rodar os Testes de Integracao

## Pre-requisitos

- **Docker Desktop** rodando. Os testes de integracao usam Testcontainers, que precisa do Docker para subir um container PostgreSQL automaticamente.
- **.NET 10 SDK** instalado.
- A **API .NET nao precisa estar rodando** separadamente. O Testcontainers sobe o banco por conta propria.

Confirme que o Docker esta ativo antes de continuar:

```bash
docker info
```

---

## Rodando todos os testes

Execute dentro do diretorio `backend/`:

```bash
cd backend
dotnet test
```

Isso roda os tres projetos de teste em sequencia.

---

## O que cada projeto testa

### Buscador.Domain.Tests — Unitarios

Testa as regras de negocio puras do dominio, sem dependencias externas.

- Criacao e validacao de entidades (`Animal`, value objects)
- Regras de invariante de dominio
- Usa: xUnit + FluentAssertions

### Buscador.Application.Tests — Unitarios com dublês

Testa os handlers de CQRS de forma isolada, com repositorios e servicos simulados.

- Handlers de query e command (ex.: busca por texto, populacao de animais)
- Validacao de inputs via FluentValidation
- Usa: xUnit + Moq + FluentAssertions

### Buscador.Api.Tests — Integracao com banco real

Testa os endpoints HTTP de ponta a ponta contra um PostgreSQL real (com pgvector).

- O `ApiTestFixture` sobe um container `pgvector/pgvector:pg16` automaticamente via Testcontainers
- As migrations sao aplicadas antes de cada suite de testes
- A API e inicializada via `WebApplicationFactory<Program>`, substituindo a connection string pelo banco do container
- Usa: xUnit + Testcontainers.PostgreSql + WebApplicationFactory

**Atencao:** os testes de `Buscador.Api.Tests` levam aproximadamente 7 minutos porque incluem geracao de embeddings via Ollama (`bge-m3`). Isso e esperado.

---

## Rodando apenas um projeto

Para rodar so os testes unitarios (mais rapido):

```bash
cd backend
dotnet test tests/Buscador.Domain.Tests
dotnet test tests/Buscador.Application.Tests
```

Para rodar apenas os testes de integracao:

```bash
cd backend
dotnet test tests/Buscador.Api.Tests
```

---

## Problemas comuns

| Sintoma | Causa provavel | Solucao |
|---------|---------------|---------|
| `Docker daemon not running` | Docker Desktop desligado | Inicie o Docker Desktop |
| Container nao sobe | Porta ocupada ou imagem ausente | `docker pull pgvector/pgvector:pg16` |
| Timeout nos testes de Api | Ollama lento ou sem GPU | Normal em CPU; aguarde os ~7 min |
| Falha de migration | Schema desatualizado | Nao e necessario rodar `dotnet ef` manualmente; o `ApiTestFixture` aplica as migrations automaticamente |
