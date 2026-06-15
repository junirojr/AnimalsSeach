# PROJECT_RISK_REGISTER

Registro de decisões arquitetônicas críticas e riscos conhecidos do projeto Buscador.

## Fase 2 — Infrastructure

### Decisão: Shadow Properties para VetorBusca e Embedding

**Contexto:**
A classe `Animal` (Domain) precisa de dois campos para suportar busca textual e semântica:
- `VetorBusca` (tipo PostgreSQL `tsvector` — índice GIN para FTS)
- `Embedding` (tipo `vector(768)` do pgvector — índice HNSW para busca semântica)

**Alternativas consideradas:**
1. **Adicionar as propriedades na classe `Animal` (Domain)** — Viola a regra de dependência: Domain não pode referenciar `Npgsql.TypeMapping` ou tipos do pgvector.
2. **Shadow properties mapeadas na Infrastructure** ✓ Escolhido — Mantém Domain limpo; campos existem no banco e na configuração EF, mas não na classe C#.

**Decisão:**
Usar **shadow properties** mapeadas via Fluent API em `AnimalConfiguration.cs`:
- `VetorBusca` e `Embedding` são configurados via `modelBuilder.Property(...)` em `OnModelCreating`
- Não aparecem como propriedades de `Animal`, apenas nas colunas SQL `search_vector` e `embedding`
- Acesso em tempo de execução via `DbContext.Entry(animal).Property("Embedding").CurrentValue`
- Risco mitigan: documentado aqui e no `CLAUDE.md` da Infrastructure

**Benefícios:**
- Domain permanece independente de frameworks
- Fácil de estender (adicionar mais campos shadow no futuro)
- Sem mudanças no contrato do agregado (API de Domain inalterada)

**Implementação:**
- T2.3: mapeamento em `AnimalConfiguracao.cs` com `.HasColumnName("search_vector")` e `.HasColumnName("embedding")`
- T2.4: trigger PostgreSQL popula `search_vector` automaticamente no INSERT/UPDATE
- Acesso aos valores durante busca semântica (T5) via SQL raw ou ExpressionAPI

---

## Estado REAL após a Fase 2 (divergências a tratar na fase certa)

> Verificado em 2026-06-15. A Fase 2 está verde (build + 13 testes, incl. integração). A implementação
> divergiu da decisão acima em dois pontos. Tratar **na fase correspondente**, não agora — evita rework
> de migration já aplicada e pushada.

### ⚠️ DÍVIDA 1 — `embedding` NÃO está mapeado no modelo EF (resolver na Fase 5)
- **Estado real:** a coluna `embedding vector(768)` foi criada por **SQL cru** na migration
  `CriacaoInicial` (`migrationBuilder.Sql("ALTER TABLE animais ADD COLUMN embedding ...")`), **fora**
  do modelo EF. O `ContextoBancoModelSnapshot` **não conhece** `embedding`. (Só `VetorBusca`/`search_vector`
  está mapeado como shadow property.)
- **Risco (armadilha):** ao mapear `embedding` como shadow property na Fase 5 e rodar
  `dotnet ef migrations add`, o EF gera um `AddColumn("embedding")` que **falha** ao aplicar (coluna já existe).
- **Correção na Fase 5 (preferida):** mapear `embedding` como shadow property em `AnimalConfiguracao`
  (`builder.Property<Vector>("Embedding").HasColumnName("embedding").HasColumnType("vector(768)")`),
  **regenerar** a migration `CriacaoInicial` (remover o `Sql(ALTER...)` do embedding e deixar o EF criar a
  coluna) e recriar o banco local (DB é descartável). Alternativa: na migration nova, remover o `AddColumn`
  redundante.

### ⚠️ DÍVIDA 2 — nome da shadow property `vetorbusca` minúsculo (resolver na Fase 4)
- **Estado real:** em `AnimalConfiguracao` está `Property<NpgsqlTsVector>("vetorbusca")`.
- **Esperado (glossário):** `"VetorBusca"` (PascalCase). A coluna `search_vector` já está correta.
- **Correção na Fase 4:** ao implementar o FTS (que referencia a property por nome, ex.:
  `EF.Property<NpgsqlTsVector>(a, "VetorBusca")`), padronizar para `"VetorBusca"`. Como `HasColumnName`
  não muda, não há alteração de schema.

### ✅ Correção já aplicada (na verificação de 2026-06-15)
- Classe de DI da Application renomeada de `DependencyInjection` → **`InjecaoDependencia`** (consistência
  com a Infrastructure e o glossário PT). O método `AdicionarAplicacao()` permanece igual.