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

### ✅ Prefixos de tarefa do `nomic-embed-text` (RESOLVIDO — commit `bde70c2`)
- **Sintoma:** no modo Semantica, as similaridades de todos os animais "grudavam" perto de ~0,50, com
  diferenças quase imperceptíveis entre o #1 e o #10 — o ranking parecia aleatório.
- **Causa:** o `nomic-embed-text` **exige prefixos de tarefa**. Sem eles os vetores ficam pouco
  separados. O `ServicoEmbeddingOllama` enviava o texto cru, sem prefixo.
- **Correção:** `IServicoEmbedding.GerarAsync` passou a receber `TipoTextoEmbedding { Documento, Consulta }`;
  o serviço prefixa `search_document: ` nos textos dos animais (gerados em `GerarEmbeddingsComandoManipulador`)
  e `search_query: ` na consulta (`ServicoBuscaSemantica`).
- **Efeito medido:** para queries conceituais e ricas o ranking melhorou claramente
  (ex.: "predador dos oceanos" → Tubarão-branco em #1 com score 0,638 e gap de ~0,058 para o #2).

### ⚠️ Limitação do `nomic-embed-text` em PT — queries curtas / atributo booleano (CONHECIDA)
- **Mesmo com os prefixos**, queries **curtas** ("voar") ou de **atributo binário** ("é venenoso",
  "vive na água") não geram separação suficiente: o modelo captura similaridade de *gênero textual*
  (descrição de animal) em vez do *atributo*. Ex.: "voar" coloca Águia só em #3; "Animais que voam"
  traz Sapo e Tubarão no top-3.
- **Causa raiz:** `nomic-embed-text` é primariamente inglês; queries curtas em português produzem vetores
  fracos.
- **Onde a busca semântica FUNCIONA bem:** queries longas com vocabulário de domínio e similaridade
  temática/conceitual ("símbolo de liberdade", "predador de topo marinho").
- **Onde FALHA:** atributos binários, queries de uma palavra, qualquer caso onde o FTS literal seria
  mais preciso.
- **Mitigações (ordem de custo/benefício):**
  1. **Incluir `NomeComum` + `Tags` no texto embedado** (hoje só `Descricao + Caracteristicas + Curiosidades`).
     A tag `voo` da Águia isolaria seu vetor do Sapo. Barato; exige regerar embeddings.
  2. **Modo `Hibrida` (F6 / RRF):** o FTS ancora a palavra literal e o semântico desempata — melhor
     resposta para queries curtas.
  3. **Modelo multilíngue** (ex.: `multilingual-e5-large`): maior impacto no PT, maior custo/risco; só
     se 1+2 não bastarem.

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

---

## Multi-vetor por fragmentos (chunks) — busca semântica (IMPLEMENTADO — commits `a7305d8`, `e8bd461`, `ab3ae52`)

### Contexto
Mesmo com prefixos + tags no texto, atributos curtos ("voar") rankeavam mal. Causa: cada animal era
**um único vetor** (mean pooling de ~200 palavras) → o sinal de um atributo (ex.: a tag `voo`) diluía na média.

### Decisão
Indexar cada animal como **vários vetores** (chunks), não um só:
- Nova tabela `fragmentos_animal(id, animal_id, texto, embedding)` criada por migration com **SQL cru**
  (não mapeada no modelo EF — mesma decisão do `embedding`), FK `ON DELETE CASCADE` + índice HNSW.
- **Chunking** em `FragmentadorAnimal` (na **Application**, função pura → testável): `NomeComum` + cada
  **frase** dos campos longos (split em `[.!?]`) + **cada tag isolada** (a tag `voo` vira um vetor próprio).
- **Geração** (`GerarEmbeddingsComandoManipulador`): 1 embedding por fragmento; idempotente por
  "animal sem fragmentos". **Para regerar no dev: `DELETE FROM fragmentos_animal;` antes do POST.**
- **Busca** (`ServicoBuscaSemantica`): **max-sim** — `MIN(distância)` entre os fragmentos de cada animal
  (`GROUP BY a.id`), o animal pontua pelo seu *melhor* fragmento, não pela média.

### Efeito medido
- ✅ "voar" → Águia #1 com gap nítido (atributo com tag isolada funciona).
- ❌ "tem asas" ainda errava (atributo **sem tag** fica enterrado numa frase → diluído).

## Troca de modelo: nomic-embed-text → `bge-m3` (IMPLEMENTADO — commit `2837ec0`)

### Contexto
O `nomic-embed-text` é primariamente inglês; atributos em PT separavam mal. Tentativa final na semântica pura:
trocar por modelo multilíngue.

### Decisão
- Modelo passa a ser **`bge-m3`** (multilíngue). **NÃO usa** prefixos `search_query:`/`search_document:`
  (query e documento simétricos) — a lógica de prefixo saiu do `ServicoEmbeddingOllama` (o parâmetro
  `TipoTextoEmbedding` foi mantido na interface só para permitir voltar ao nomic com baixo atrito).
- **bge-m3 = 1024 dimensões** (nomic era 768) → migration `EmbeddingBgeM3Vetor1024` altera as colunas
  `embedding` de `vector(768)` → `vector(1024)` nas tabelas `animais` e `fragmentos_animal` (limpa vetores
  antigos + recria HNSW). Reversível via `Down` (volta a 768).
- Custo: bge-m3 é maior/mais lento na geração (regerar 150 fragmentos ~140s vs ~50s do nomic).

### Efeito medido (modo Semantica, multi-vetor)
| Query | nomic (768) | bge-m3 (1024) |
|-------|-------------|---------------|
| "voar" | Águia #1, 0.602 (gap 0.088) | Águia #1, **0.784 (gap 0.219)** — muito melhor |
| "tem asas" | Cobra #1 ❌ | Águia #1 / Papagaio #3 — melhor (Tubarão intruso #2) |
| "animais que voam" | amontoado | amontoado (diluição da **consulta**, não do documento) |
| "predador dos oceanos" | Tubarão #1, 0.638 ✅ | Lobo/Águia/Leão empatam 0.798 > Tubarão #4 ❌ — **regrediu** |

### Conclusão
A busca **puramente semântica** atingiu o teto: ótima para conceito/atributo-com-tag, fraca em **palavra
literal** ("oceanos", "asas") e **frase com enchimento** ("animais que voam"). O empate 0.798 em
"predador dos oceanos" revela que bge-m3 "crava" a tag literal `predador` (texto idêntico em 3 animais →
chunks idênticos → scores idênticos) e ignora "oceanos". **Decisão: manter bge-m3 e seguir para o Híbrido
(F6/RRF)**, onde o FTS ancora a palavra literal e o semântico entra para o conceito.

## Busca híbrida (F6/RRF) — comparativo medido e novo gargalo (commits `844d6bb`, `c94493b`)

### Resultado (4 consultas × 3 modos)
- ✅ "tem asas" → **Híbrido venceu**: FTS achou as 2 aves, RRF as levou a #1/#2 e jogou o intruso (Tubarão) pra baixo.
- ✅ "voar" → Híbrido reforçou Águia #1 / Papagaio #2.
- ❌ "predador dos oceanos" e "animais que voam" → **FTS voltou VAZIO**, então Híbrido = Semântica (sem resgate).

### Diagnóstico — o gargalo migrou para a RECALL do FTS
O RRF está correto; ele só não tem o que fundir quando o FTS não dispara. O FTS volta vazio por dois motivos:
1. **AND**: `ServicoBuscaTextual` faz `Replace(" ", " & ")` → exige TODOS os termos no mesmo documento.
2. **Campos não indexados**: o gatilho do `search_vector` cobre nome + descrição + características + curiosidades,
   mas **não** `distribuicao_geografica` (onde está "oceanos") nem `tags`.

### Soluções priorizadas (por custo/benefício)
1. **[F6.1] Recall do FTS** (maior retorno): trocar AND→OR no `tsquery` (ts_rank já premia quem casa mais termos);
   incluir `distribuicao_geografica` + `tags` no gatilho; `unaccent` (acento-insensível). Custo: 1 migration + ajuste no serviço.
2. **[F6.1] Tags contextualizadas**: no `FragmentadorAnimal`, embedar `"{NomeComum}: {tag}"` em vez da tag pura →
   elimina o empate de chunks idênticos ("predador") e melhora a precisão da semântica. Custo: 1 linha + regerar.
3. **[F6.2] Geração em lote**: o handler cria um `OllamaEmbeddingGenerator` por fragmento e chama 1 a 1 →
   reusar instância + `GenerateAsync` em lote corta o tempo (bge-m3 é lento). Custo: refactor pequeno.
4. **[F6.2] Normalizar pontuação do RRF** (0–1) no DTO — problema de apresentação, resolver na borda.
5. **Higiene (baixa urgência)**: mover handler de `GerarEmbeddings` p/ Application (via `IServicoPersistenciaEmbedding`);
   remover o parâmetro `TipoTextoEmbedding` morto quando a escolha de modelo estabilizar.

### Não fazer agora
Trocar de modelo de novo (bge-m3 cobre o que falta via híbrido) e tunar pesos do RRF (sem evidência de que peso
igual seja ruim) — otimização prematura.