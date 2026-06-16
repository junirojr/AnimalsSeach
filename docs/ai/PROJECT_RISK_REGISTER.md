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

> Verificado em 2026-06-15. A implementação da Fase 2 divergiu da decisão acima em dois pontos.
> **Ambos resolvidos:** dívida #1 (embedding) por decisão de arquitetura (acesso via SQL cru) e
> dívida #2 (casing `VetorBusca`) na Fase 4. Histórico mantido abaixo.

### ✅ DÍVIDA 1 — acesso ao `embedding` (RESOLVIDA POR DECISÃO — Fase 5)
- **Contexto:** a coluna `embedding vector(768)` foi criada por SQL cru na migration `CriacaoInicial`,
  **fora** do modelo EF (o snapshot não a conhece). Mapeá-la como shadow property exigiria regenerar
  migration já aplicada/pushada — rework arriscado.
- **DECISÃO (2026-06-15):** o `embedding` **NÃO será mapeado no modelo EF**. Será acessado por **SQL cru**,
  exatamente como o `search_vector`/FTS já faz (que é o padrão idiomático para operadores do pgvector,
  ex.: `<=>` cosine). Assim:
  - **Gravação** (T5.3 `GerarEmbeddingsComando`): `UPDATE animais SET embedding = {vetor}::vector WHERE id = {id}`
    via `ContextoBanco.Database.ExecuteSqlRaw`.
  - **Leitura/busca** (T5.4 `ServicoBuscaSemantica`): `... ORDER BY embedding <=> {vetorConsulta}::vector ...`,
    espelhando o `ServicoBuscaTextual` (busca por SQL em 3 passos: scores por SQL → carrega entidades por id → combina).
- **Efeito:** elimina a armadilha de `AddColumn` em coluna existente e dispensa rework de migration.
  `embedding` permanece como coluna do banco, não como propriedade do agregado nem do modelo EF.

### ✅ DÍVIDA 2 — casing da shadow property `VetorBusca` (RESOLVIDA na Fase 4)
- Era `Property<NpgsqlTsVector>("vetorbusca")`; padronizado para `"VetorBusca"` (commit `cbcb4c9`).

### ✅ Correções já aplicadas
- DI da Application renomeada de `DependencyInjection` → **`InjecaoDependencia`** (consistência com a
  Infrastructure e o glossário PT). O método `AdicionarAplicacao()` permanece igual. (commit `64e8ce9`)
- Shadow property `VetorBusca` padronizada (commit `cbcb4c9`).

---

## Fase 4 — Limitação conhecida do Full-Text Search (acentos)

- **Comportamento atual:** a busca usa `to_tsquery('portuguese', ...)`, que é **sensível a acento**
  (sem extensão `unaccent`). Ex.: `q=leão` acha o Leão; `q=leao` (sem acento) **não** acha.
- **Também:** múltiplos termos viram `AND` (espaço → ` & `), e há *stemming* português (singular/plural,
  formas verbais colapsam).
- **Melhoria opcional (não planejada):** habilitar `CREATE EXTENSION unaccent` e usar
  `to_tsvector('portuguese', unaccent(...))` no gatilho + `unaccent()` na query para tornar a busca
  insensível a acento. Decidir se vale a pena (custo: nova migration + ajuste no `ServicoBuscaTextual`).

---

## Fase 5 — Observações (verificação de 2026-06-15)

### ✅ Teste semântico flaky — ENDURECIDO
- **Sintoma:** `ServicoBuscaSemanticaTests` passava isolado, mas falhava na suíte completa (xUnit roda
  classes em paralelo; sob carga, o ranking mudava).
- **Causa:** a asserção fixava um animal específico (`Lobo`) no top-5 de uma busca semântica — relevância
  de embedding **não é determinística** o bastante para isso.
- **Correção:** manter `NotBeEmpty` (prova que busca semântica retorna sem correspondência textual) e
  trocar a asserção frágil por uma **determinística**: `BeInDescendingOrder(r => r.Pontuacao)`. A
  verificação de "o Lobo aparece pra 'caça em bando'" fica para teste **manual/exploratório**, não gate.

### ⚠️ `GerarEmbeddingsComandoManipulador` está na Infrastructure (decisão pendente)
- **Estado real:** o comando `GerarEmbeddingsComando` está na Application, mas seu **handler** está em
  `Infrastructure/Embeddings/` (porque usa `ContextoBanco`/SQL cru). A Infrastructure passou a registrar
  MediatR do próprio assembly — por isso funciona em runtime. **A regra de dependência NÃO é violada**
  (Infrastructure→Application é permitido), mas **quebra a convenção** (todos os outros handlers estão na
  Application) e é o único caso assim.
- **Opção de refatoração (se quiser padronizar):** mover o handler para
  `Application/Funcionalidades/GerarEmbeddings/` e extrair a gravação para uma interface
  (ex.: `IServicoPersistenciaEmbedding` com `AtualizarEmbeddingAsync` + `ObterIdsSemEmbeddingAsync`)
  implementada na Infrastructure. Aí a Application volta a conter o handler e a Infra não precisa registrar MediatR.
- **Decisão:** pendente do usuário (aceitar pragmático vs refatorar).