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