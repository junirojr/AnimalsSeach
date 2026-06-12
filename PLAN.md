# Plano: Projeto Buscador de Animais (Deep Sparrow)

## Context

Projeto de aprendizado em C# para construir um mecanismo de busca híbrido (full-text + semântica) sobre um catálogo de animais. O objetivo é aprender C#/.NET 10, Clean Architecture, Domain Driven Design e as técnicas de busca de forma progressiva e prática. O projeto terá backend em ASP.NET Core Web API e frontend em Next.js, ambos com cobertura de testes.

---

## Stack Tecnológica

### Backend
| Camada | Tecnologia |
|--------|------------|
| Linguagem | C# / .NET 10 (LTS) |
| API | ASP.NET Core Web API (Minimal API) |
| ORM | Entity Framework Core 10 |
| Banco | PostgreSQL 16 (via Docker) |
| Full-text Search | PostgreSQL `tsvector` / `tsquery` nativo |
| Semantic Search | `pgvector` extension + Ollama |
| Embeddings | Ollama + modelo `nomic-embed-text` (local, gratuito) |
| CQRS | MediatR 12 |
| Validação | FluentValidation |
| Docs API | Scalar (OpenAPI moderno, substituiu Swagger UI) |

### Frontend
| Camada | Tecnologia |
|--------|------------|
| Framework | Next.js 15 (App Router) |
| Linguagem | TypeScript |
| Estilos | Tailwind CSS |
| Fetch | TanStack Query (React Query) |

### Infraestrutura (gratuito/local)
- **Docker Desktop** — PostgreSQL + pgvector + Ollama em containers
- **Git + GitHub** — controle de versão; remoto `origin` → `https://github.com/junirojr/AnimalsSeach.git`

### Testes — Backend
| Tipo | Tecnologia |
|------|------------|
| Unitários (Domain + Application) | xUnit + FluentAssertions + Moq |
| Integração (Infrastructure + API) | Testcontainers.PostgreSQL + WebApplicationFactory |

### Testes — Frontend
| Tipo | Tecnologia |
|------|------------|
| Unitários / Componentes | Jest + React Testing Library |
| End-to-End | Playwright |
| Mock de API | MSW (Mock Service Worker) |

---

## Estrutura de Pastas

