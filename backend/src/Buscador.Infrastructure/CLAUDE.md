# Buscador.Infrastructure

## Responsabilidade
Implementar os contratos definidos no Domain e Application.
Aqui ficam banco de dados, migrations, repositórios e serviços externos (Ollama).

## EF Core + PostgreSQL

- `ContextoBanco` com `DbSet<Animal> Animais` — tabela `animais`
- `AnimalConfiguracao` (Fluent API em `Persistencia/Configuracoes/`):
  - Enums `Dieta`, `Habitat`, `StatusConservacao` gravados como **texto** via `.HasConversion<string>()`
  - Shadow properties: `search_vector` (tsvector) e `embedding` (vector(1024)) — o Domain não as conhece
- Migrations geradas com `dotnet ef migrations add` (ver backend/CLAUDE.md)

## pgvector

- `UseVector()` no `ContextoBanco` + `HasPostgresExtension("vector")` no `OnModelCreating`
- Índice **GIN** em `search_vector` (busca full-text)
- Índice **HNSW** em `embedding` com distância cosseno (`<=>`) — tabela `animais` e `fragmentos_animal`
- Acesso a pgvector via **SQL cru** (`ExecuteSqlRawAsync`) — sem EF Navigation

## Tabelas

| Tabela | Descrição |
|--------|-----------|
| `animais` | Dados dos animais + shadow properties search_vector e embedding |
| `fragmentos_animal` | `id`, `animal_id` FK ON DELETE CASCADE, `texto`, `embedding vector(1024)` |

`fragmentos_animal` implementa busca multi-vetor: cada animal gera vários fragmentos; a busca semântica usa `MIN(distância) GROUP BY animal_id` (max-sim).

## Serviços de busca

| Classe | Técnica |
|--------|---------|
| `ServicoBuscaTextual` | `ts_rank` + `tsquery` com OR e `unaccent` |
| `ServicoBuscaSemantica` | distância cosseno (`<=>`), max-sim via `MIN(...) GROUP BY animal_id` |
| `ServicoBuscaHibrida` | busca os dois modos com pool ≥ 20 e repassa para `FusaoRrf` |

`FusaoRrf` fica na **Application** (função pura, k=60).

## Ollama / Embeddings

- `ServicoEmbeddingOllama`: modelo `bge-m3`, 1024 dimensões, multilíngue, **sem prefixos**, geração em batch via `Microsoft.Extensions.AI.Ollama`
- `ServicoPersistenciaFragmentos`: grava fragmentos em `fragmentos_animal` via SQL cru

`FragmentadorAnimal` (que divide o animal em chunks de texto) fica na **Application**.

## Estrutura de pastas

```
Busca/
  ServicoBuscaTextual.cs
  ServicoBuscaSemantica.cs
  ServicoBuscaHibrida.cs
Embeddings/
  ServicoEmbeddingOllama.cs
  ServicoPersistenciaFragmentos.cs
Persistencia/
  ContextoBanco.cs
  Configuracoes/
    AnimalConfiguracao.cs
  RepositorioAnimal.cs
Migrations/               ← gerado por dotnet ef migrations add
InjecaoDependencia.cs     → AdicionarInfraestrutura(services, config)
```

## Registro de DI

`AdicionarInfraestrutura` registra: `ContextoBanco`, `IRepositorioAnimal`, todos os serviços de busca e embeddings.

Variáveis esperadas: `ConnectionStrings:Postgres`, `Ollama:BaseUrl`.
