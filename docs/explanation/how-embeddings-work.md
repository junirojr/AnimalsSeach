# O que São Embeddings e Como Funcionamos com Eles

## O problema que embeddings resolvem

Busca textual encontra palavras. Ela não entende significado.

Se um animal é descrito como *"predador que caça em matilha"* e o usuário digita *"animal social de caça em grupo"*, a busca FTS não encontra nada — as palavras não coincidem. Embeddings resolvem isso.

## O que é um embedding?

Um embedding é uma lista de números (vetor) que representa o **significado** de um texto num espaço de alta dimensão. Textos com significado parecido ficam próximos nesse espaço; textos com significado diferente ficam distantes.

O modelo `nomic-embed-text` produz vetores de 768 dimensões. Cada dimensão captura alguma faceta semântica do texto — o modelo aprendeu essas facetas treinando em bilhões de frases.

```
"caça em grupo"    → [0.12, -0.45, 0.78, ...]  (768 floats)
"predador social"  → [0.11, -0.43, 0.80, ...]  (muito próximo!)
"fotossíntese"     → [-0.92, 0.31, -0.10, ...]  (distante)
```

## Cosine similarity (similaridade cosseno)

Para medir a proximidade entre dois vetores usamos a **distância cosseno**, que mede o ângulo entre eles:

- **Distância = 0** → vetores idênticos (mesmo significado)
- **Distância = 1** → vetores perpendiculares (sem relação)
- **Distância = 2** → vetores opostos (significados contrários)

No pgvector o operador é `<=>` e a consulta ordena por distância **ASC** (menor = mais similar):

```sql
SELECT id, (embedding <=> '[0.12, -0.45, ...]'::vector) AS distancia
FROM animais
WHERE embedding IS NOT NULL
ORDER BY distancia ASC
LIMIT 5;
```

Convertemos distância em **pontuação de similaridade** na aplicação:

```csharp
Pontuacao = 1 - distancia   // 0 = sem relação, 1 = idêntico
```

## Como funciona no Deep Sparrow

### 1. Indexação (geração dos embeddings)

Ao chamar `POST /api/animais/embeddings/gerar`, o sistema:

1. Consulta via SQL todos os animais sem embedding (`WHERE embedding IS NULL`)
2. Para cada animal, concatena `Descricao + Caracteristicas + Curiosidades`
3. Envia o texto para o Ollama (`nomic-embed-text`) e recebe um vetor de 768 floats
4. Salva o vetor no banco via `UPDATE animais SET embedding = {vetor}::vector WHERE id = {id}`

O embedding é armazenado na coluna `embedding vector(768)` da tabela `animais`. Essa coluna **não é mapeada pelo EF Core** — acesso é sempre via SQL bruto para evitar dependência da extensão pgvector no ORM.

### 2. Busca semântica

Ao chamar `GET /api/animais/buscar?q=...&modo=Semantica`:

1. O texto da consulta é convertido em vetor pelo mesmo modelo (`nomic-embed-text`)
2. O PostgreSQL calcula a distância cosseno entre o vetor da consulta e todos os embeddings dos animais
3. Os N animais mais próximos são retornados, ordenados por similaridade decrescente

### 3. Por que `nomic-embed-text`?

- Modelo open-source, rodando localmente via Ollama (sem custo de API)
- 768 dimensões: bom equilíbrio entre qualidade e performance
- Treinado em textos multilíngues, incluindo português

## Limitações

- **Embeddings estáticos**: o vetor de um animal não muda automaticamente quando sua descrição é editada. É preciso re-executar a geração de embeddings após edições.
- **Sensível ao modelo**: embeddings gerados com modelos diferentes não são comparáveis. Troque o modelo apenas reindexando todos os animais.
- **Ollama obrigatório**: sem o serviço Ollama no ar, buscas semânticas falham com erro 500.