```
Buscador/
├── CLAUDE.md                              # Contexto geral do projeto para IA
├── docker-compose.yml                     # PostgreSQL (pgvector) + Ollama
│
├── .claude/
│   ├── settings.json                      # Permissões, hooks e config do Claude Code
│   ├── settings.local.json                # Config pessoal local (no .gitignore)
│   ├── commands/                          # Slash commands customizados do projeto
│   │   ├── seed.md                        # /seed — popular o banco de animais
│   │   ├── test-all.md                    # /test-all — rodar backend + frontend tests
│   │   └── embeddings.md                  # /embeddings — gerar vetores via Ollama
│   └── hooks/                             # Hooks de ciclo de vida (pre/post tool)
│       └── post-edit.sh                   # Exemplo: rodar lint após edições
│
├── backend/
│   ├── CLAUDE.md                          # Stack, regra de deps, comandos dotnet
│   ├── Buscador.sln
│   ├── src/
│   │   ├── Buscador.Domain/
│   │   │   ├── CLAUDE.md                  # Sem deps externas, só regras de negócio
│   │   │   ├── Animals/
│   │   │   │   ├── Animal.cs
│   │   │   │   ├── AnimalId.cs
│   │   │   │   └── IAnimalRepository.cs
│   │   │   └── Common/
│   │   │       ├── AggregateRoot.cs
│   │   │       ├── Entity.cs
│   │   │       └── ValueObject.cs
│   │   ├── Buscador.Application/
│   │   │   ├── CLAUDE.md                  # CQRS com MediatR, só interfaces
│   │   │   └── Animals/
│   │   │       ├── Queries/
│   │   │       └── Commands/
│   │   ├── Buscador.Infrastructure/
│   │   │   ├── CLAUDE.md                  # EF Core, pgvector, Ollama
│   │   │   ├── Persistence/
│   │   │   ├── Search/
│   │   │   └── Embeddings/
│   │   └── Buscador.Api/
│   │       ├── CLAUDE.md                  # Minimal API, contratos HTTP, sem lógica
│   │       ├── Program.cs
│   │       └── Endpoints/
│   └── tests/
│       ├── CLAUDE.md                      # Padrões: Método_Cenário_Resultado
│       ├── Buscador.Domain.Tests/
│       ├── Buscador.Application.Tests/
│       └── Buscador.Api.Tests/
│           └── Fixtures/
│               └── ApiTestFixture.cs      # Base: Testcontainers + WebApplicationFactory
│
├── frontend/
│   ├── CLAUDE.md                          # Next.js App Router, TanStack Query, testes
│   ├── src/
│   │   ├── app/
│   │   │   └── page.tsx                   # Página principal de busca
│   │   ├── components/
│   │   │   ├── search/
│   │   │   │   ├── SearchBar.tsx
│   │   │   │   ├── SearchBar.test.tsx
│   │   │   │   └── SearchModeToggle.tsx
│   │   │   └── animals/
│   │   │       ├── AnimalCard.tsx
│   │   │       ├── AnimalCard.test.tsx
│   │   │       └── AnimalDetail.tsx
│   │   ├── services/
│   │   │   └── animals.ts                 # Chamadas à API
│   │   └── types/
│   │       └── animal.ts
│   ├── e2e/
│   │   └── search.spec.ts                 # Playwright: fluxo completo
│   ├── jest.config.ts
│   ├── playwright.config.ts
│   └── package.json
│
└── docs/
    ├── tutorials/                         # APRENDIZADO — passo a passo orientado ao fazer
    ├── how-to/                            # TAREFAS — guias para problemas específicos
    ├── reference/                         # REFERÊNCIA — descrição técnica objetiva
    ├── explanation/                       # EXPLICAÇÃO — conceitos e decisões de arquitetura
    └── ai/                               # Engenharia de contexto para IA
        ├── PROJECT_CONTEXT.md             # Visão geral: objetivo, stack, público-alvo
        ├── PROJECT_DOMAIN_MAP.md          # Mapa do domínio: entidades, relações, linguagem ubíqua
        ├── PROJECT_PLAYBOOK.md            # Regras operacionais: como trabalhar no projeto
        ├── PROJECT_RISK_REGISTER.md       # Riscos conhecidos, débitos técnicos, armadilhas
        └── DESIGN_PATTERNS.md            # Padrões adotados e quando/como aplicá-los
```

---

## Documentação — Estrutura Diataxis

O Diataxis é um framework de documentação com 4 quadrantes. Cada tipo serve um propósito diferente e é escrito de forma distinta.

```
docs/
│
├── tutorials/                         # APRENDIZADO — orientado ao fazer
│   ├── 01-setup-environment.md        # "Instale e configure tudo do zero"
│   ├── 02-domain-layer.md             # "Crie sua primeira entidade de domínio"
│   ├── 03-fulltext-search.md          # "Implemente sua primeira busca FTS"
│   └── 04-semantic-search.md          # "Configure Ollama e busca semântica"
│
├── how-to/                            # TAREFAS — orientado a problemas
│   ├── add-new-animal-species.md      # Como adicionar um novo animal
│   ├── configure-ollama-gpu.md        # Como usar GPU com Ollama
│   ├── run-integration-tests.md       # Como rodar testes de integração
│   ├── tune-search-weights.md         # Como ajustar pesos do RRF híbrido
│   └── reset-database.md             # Como resetar e re-seed o banco
│
├── reference/                         # REFERÊNCIA — orientado à informação
│   ├── api-endpoints.md              # Todos os endpoints, parâmetros, respostas
│   ├── domain-model.md               # Entidades, VOs, enums com descrição
│   ├── search-modes.md               # fulltext vs semantic vs hybrid — diferenças
│   ├── environment-variables.md      # Todas as variáveis de ambiente
│   └── database-schema.md            # Schema do banco, índices, triggers
│
└── explanation/                       # EXPLICAÇÃO — orientado ao entendimento
    ├── architecture-decisions.md     # Por que Clean Architecture + DDD?
    ├── why-pgvector.md               # Por que pgvector e não Elasticsearch?
    ├── how-fts-works.md              # Como tsvector/tsquery funciona internamente
    ├── how-embeddings-work.md        # O que são vetores e cosine similarity
    └── rrf-algorithm.md              # Por que RRF para combinar os rankings?
```

