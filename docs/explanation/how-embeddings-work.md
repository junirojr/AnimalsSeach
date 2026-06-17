# O que São Embeddings e Como Funcionamos com Eles

## O problema que embeddings resolvem

Busca textual encontra palavras. Ela não entende significado.

Se um animal é descrito como *"predador que caça em matilha"* e o usuário digita *"animal social de caça em grupo"*, a busca FTS não encontra nada — as palavras não coincidem. Embeddings resolvem isso.

## O que é um embedding?

Um embedding é uma lista de números (vetor) que representa o **significado** de um texto num espaço de alta dimensão. Textos com significado parecido ficam próximos nesse espaço; textos com significado diferente ficam distantes.

O modelo `bge-m3` (usado no projeto) produz vetores de **1024 dimensões**. Cada dimensão captura alguma faceta semântica do texto — o modelo aprendeu essas facetas treinando em bilhões de frases, em vários idiomas.

```
"caça em grupo"    → [0.12, -0.45, 0.78, ...]  (1024 floats)
"predador social"  → [0.11, -0.43, 0.80, ...]  (muito próximo!)
"fotossíntese"     → [-0.92, 0.31, -0.10, ...]  (distante)
```

> **Por que `bge-m3` e não `nomic-embed-text`?** O projeto começou com o `nomic-embed-text` (768 dim),
> mas ele é primariamente inglês e separava mal atributos em português ("voar", "tem asas"). Trocamos
> pelo `bge-m3` — multilíngue, 1024 dim — que entende PT bem melhor. (Detalhes da decisão e dos números
> medidos no `docs/ai/PROJECT_RISK_REGISTER.md`.) Diferente do nomic, o bge-m3 **não usa prefixos de
> tarefa** (`search_query:`/`search_document:`): trata consulta e documento de forma simétrica.

## Cosine similarity (similaridade cosseno)

Para medir a proximidade entre dois vetores usamos a **distância cosseno**, que mede o ângulo entre eles:

- **Distância = 0** → vetores idênticos (mesmo significado)
- **Distância = 1** → vetores perpendiculares (sem relação)
- **Distância = 2** → vetores opostos (significados contrários)

No pgvector o operador é `<=>` e a consulta ordena por distância **ASC** (menor = mais similar). Convertemos distância em **pontuação de similaridade** na aplicação: `Pontuacao = 1 - distancia`.

## Multi-vetor: um animal vira VÁRIOS vetores

Aqui está a parte mais importante da arquitetura atual. Não guardamos **um** vetor por animal — guardamos **vários** (chamados *fragmentos* ou *chunks*).

**Por quê?** Um único vetor é a *média* (mean pooling) de todo o texto. Se a descrição tem ~200 palavras e o atributo "voo" aparece só uma vez, esse sinal **dilui** na média — como uma gota de tinta num balde. A busca por "voar" não destacava a Águia.

**A solução:** quebrar cada animal em pedaços e gerar um embedding por pedaço, guardados na tabela `fragmentos_animal(id, animal_id, texto, embedding vector(1024))`. O `FragmentadorAnimal` (na camada Application, lógica pura) divide assim:

- o **NomeComum**;
- cada **frase** de `Descricao`, `Caracteristicas` e `Curiosidades` (split em `.!?`);
- cada **tag** isolada, **contextualizada com o nome**: `"Águia-real: voo"`, `"Lobo: predador"`.

> **Por que contextualizar a tag com o nome?** Se embedássemos a tag pura "predador", Lobo, Leão e Águia
> teriam chunks **idênticos** → vetores idênticos → empate de pontuação. Prefixar o nome
> (`"Lobo: predador"` ≠ `"Leão: predador"`) torna cada vetor único e adiciona um sinal desambiguador.

## Como funciona no Deep Sparrow

### 1. Indexação (geração dos embeddings)

Ao chamar `POST /api/animais/embeddings/gerar`, o sistema:

1. Consulta via SQL os animais que **ainda não têm fragmentos** (`WHERE NOT EXISTS (SELECT 1 FROM fragmentos_animal ...)`) — operação idempotente.
2. Para cada animal, o `FragmentadorAnimal` produz a lista de fragmentos (nome + frases + tags contextualizadas).
3. Cada fragmento é enviado ao Ollama (`bge-m3`) e vira um vetor de 1024 floats.
4. Cada vetor é gravado via `INSERT INTO fragmentos_animal (id, animal_id, texto, embedding) VALUES (...)`.

As colunas `embedding` (em `animais` e `fragmentos_animal`) são do tipo `vector(1024)` e **não são mapeadas pelo EF Core** — o acesso é sempre via SQL bruto, para usar os operadores do pgvector sem acoplar o ORM à extensão.

> **Para regenerar:** como a idempotência é por "animal sem fragmentos", para reindexar é preciso limpar
> antes: `DELETE FROM fragmentos_animal;` e então chamar o `POST` de novo. (Necessário sempre que o texto
> dos fragmentos muda — ex.: mudou o `FragmentadorAnimal` — ou que se troca o modelo.)

### 2. Busca semântica (max-sim)

Ao chamar `GET /api/animais/buscar?q=...&modo=Semantica`:

1. O texto da consulta vira um vetor pelo mesmo modelo (`bge-m3`).
2. O PostgreSQL calcula, para **cada animal**, a **menor distância** entre o vetor da consulta e qualquer um dos seus fragmentos (estratégia *max-sim*):

```sql
SELECT a.id, MIN(f.embedding <=> q) AS distancia
FROM fragmentos_animal f
JOIN animais a ON a.id = f.animal_id
WHERE f.embedding IS NOT NULL
GROUP BY a.id
ORDER BY distancia ASC
LIMIT k;
```

3. O animal pontua pelo seu **melhor** fragmento, não pela média — é isso que faz a tag "voo" da Águia "ganhar" sozinha, sem a descrição inteira puxar pra baixo.

### 3. E quando a semântica não basta?

Embeddings entendem **conceito**, mas erram **palavra literal** ("oceanos", "asas") e frases com enchimento ("animais que voam"). Para isso existe o **modo Híbrido**, que funde a busca semântica com a FTS via *Reciprocal Rank Fusion* (RRF) — ver `how-fts-works.md`.

## Limitações

- **Embeddings estáticos**: o vetor não muda quando a descrição é editada. É preciso regenerar (`DELETE` + `POST`).
- **Sensível ao modelo**: vetores de modelos diferentes não são comparáveis, e a dimensão faz parte do schema (`vector(1024)`). Trocar de modelo exige migration + reindexação total.
- **Fraco em multi-conceito / palavra literal**: ex.: "predador dos oceanos" tende a destacar predadores genéricos. Coberto pelo modo Híbrido (FTS + RRF).
- **Ollama obrigatório**: sem o serviço Ollama no ar, buscas semânticas falham.
