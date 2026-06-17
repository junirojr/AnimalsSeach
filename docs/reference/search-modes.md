# Referencia — Modos de Busca

Referencia objetiva dos tres modos de busca disponiveis no endpoint `GET /api/animais/buscar`.

---

## Comparacao rapida

| Modo       | Tecnica                          | Indice usado                         | Quando brilha                              | Requisito especial         |
|------------|----------------------------------|--------------------------------------|--------------------------------------------|----------------------------|
| `Textual`  | FTS com tsvector + tsquery       | Indice GIN em `search_vector`        | Palavras exatas, nomes cientificos, enums  | Nenhum                     |
| `Semantica`| Distancia coseno via pgvector    | Indice HNSW em `fragmentos_animal`   | Conceitos, sinonimos, linguagem natural    | Embeddings gerados         |
| `Hibrida`  | Textual + Semantica + RRF (k=60) | Ambos os indices                     | Melhor qualidade geral                     | Embeddings gerados         |

---

## Modo Textual

### O que usa

Busca full-text nativa do PostgreSQL com `tsvector` e `tsquery`. O campo `search_vector` da
tabela `animais` e um `tsvector` precomputado indexado com GIN. A extensao `unaccent` torna a
busca insensivel a acentos.

Idioma configurado: `portuguese` (stemmer do PostgreSQL para portugues).

### Como pontua os resultados

Usa `ts_rank(search_vector, query)`. O valor nao tem limite definido — depende da frequencia
e da posicao do termo no documento. O campo `pontuacao` no retorno da API reflete esse valor
bruto (sem normalizacao no modo Textual puro).

A query e construida com operador `OR` entre tokens, apos aplicar `unaccent` e `to_tsquery`.

### Melhor para

- Busca por nome comum ou cientifico: `"onca"`, `"Panthera onca"`
- Busca por campos de enum que aparecem como texto: `"carnivoro"`, `"vulneravel"`
- Situacoes onde embeddings nao foram gerados ainda
- Consultas que exigem correspondencia exata de termos

### Limitacoes

- Nao entende sinonimos: `"felino"` pode nao encontrar `"gato-do-mato"` se o campo nao contiver
  a palavra exata
- Sensivel ao vocabulario: `"em risco"` nao encontra `"status vulneravel"` sem stemming adequado
- Nao interpreta intencao semantica de frases completas

---

## Modo Semantica

### O que usa

Embeddings de 1024 dimensoes gerados pelo modelo `bge-m3` via Ollama, armazenados na tabela
`fragmentos_animal` com o tipo `vector(1024)` do pgvector. A busca usa o operador de distancia
coseno `<=>`.

Cada animal possui entre 10 e 20 fragmentos (um por campo: nome, descricao, caracteristicas,
curiosidades, tags, etc.). O algoritmo **max-sim** seleciona o fragmento mais proximo da
consulta para representar o animal.

### Como pontua os resultados

```sql
SELECT a.id, MIN(f.embedding <=> :vetor::vector) AS distancia
FROM fragmentos_animal f
JOIN animais a ON a.id = f.animal_id
WHERE f.embedding IS NOT NULL
GROUP BY a.id
ORDER BY distancia ASC
```

A pontuacao retornada e `1 - distancia_coseno`, normalizada entre `0` e `1`. Valor `1.0`
indica maxima similaridade; valores abaixo de `0.5` indicam baixa relevancia semantica.

### Melhor para

- Consultas em linguagem natural: `"animal que vive em rios e come peixe"`
- Busca por conceitos sem palavra exata: `"especie em risco de extincao"` encontra animais com
  status `Vulneravel` ou `EmPerigo` mesmo sem a palavra "risco" no texto
- Sinonimos e variantes: `"predador"` encontra `"carnivoro"`, `"cacador"`
- Consultas em que o usuario nao sabe o nome exato do animal

### Limitacoes

- Requer que os embeddings tenham sido gerados previamente via `POST /api/animais/embeddings/gerar`
- A geracao de embeddings depende do container Ollama em execucao
- Pode trazer resultados semanticamente relacionados mas que nao correspondem ao que o usuario
  queria de forma literal

---

## Modo Hibrida

### O que usa

Executa os modos Textual e Semantica em paralelo e combina os rankings com o algoritmo
**Reciprocal Rank Fusion (RRF)** com constante `k = 60`.

O RRF usa apenas a posicao de cada resultado em cada lista — nao os scores brutos — o que
resolve o problema de escalas incompativeis entre `ts_rank` e similaridade coseno.

### Como pontua os resultados

Cada animal recebe um score RRF calculado como:

```
score_rrf = soma de  1 / (60 + posicao_na_lista_i)
```

Apos calcular todos os scores, o resultado e normalizado: o animal de maior score recebe `1.0`,
os demais recebem valores proporcionais. Um `pontuacao = 0.5` significa que o animal tem metade
do consenso do melhor resultado.

O sistema busca um pool minimo de 20 candidatos por lado antes de aplicar o RRF, independentemente
do `limite` solicitado. Isso garante que o algoritmo tenha representantes suficientes de ambos os
sistemas para fusionar com qualidade.

### Melhor para

- Uso geral: maior qualidade de ranking na maioria das consultas
- Consultas mistas: parte da resposta vem de correspondencia textual, parte de semelhanca semantica
- Producao: combina a precisao da FTS com a flexibilidade da busca semantica

### Limitacoes

- Requer embeddings gerados (mesma restricao do modo Semantica)
- Ligeiramente mais lento que os modos individuais (executa duas buscas antes de fundir)

---

## Parametros da API

Endpoint: `GET /api/animais/buscar`

| Parametro | Tipo   | Padrao    | Valores validos              | Descricao                                      |
|-----------|--------|-----------|------------------------------|------------------------------------------------|
| `q`       | string | —         | qualquer string              | Termo de busca. Obrigatorio.                   |
| `modo`    | string | `Textual` | `Textual`, `Semantica`, `Hibrida` | Modo de busca a utilizar.             |
| `limite`  | int    | `10`      | `1` ate `100`                | Numero maximo de resultados a retornar.        |

### Exemplos de chamada

```bash
# Busca textual (padrao)
curl "http://localhost:5024/api/animais/buscar?q=onca"

# Busca semantica com limite personalizado
curl "http://localhost:5024/api/animais/buscar?q=predador+da+floresta&modo=Semantica&limite=5"

# Busca hibrida
curl "http://localhost:5024/api/animais/buscar?q=animal+aquatico+carnivoro&modo=Hibrida&limite=10"
```

### Estrutura da resposta

```json
[
  {
    "animal": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nomeComum": "Onca-pintada",
      "nomeCientifico": "Panthera onca",
      "dieta": "Carnivora",
      "habitat": "FlorestaUmida",
      "statusConservacao": "Vulneravel"
    },
    "pontuacao": 1.0
  }
]
```

O campo `pontuacao` tem significado diferente por modo:
- **Textual**: valor bruto do `ts_rank` (sem normalizacao)
- **Semantica**: similaridade coseno `1 - distancia` (0 a 1)
- **Hibrida**: score RRF normalizado (0 a 1, topo sempre = 1.0)

---

## Pre-requisito: gerar embeddings

Os modos `Semantica` e `Hibrida` requerem que os embeddings estejam gerados. Para gerar:

```bash
# Requer container Ollama em execucao com o modelo bge-m3
curl -X POST "http://localhost:5024/api/animais/embeddings/gerar"
```

Este endpoint e idempotente — pode ser chamado multiplas vezes sem duplicar dados. Novos animais
adicionados ao catalogo precisam ter seus embeddings gerados apos a insercao.