### Distinção entre os quadrantes

| Tipo | Orientação | Responde | Exemplo de título |
|------|-----------|----------|-------------------|
| Tutorial | Aprendizado | "Ensine-me" | "Crie sua primeira busca" |
| How-to | Tarefa | "Como faço X?" | "Como ajustar os pesos de busca" |
| Reference | Informação | "O que é X?" | "Endpoint GET /api/animals/search" |
| Explanation | Conceito | "Por que X?" | "Por que usamos pgvector e não Elasticsearch" |

---

## Engenharia de Contexto — Arquivos CLAUDE.md

Os arquivos `CLAUDE.md` são a memória de contexto para IAs (Claude Code) que trabalham no projeto. Cada nível tem seu próprio arquivo com o contexto daquela camada.

### CLAUDE.md Raiz — `/CLAUDE.md`
Contexto geral: nome do projeto, objetivo, stack completa, comandos essenciais (`docker-compose up`, `dotnet test`, `npm test`), convenções globais (idioma do código, commits, etc.).

### CLAUDE.md Backend — `/backend/CLAUDE.md`
Contexto do backend: estrutura de camadas, regra de dependências (Domain → Application → Infrastructure → Api), pacotes NuGet principais, comandos `dotnet`, padrões de erro.

### CLAUDE.md por Camada

| Arquivo | Conteúdo chave |
|---------|----------------|
| `Buscador.Domain/CLAUDE.md` | Sem dependências externas. Aqui vivem apenas regras de negócio. Nunca referencia EF Core, Npgsql ou MediatR. |
| `Buscador.Application/CLAUDE.md` | CQRS com MediatR: toda Query/Command tem seu Handler. FluentValidation para inputs. Interfaces, nunca implementações. |
| `Buscador.Infrastructure/CLAUDE.md` | EF Core com Npgsql. Migrations via `dotnet ef`. pgvector para embeddings. Ollama via `Microsoft.Extensions.AI`. |
| `Buscador.Api/CLAUDE.md` | Minimal API. Endpoints mapeados em arquivos de extensão. Contratos em `Contracts/`. Nunca lógica de negócio aqui. |
| `backend/tests/CLAUDE.md` | Padrão `Método_Cenário_Resultado`. Testcontainers para banco real. `ApiTestFixture` como base dos integration tests. |

### CLAUDE.md Frontend — `/frontend/CLAUDE.md`
Next.js App Router. Componentes em `components/`. Chamadas à API via TanStack Query em `services/`. Testes co-localizados (`*.test.tsx`). E2E em `e2e/` com Playwright.

### `docs/ai/` — Contexto semântico profundo para IA

Complementa os `CLAUDE.md` com contexto rico que a IA precisa para tomar boas decisões arquiteturais e de código:

| Arquivo | Conteúdo |
|---------|----------|
| `PROJECT_CONTEXT.md` | Objetivo do projeto, público-alvo, stack, restrições, metas de aprendizado |
| `PROJECT_DOMAIN_MAP.md` | Linguagem ubíqua do domínio: entidades, relações, glossário de termos de busca e animais |
| `PROJECT_PLAYBOOK.md` | Como trabalhar no projeto: idioma do código (inglês), idioma dos dados (PT), padrão de commits, fluxo de desenvolvimento por fase |
| `PROJECT_RISK_REGISTER.md` | Riscos conhecidos, armadilhas de pgvector/Ollama, débitos técnicos planejados, limitações do iniciante |
| `DESIGN_PATTERNS.md` | Padrões adotados (CQRS, Repository, Value Object, RRF), quando usar cada um, exemplos reais do projeto |

---

## Modelo de Domínio — Animal

