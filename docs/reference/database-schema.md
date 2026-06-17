# Schema do Banco de Dados

Referencia completa das tabelas, colunas, constraints e indices do banco PostgreSQL do projeto Deep Sparrow.

O banco utiliza duas extensoes: `vector` (pgvector — vetores de embedding) e `unaccent` (normalizacao de texto para busca full-text).

---

## Tabela `animais`

Armazena o catalogo de animais. Cada linha representa um animal com todos os seus campos descritivos, alem dos campos de busca full-text e busca semantica.

### Colunas

| Coluna | Tipo | Nullable | Descricao |
|---|---|---|---|
| `id` | `uuid` | NOT NULL | Chave primaria. Gerado pela aplicacao (GUID). |
| `nome_comum` | `text` | NOT NULL | Nome popular do animal (ex.: "Onca-pintada"). |
| `nome_cientifico` | `text` | NOT NULL | Nome cientifico em nomenclatura binomial (ex.: "Panthera onca"). |
| `descricao` | `text` | NOT NULL | Descricao geral do animal. |
| `caracteristicas` | `text` | NOT NULL | Caracteristicas fisicas e comportamentais. |
| `dieta` | `text` | NOT NULL | Nome do enum de dieta (`Carnivoro`, `Herbivoro`, `Onivoro`, etc.). |
| `habitat` | `text` | NOT NULL | Tipo de ambiente onde o animal vive. |
| `distribuicao_geografica` | `text` | NOT NULL | Regioes geograficas onde a especie ocorre. |
| `status_conservacao` | `text` | NOT NULL | Status na lista de conservacao (ex.: "Vulneravel", "Em perigo"). |
| `tags` | `text[]` | NOT NULL | Array de palavras-chave para categorizar o animal. |
| `curiosidades` | `text` | NOT NULL | Fatos curiosos sobre o animal. |
| `search_vector` | `tsvector` | NULL | Vetor de busca full-text. Populado automaticamente pelo gatilho `fn_atualizar_vetor_busca` em INSERT e UPDATE. |
| `embedding` | `vector(1024)` | NULL | Vetor de embedding semantico gerado pelo modelo `bge-m3` (1024 dimensoes). Preenchido via `POST /api/animais/embeddings/gerar`. |

### Constraints

| Nome | Tipo | Coluna(s) |
|---|---|---|
| `PK_animais` | PRIMARY KEY | `id` |

### Indices

| Nome | Tipo | Coluna | Proposito |
|---|---|---|---|
| `ix_animais_search_vector` | GIN | `search_vector` | Acelera consultas de busca full-text com `@@` (operador tsquery). |
| `ix_animais_embedding` | HNSW (`vector_cosine_ops`) | `embedding` | Acelera busca por vizinhos mais proximos (ANN) usando distancia de cosseno para busca semantica. |

### Gatilho de busca full-text

A migration `RecallFtsUnaccentMaisCampos` criou a funcao `fn_atualizar_vetor_busca`, que e executada como gatilho BEFORE INSERT OR UPDATE na tabela `animais`.

O gatilho constroi o `search_vector` concatenando os campos de texto e aplicando `unaccent` antes de `to_tsvector` com dicionario `portuguese`:

```sql
NEW.search_vector :=
  to_tsvector('portuguese', unaccent(
    coalesce(NEW.nome_comum, '') || ' ' ||
    coalesce(NEW.nome_cientifico, '') || ' ' ||
    coalesce(NEW.descricao, '') || ' ' ||
    coalesce(NEW.caracteristicas, '') || ' ' ||
    coalesce(NEW.curiosidades, '') || ' ' ||
    coalesce(NEW.distribuicao_geografica, '') || ' ' ||
    coalesce(array_to_string(NEW.tags, ' '), '')
  ));
```

O uso de `unaccent` garante que buscas sem acento (ex.: "onca") encontrem documentos com acento (ex.: "Onca-pintada").

---

## Tabela `fragmentos_animal`

Armazena fragmentos (chunks) de texto derivados dos animais para a estrategia de busca semantica multi-vetor com max-sim.
Cada animal pode ter multiplos fragmentos; cada fragmento tem seu proprio embedding.

### Colunas

| Coluna | Tipo | Nullable | Descricao |
|---|---|---|---|
| `id` | `uuid` | NOT NULL | Chave primaria do fragmento. Gerado pela aplicacao. |
| `animal_id` | `uuid` | NOT NULL | FK para `animais.id`. Identifica a qual animal o fragmento pertence. |
| `texto` | `text` | NOT NULL | Trecho de texto do animal que foi segmentado para embedding. |
| `embedding` | `vector(1024)` | NULL | Vetor de embedding do fragmento gerado pelo modelo `bge-m3` (1024 dimensoes). |

### Constraints

| Nome | Tipo | Coluna(s) | Detalhe |
|---|---|---|---|
| `pk_fragmentos_animal` | PRIMARY KEY | `id` | — |
| `fk_fragmentos_animal_animais` | FOREIGN KEY | `animal_id` -> `animais.id` | ON DELETE CASCADE: exclui os fragmentos automaticamente quando o animal e deletado. |

### Indices

| Nome | Tipo | Coluna | Proposito |
|---|---|---|---|
| `ix_fragmentos_animal_animal_id` | B-tree | `animal_id` | Acelera a busca de todos os fragmentos de um animal especifico. |
| `ix_fragmentos_animal_embedding` | HNSW (`vector_cosine_ops`) | `embedding` | Acelera busca por vizinhos mais proximos (ANN) para encontrar os fragmentos semanticamente mais proximos de uma consulta. |

---

## Historico de migrations

| Migration | Data | O que fez |
|---|---|---|
| `CriacaoInicial` | 2026-06-15 | Criou a tabela `animais`, adicionou `embedding vector(768)`, indices GIN e HNSW iniciais. |
| `AdicionarFragmentosAnimal` | 2026-06-16 | Criou a tabela `fragmentos_animal` com `embedding vector(768)` e seus indices. |
| `EmbeddingBgeM3Vetor1024` | 2026-06-16 | Migrou ambas as colunas `embedding` de 768 para 1024 dimensoes (modelo bge-m3). Recriou os indices HNSW. |
| `RecallFtsUnaccentMaisCampos` | 2026-06-16 | Criou a extensao `unaccent`, recriou a funcao gatilho `fn_atualizar_vetor_busca` incluindo `unaccent` e os campos `distribuicao_geografica` e `tags`. |

---

## Relacao entre as tabelas

```
animais (1)
  └── fragmentos_animal (N)   ON DELETE CASCADE
```

Um animal pode ter zero ou mais fragmentos. Os fragmentos sao gerados durante o processo de embedding e excluidos automaticamente ao se deletar o animal pai.
