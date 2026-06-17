# Por Que PostgreSQL + pgvector e Nao um Banco Dedicado?

Quando o assunto e busca vetorial, e natural pensar em ferramentas especializadas como
Elasticsearch, Weaviate ou Qdrant. Este documento explica por que o projeto Buscador usa
PostgreSQL com a extensao pgvector, e quando essa escolha começa a nao ser mais ideal.

---

## 1. Busca fulltext ja vem com o PostgreSQL

O PostgreSQL tem suporte nativo a busca fulltext (FTS) via os tipos `tsvector` e `tsquery`.
Nao e um recurso add-on — faz parte do banco desde a versao 7.

No projeto, cada animal tem uma coluna `search_vector` do tipo `tsvector`, mantida
automaticamente por um gatilho:

```sql
-- Migration GatilhoVetorBusca
CREATE OR REPLACE FUNCTION fn_atualizar_vetor_busca()
RETURNS trigger AS $$
BEGIN
  NEW.search_vector :=
    to_tsvector('portuguese',
      coalesce(NEW.nome_comum,   '') || ' ' ||
      coalesce(NEW.nome_cientifico, '') || ' ' ||
      coalesce(NEW.descricao,    '') || ' ' ||
      coalesce(NEW.caracteristicas, '') || ' ' ||
      coalesce(NEW.curiosidades, '')
    );
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

O argumento `'portuguese'` instrui o PostgreSQL a usar o dicionario de stemming em portugues:
"correndo" e "correr" viram o mesmo lexema, "animais" e "animal" tambem. O `unaccent` usado
nas queries torna a busca insensivel a acentos — buscar "Onca" encontra "Onca-pintada".

Isso significa que FTS funciona no mesmo banco, sem nenhum servico extra, sem sincronizacao
de indices, sem ficar desatualizado.

---

## 2. Embeddings e texto na mesma base, sem sincronizacao

A extensao `pgvector` adiciona o tipo `vector(N)` ao PostgreSQL. No projeto, a tabela
`fragmentos_animal` tem uma coluna `embedding vector(1024)`:

```sql
CREATE TABLE fragmentos_animal (
    id uuid NOT NULL,
    animal_id uuid NOT NULL,
    texto text NOT NULL,
    embedding vector(1024) NULL,
    CONSTRAINT fk_fragmentos_animal_animais FOREIGN KEY (animal_id)
        REFERENCES animais(id) ON DELETE CASCADE
);
CREATE INDEX ix_fragmentos_animal_embedding
    ON fragmentos_animal USING hnsw (embedding vector_cosine_ops);
```

O indice HNSW (Hierarchical Navigable Small World) acelera a busca por vizinhos mais proximos
— o mesmo tipo de indice que bancos vetoriais dedicados usam internamente.

A vantagem de ter texto e vetor no mesmo banco e que o `JOIN` entre as tabelas e uma operacao
local, dentro da mesma transacao:

```sql
-- ServicoBuscaSemantica.cs
SELECT a.id AS "Id", MIN(f.embedding <=> $vetor::vector) AS "Pontuacao"
FROM fragmentos_animal f
JOIN animais a ON a.id = f.animal_id
WHERE f.embedding IS NOT NULL
GROUP BY a.id
ORDER BY MIN(f.embedding <=> $vetor::vector) ASC
LIMIT $limite
```

Com Weaviate ou Qdrant, esse `JOIN` exigiria uma chamada HTTP ao banco vetorial, depois
outra consulta ao banco relacional para buscar os dados completos do animal, e depois
combinar os resultados no codigo da aplicacao. Com pgvector, tudo acontece numa unica query.

---

## 3. Sem infra extra: uma conexao, um backup, um schema

Com pgvector, o projeto inteiro precisa de exatamente um servico de banco de dados:

```yaml
# docker-compose.yml (resumo)
services:
  postgres:
    image: pgvector/pgvector:pg16
    environment:
      POSTGRES_DB: buscador
      POSTGRES_USER: buscador
      POSTGRES_PASSWORD: buscador
```

Isso tem consequencias praticas importantes:

**Migrations unificadas**: o schema de FTS (tabela `animais` com `search_vector`), o schema
relacional (enums como `dieta`, `habitat`) e o schema vetorial (tabela `fragmentos_animal`
com `embedding vector(1024)`) sao todos gerenciados pelo mesmo `dotnet ef migrations add`.
Voce ve o historico completo do banco num unico lugar.

**Uma unica string de conexao**: `ConnectionStrings:Postgres` em `appsettings.json` e tudo
que a aplicacao precisa para acessar texto, vetores e metadados.

**Backup simples**: um `pg_dump` captura tudo — animais, fragmentos, embeddings, indices.

**Desenvolvimento local trivial**: `docker compose up -d` sobe o PostgreSQL com pgvector ja
habilitado. Nao ha um segundo container de banco vetorial para configurar, autenticar ou
sincronizar.

---

## 4. Quando pgvector nao e suficiente

pgvector e excelente para o tamanho deste projeto, mas tem limitacoes reais:

**Volume de vetores**: com milhoes de vetores, o HNSW do pgvector começa a ficar atras de
implementacoes especializadas. Qdrant e Weaviate tem arquiteturas otimizadas para busca
vetorial em escala — sharding automatico, indices distribuidos, filtros pre-computados.

**Atualizacao constante em tempo real**: se vetores sao inseridos e deletados
continuamente em alta frequencia, o HNSW precisa de reindexacao periodica. Bancos vetoriais
dedicados lidam melhor com esse padrao de acesso.

**Busca vetorial como produto principal**: se a aplicacao e um sistema de recomendacao com
bilhoes de vetores e latencia de milissegundos como SLA, vale o custo operacional de um
banco especializado.

**Filtragem vetorial complexa**: Qdrant e Weaviate oferecem filtros pre-indexados (payload
filters) que permitem combinar busca vetorial com filtros de metadados de forma mais
eficiente do que uma clausula `WHERE` no PostgreSQL.

---

## 5. Por que pgvector e a escolha certa para este projeto

O catalogo do Buscador tem 52 animais. Com entre 10 e 20 fragmentos por animal, isso da
aproximadamente 780 linhas na tabela `fragmentos_animal` — 780 vetores de 1024 dimensoes.

Para essa escala, um banco vetorial dedicado seria um canhao para matar uma formiga. O custo
operacional (mais um servico para subir, configurar, autenticar, monitorar e fazer backup)
nao traria nenhum beneficio pratico de performance.

Alem disso, o objetivo do projeto e de aprendizado: C# / .NET 10, Clean Architecture, DDD,
pgvector, Ollama. Concentrar a complexidade em menos servicos deixa mais espaco mental para
aprender os conceitos centrais sem se perder em detalhes de infraestrutura.

O quadro abaixo resume quando cada opcao faz sentido:

| Criterio | pgvector | Qdrant / Weaviate |
|---|---|---|
| Volume de vetores | ate ~1M | acima de 1M |
| Infra ja existente | PostgreSQL ja em uso | banco relacional separado aceitavel |
| Joins com dados relacionais | JOIN local, sem overhead | chamada HTTP extra |
| Atualizacao em tempo real | reindexacao periodica | otimizado para inserts frequentes |
| Custo operacional | baixo (um servico) | maior (dois servicos) |
| **Este projeto (780 vetores)** | **ideal** | desnecessario |