```csharp
// Buscador.Domain/Animais/Animal.cs
public sealed class Animal : RaizAgregada<AnimalId>
{
    public string NomeComum { get; private set; }            // "Leão"
    public string NomeCientifico { get; private set; }       // "Panthera leo"
    public string Descricao { get; private set; }            // Texto narrativo longo
    public string Caracteristicas { get; private set; }      // Traços físicos
    public Dieta Dieta { get; private set; }                 // enum: Carnivoro/Herbivoro/Onivoro
    public Habitat Habitat { get; private set; }             // enum: Floresta/Oceano/Deserto/etc.
    public string DistribuicaoGeografica { get; private set; }
    public StatusConservacao StatusConservacao { get; private set; } // enum: IUCN
    public string[] Tags { get; private set; }               // ["mamífero", "predador", "felino"]
    public string Curiosidades { get; private set; }         // Fatos curiosos

    // Campos de busca: VetorBusca (tsvector) e Embedding (vector 768) NÃO ficam nesta classe.
    // São shadow properties mapeadas via Fluent API na Infrastructure (Fase 2), para manter o
    // Domain limpo (sem dependência de Npgsql/pgvector).
}
```

---

## Endpoints da API

```
GET  /api/animais/buscar?q=...&modo=textual|semantica|hibrida  # Busca principal
GET  /api/animais/{id}                                          # Detalhes de um animal
GET  /api/animais?pagina=1&tamanho=20                           # Listagem paginada
POST /api/animais/popular                                       # Popular banco com dados exemplo
POST /api/animais/embeddings/gerar                              # Gerar embeddings (Ollama)
```

---

## Casos de Uso (CQRS com MediatR)

**Consultas (Queries):**
- `BuscarAnimaisConsulta` — busca híbrida com modo configurável
- `ObterAnimalPorIdConsulta`
- `ObterAnimaisConsulta` (paginada)

**Comandos (Commands):**
- `PopularAnimaisComando` — insere os animais pré-definidos (10 no MVP, expande para 50)
- `GerarEmbeddingsComando` — chama Ollama e popula a coluna `embedding`

---

## Roteiro de Implementação (fases progressivas)

### Fase 0 — Ambiente + Documentação Inicial (½ dia)
- [ ] Instalar: .NET 10 SDK, Docker Desktop, Node.js 22, Ollama
- [ ] `docker-compose.yml` com `pgvector/pgvector:pg16` e `ollama/ollama`
- [ ] Criar solution, projetos e estrutura de pastas completa (incluindo `docs/`)
- [ ] Init Next.js com TypeScript + Tailwind
- [ ] Criar `CLAUDE.md` raiz e todos os `CLAUDE.md` das camadas (esboços iniciais)
- [ ] Criar estrutura vazia dos 4 quadrantes Diataxis diretamente em `docs/`
- [ ] Criar `docs/ai/PROJECT_CONTEXT.md`, `PROJECT_PLAYBOOK.md` e `DESIGN_PATTERNS.md` com base inicial

### Fase 1 — Domain Layer (1-2 dias)
- [ ] `AggregateRoot<TId>`, `Entity<TId>`, `ValueObject` base classes
- [ ] `Animal` aggregate + `AnimalId` value object
- [ ] Enums: `Diet`, `Habitat`, `ConservationStatus`
- [ ] `IAnimalRepository` interface
- [ ] **Testes unitários do Domain** (xUnit + FluentAssertions)

### Fase 2 — Infrastructure: Banco (1-2 dias)
- [ ] `AppDbContext` com Npgsql + pgvector
- [ ] `AnimalConfiguration` (Fluent API, coluna tsvector + vector)
- [ ] Migrations — incluindo índice GIN para FTS e HNSW para pgvector
- [ ] `AnimalRepository` implementação
- [ ] **Testes de integração com Testcontainers** (banco PostgreSQL real em Docker)

### Fase 3 — Application Layer (1-2 dias)
- [ ] MediatR: handlers de CRUD básico
- [ ] `SeedAnimalsCommand` com 50 animais populados
- [ ] FluentValidation nas queries/commands
- [ ] **Testes unitários do Application** (Moq para repositório)

### Fase 4 — Full-Text Search (1-2 dias)
- [ ] Trigger PostgreSQL para atualizar `search_vector` automaticamente
- [ ] `FullTextSearchService` com ranking por `ts_rank`
- [ ] `SearchAnimalsQuery` modo `fulltext`
- [ ] Testes de integração de busca FTS

### Fase 5 — Busca Semântica (2-3 dias)
- [ ] `OllamaEmbeddingService` usando `Microsoft.Extensions.AI.Ollama`
- [ ] `GenerateEmbeddingsCommand` em batch
- [ ] `SemanticSearchService` com `<=>` (cosine distance no pgvector)
- [ ] `SearchAnimalsQuery` modo `semantic`
- [ ] Testes de integração semântica

### Fase 6 — Busca Híbrida (1 dia)
- [ ] Combinar scores FTS + semântico com RRF (Reciprocal Rank Fusion)
- [ ] `SearchAnimalsQuery` modo `hybrid`
- [ ] Testes comparativos dos três modos

### Fase 7 — API Layer (1 dia)
- [ ] Minimal API endpoints
- [ ] Scalar (OpenAPI/docs)
- [ ] Global error handling (`IExceptionHandler`)
- [ ] **Testes de integração da API** com `WebApplicationFactory`

### Fase 8 — Frontend Next.js (2-3 dias)
- [ ] Página de busca com `SearchBar` e `SearchModeToggle`
- [ ] Grid de `AnimalCard` com TanStack Query
- [ ] `AnimalDetail` modal/drawer
- [ ] **Testes de componentes** (Jest + React Testing Library)
- [ ] **Testes E2E** (Playwright — fluxo de busca completo)

---

## Arquitetura de Testes

### Backend

```
Buscador.Domain.Tests/
  Animais/
    AnimalTests.cs           # Testa regras de negócio do aggregate
    AnimalIdTests.cs         # Testa value object

Buscador.Application.Tests/
  Animais/
    BuscarAnimaisManipuladorTests.cs   # Moq do repositório e serviços
    PopularAnimaisManipuladorTests.cs

Buscador.Api.Tests/
  Animais/
    BuscarEndpointTests.cs         # WebApplicationFactory + Testcontainers
    PopularEndpointTests.cs
  Fixtures/
    ApiTestFixture.cs              # Classe base: inicia container PostgreSQL
```

**Padrão de nomenclatura de testes (em português, sem acento):**
```csharp
// Metodo_Cenario_ResultadoEsperado
public void Buscar_ComPalavraChaveCorrespondente_RetornaAnimaisRelevantes() { }
public void Buscar_ComConsultaVazia_LancaValidationException() { }
```

### Frontend

```
src/components/busca/
  BarraBusca.test.tsx        # Renderização, input, debounce

src/components/animais/
  CartaoAnimal.test.tsx      # Renderização de dados do animal

e2e/
  busca.spec.ts              # Playwright: digitar, ver resultados, mudar modo
```

**MSW para mock da API nos testes de componente:**
```ts
// tests/mocks/handlers.ts
http.get('/api/animais/buscar', () => {
  return HttpResponse.json({ itens: animaisMock })
})
```

---

## Docker Compose

```yaml
# docker-compose.yml
services:
  postgres:
    image: pgvector/pgvector:pg16
    environment:
      POSTGRES_DB: buscador
      POSTGRES_USER: buscador
      POSTGRES_PASSWORD: buscador
    ports:
      - "5432:5432"

  ollama:
    image: ollama/ollama
    ports:
      - "11434:11434"
    volumes:
      - ollama_data:/root/.ollama

volumes:
  ollama_data:
```

Após subir: `docker exec ollama ollama pull nomic-embed-text`

---

## Controle de Versão (Git)

O projeto é versionado com Git e tem como remoto o repositório:

- **`origin`** → `https://github.com/junirojr/AnimalsSeach.git` · **branch:** `main`

### Fluxo de commits durante a execução

O versionamento acompanha a execução das tasks (ver `TASKS.md`):

1. **Commit por task aprovada** — somente após o DoD da task passar (build/testes verdes) **e** o usuário aprovar.
2. **Push ao final de cada fase** — quando todas as tasks da fase estão aprovadas: `git push origin main`.
3. **Nunca** commitar com build quebrado, testes vermelhos ou segredos.

### Convenção de mensagens (Conventional Commits)

| Prefixo | Uso | Exemplo |
|---------|-----|---------|
| `feat`  | nova funcionalidade | `feat(domain): adiciona aggregate Animal` |
| `test`  | testes | `test(application): handlers com Moq` |
| `docs`  | documentação | `docs: preenche how-fts-works` |
| `chore` | infra/config | `chore: docker-compose + estrutura inicial` |
| `fix`   | correção | `fix(search): corrige ranking RRF` |

Cada mensagem termina com o trailer de coautoria do Claude.

### `.gitignore`

Cobre artefatos de build e segredos locais: `bin/`, `obj/`, `node_modules/`, `.next/`, `*.user`,
`.env.local`, `.claude/settings.local.json`. As senhas de dev (`buscador/buscador`) são locais e
podem ficar versionadas no `appsettings.json` / `docker-compose.yml`.

---

## Referências Essenciais

### C# / .NET
- [Documentação oficial .NET 10](https://learn.microsoft.com/pt-br/dotnet/) — Microsoft Learn
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/pt-br/aspnet/core/fundamentals/minimal-apis) — ponto de partida da API
- [EF Core com PostgreSQL](https://www.npgsql.org/efcore/) — npgsql.org

### Arquitetura
- [Clean Architecture Template — Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture) — template de referência em C#
- [Domain-Driven Design Fundamentals](https://www.pluralsight.com/courses/fundamentals-domain-driven-design) — conceitos DDD
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers) — exemplo real DDD da Microsoft

### Busca
- [PostgreSQL Full Text Search](https://www.postgresql.org/docs/current/textsearch.html) — docs oficiais tsvector/tsquery
- [pgvector README](https://github.com/pgvector/pgvector) — tipos de índice, operadores
- [Npgsql pgvector plugin](https://www.npgsql.org/doc/types/nativepgtypes.html) — integração .NET

### Embeddings
- [Ollama docs](https://ollama.com/docs) — como rodar modelos localmente
- [Microsoft.Extensions.AI](https://learn.microsoft.com/pt-br/dotnet/ai/microsoft-extensions-ai) — integração oficial Microsoft com Ollama
- [nomic-embed-text no Ollama](https://ollama.com/library/nomic-embed-text) — modelo de 768 dimensões

### Testes
- [xUnit docs](https://xunit.net/) — framework de testes
- [Testcontainers for .NET](https://dotnet.testcontainers.org/) — containers em testes de integração
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/) — testes de componente
- [Playwright for Next.js](https://playwright.dev/docs/intro) — testes E2E

### Documentação e Engenharia de Contexto
- [Diataxis Framework](https://diataxis.fr/) — framework de documentação técnica (4 quadrantes)
- [CLAUDE.md Best Practices](https://docs.anthropic.com/pt/docs/claude-code/memory) — como escrever contexto para Claude Code

---

## Verificação (como testar ao final)

1. **Subir infra**: `docker-compose up -d`
2. **Migrations**: `dotnet ef database update` na pasta Api
3. **Seed**: `POST /api/animais/popular` via Scalar UI
4. **Gerar embeddings**: `POST /api/animais/embeddings/gerar` (aguarda Ollama)
5. **Testar FTS**: `GET /api/animais/buscar?q=carnívoro+savana&modo=textual`
6. **Testar semântica**: `GET /api/animais/buscar?q=animal+que+vive+em+grupo+e+caça+em+bando&modo=semantica`
7. **Testar híbrido**: mesma query com `modo=hibrida`, comparar ranking
8. **Testes backend**: `dotnet test` na pasta `backend/`
9. **Frontend**: abrir `http://localhost:3000`, buscar e comparar os três modos
10. **Testes frontend**: `npm test` + `npx playwright test`
