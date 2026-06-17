# Tutorial 04 — Busca Semântica com Embeddings

Neste tutorial, vamos entender como o Deep Sparrow vai além das palavras-chave e consegue encontrar animais pelo **significado** da consulta. Você vai aprender o que são embeddings, por que o projeto usa fragmentos (chunks) em vez de um vetor por animal, como o pipeline de geração funciona e como a busca semântica é executada.

---

## O que são embeddings?

Um embedding é uma representação numérica de um texto — um vetor de números decimais. O que torna essa representação especial é que textos com significado parecido geram vetores parecidos, e textos com significado diferente geram vetores distantes.

Imagine um espaço com 1024 dimensões (o modelo que usamos aqui). Cada texto é um ponto nesse espaço. Depois de treinar com bilhões de exemplos, o modelo aprendeu a posicionar:

- `predador` e `carnivoro` perto um do outro
- `especie em risco` perto de `vulneravel` e `em perigo`
- `animal aquatico` perto de `oceano`, `rio` e `peixe`

Isso significa que uma busca por `predador` pode encontrar animais cujos textos usam `carnivoro`, mesmo sem a palavra exata. A busca textual do Tutorial 03 nunca conseguiria fazer isso.

---

## Por que o modelo bge-m3?

O projeto usa o modelo `bge-m3` por três razões:

**Multilíngue**: o `bge-m3` foi treinado em mais de 100 idiomas e coloca português e inglês no mesmo espaço vetorial. Isso significa que a busca por `shark` pode encontrar animais descritos em português como "tubarão" — os vetores ficam próximos independentemente do idioma.

**1024 dimensões**: dimensionalidade suficiente para capturar nuances semânticas, sem ser excessivamente pesado para rodar localmente.

**Roda via Ollama, sem custo por chamada**: toda a geração de embeddings acontece no container local do Ollama. Não há chamadas para APIs externas, sem custo variável e sem dependência de internet após o download inicial do modelo.

O modelo é configurado como constante no `ServicoEmbeddingOllama`:

```csharp
// bge-m3: modelo multilingue (1024 dimensoes). NAO usa prefixos de tarefa.
private const string Modelo = "bge-m3";
```

O comentário "NAO usa prefixos de tarefa" é importante: alguns modelos esperam prefixos como `"query: "` ou `"passage: "` para distinguir consultas de documentos. O `bge-m3` não usa esse padrão — o mesmo formato é usado tanto para indexar os fragmentos dos animais quanto para codificar a consulta do usuário.

---

## O problema da diluição semântica

Uma abordagem ingênua seria gerar um único vetor por animal, concatenando todo o texto (nome, descrição, características, curiosidades) e enviando para o modelo. Isso parece razoável, mas tem um problema: quanto mais longo o texto, mais o vetor vai sendo "diluído" com todos os temas mencionados.

Imagine um animal que vive em `oceano` mas tem uma longa descrição sobre reprodução terrestre. Um vetor único vai misturar esses dois temas. Se alguém buscar por `animal aquatico`, o vetor do animal pode estar longe do vetor da consulta, mesmo que o animal seja essencialmente marinho.

A solução é dividir o texto em **fragmentos menores** e gerar um vetor por fragmento. Cada fragmento captura um aspecto específico do animal. Na hora da busca, comparamos a consulta com todos os fragmentos e pegamos o mais próximo (técnica chamada de **max-sim**, ou máxima similaridade).

---

## FragmentadorAnimal — como o texto é dividido

A classe `FragmentadorAnimal` em `backend/src/Buscador.Application/Funcionalidades/GerarEmbeddings/FragmentadorAnimal.cs` define como cada animal é dividido:

```csharp
public static IReadOnlyList<string> Fragmentar(Animal animal)
{
    var fragmentos = new List<string>();

    if (!string.IsNullOrWhiteSpace(animal.NomeComum))
        fragmentos.Add(animal.NomeComum.Trim());

    fragmentos.AddRange(DividirEmFrases(animal.Descricao));
    fragmentos.AddRange(DividirEmFrases(animal.Caracteristicas));
    fragmentos.AddRange(DividirEmFrases(animal.Curiosidades));

    // Tag contextualizada com o nome: "Lobo: predador" != "Leao: predador",
    // evitando chunks identicos (e scores empatados) entre animais que compartilham a mesma tag.
    foreach (var tag in animal.Tags)
        if (!string.IsNullOrWhiteSpace(tag))
            fragmentos.Add($"{animal.NomeComum}: {tag.Trim()}");

    return fragmentos
        .Where(f => !string.IsNullOrWhiteSpace(f))
        .Distinct()
        .ToList();
}
```

Os fragmentos gerados para cada animal são:

1. **O nome comum** — um fragmento curto e direto, com o nome do animal.
2. **Cada frase da Descricao** — dividida nos marcadores `.`, `!` ou `?` seguidos de espaço.
3. **Cada frase de Caracteristicas** — idem.
4. **Cada frase de Curiosidades** — idem.
5. **Cada tag contextualizada** — em vez de guardar apenas `predador`, o fragmento é `Lobo: predador`. Isso evita que dois animais com a mesma tag gerem vetores idênticos, o que causaria empates e reduziria a precisão da busca.

Um animal típico gera entre 10 e 20 fragmentos. O `.Distinct()` ao final remove duplicatas caso a mesma frase apareça em mais de um campo.

---

## O pipeline de geração de embeddings

O endpoint `POST /api/animais/embeddings/gerar` dispara o `GerarEmbeddingsComandoManipulador`. Veja o que acontece dentro do handler:

```csharp
// Passo 1: quais animais ainda não têm fragmentos?
var ids = await _persistencia.ObterIdsSemFragmentosAsync(cancellationToken);

foreach (var id in ids)
{
    var animal = await _repositorio.ObterPorIdAsync(AnimalId.De(id), cancellationToken);
    if (animal is null) continue;

    // Passo 2: dividir o animal em fragmentos de texto
    var fragmentos = FragmentadorAnimal.Fragmentar(animal);
    if (fragmentos.Count == 0) continue;

    // Passo 3: enviar todos os fragmentos do animal de uma vez para o Ollama (batch)
    var vetores = await _servicoEmbedding.GerarVariosAsync(fragmentos, cancellationToken);

    // Passo 4: salvar cada par (texto, vetor) na tabela fragmentos_animal
    for (var j = 0; j < fragmentos.Count; j++)
    {
        await _persistencia.InserirFragmentoAsync(id, fragmentos[j], vetores[j], cancellationToken);
    }
}
```

O passo 3 usa `GerarVariosAsync` — uma chamada em lote para o Ollama — em vez de uma chamada por fragmento. Isso reduz o overhead de rede e aproveita a capacidade de processamento em paralelo do modelo.

A persistência usa SQL cru:

```csharp
await _contexto.Database.ExecuteSqlRawAsync(
    "INSERT INTO fragmentos_animal (id, animal_id, texto, embedding) VALUES ({0}, {1}, {2}, {3}::vector)",
    Guid.NewGuid(), animalId, texto, vetorString);
```

O vetor é serializado como uma string no formato `[0.1,-0.3,0.7,...]` e a notação `::vector` faz o PostgreSQL convertê-lo para o tipo `vector(1024)` da extensão pgvector.

---

## Como a busca semântica funciona

Quando você faz uma busca semântica, o `ServicoBuscaSemantica` executa o seguinte:

### Passo 1 — Gerar o vetor da consulta

```csharp
var vetorConsulta = await _servicoEmbedding.GerarAsync(consulta.Trim(), cancellationToken);
```

A consulta do usuário é enviada para o Ollama, que retorna um vetor de 1024 floats. Esse vetor representa o significado da consulta no mesmo espaço onde estão os vetores dos fragmentos dos animais.

### Passo 2 — Busca max-sim no banco

```csharp
var scores = await _contexto.Database
    .SqlQuery<IdComPontuacao>(
        $"""
        SELECT a.id AS "Id", MIN(f.embedding <=> {vetorString}::vector) AS "Pontuacao"
        FROM fragmentos_animal f
        JOIN animais a ON a.id = f.animal_id
        WHERE f.embedding IS NOT NULL
        GROUP BY a.id
        ORDER BY MIN(f.embedding <=> {vetorString}::vector) ASC
        LIMIT {limite}
        """)
    .ToListAsync(cancellationToken);
```

O operador `<=>` do pgvector calcula a **distância de cosseno** entre dois vetores. Quanto menor a distância, mais próximos (semelhantes) os vetores são.

O `MIN(f.embedding <=> vetor)` é a estratégia max-sim: para cada animal, pegamos o fragmento que está mais próximo da consulta (menor distância). Isso garante que um animal seja bem ranqueado se pelo menos um de seus fragmentos for relevante, mesmo que outros fragmentos não sejam.

O `GROUP BY a.id` agrupa os fragmentos por animal, e o `ORDER BY ... ASC` coloca primeiro os animais com menor distância (maior similaridade).

### Passo 3 — Conversão de distância para similaridade

```csharp
return scores
    .Join(animais, s => s.Id, a => a.Id.Valor,
        (s, a) => new ResultadoBuscaDto(a.ParaDto(), 1 - s.Pontuacao))
    .ToList();
```

A distância de cosseno vai de 0 (idêntico) a 2 (oposto). Calculamos `1 - distancia` para obter uma pontuação de similaridade que vai de 1 (perfeito) a -1 (oposto). Na prática, os resultados relevantes ficam entre 0.7 e 1.0.

---

## Testando a busca semântica

Se você ainda não gerou os embeddings, faça isso primeiro:

```bash
curl -X POST http://localhost:5024/api/animais/embeddings/gerar
```

Aguarde a resposta (pode demorar alguns minutos). Depois, teste buscas semânticas:

```bash
# "especie em risco" — encontra animais Vulneravel, EmPerigo e CriticamenteEmPerigo
curl "http://localhost:5024/api/animais/buscar?q=especie em risco&modo=Semantica"

# "animal aquatico" — encontra animais de Oceano e AguaDoce
curl "http://localhost:5024/api/animais/buscar?q=animal aquatico&modo=Semantica"

# Funciona em inglês também (bge-m3 é multilíngue)
curl "http://localhost:5024/api/animais/buscar?q=endangered species&modo=Semantica"
```

---

## A diferença prática entre os dois modos de busca

Para fixar a diferença, compare os dois modos com a mesma consulta:

```bash
# Textual: busca pela palavra exata "predador" no texto indexado
curl "http://localhost:5024/api/animais/buscar?q=predador&modo=Textual"

# Semantica: busca pelo conceito de "predador" — encontra carnivoros mesmo sem a palavra
curl "http://localhost:5024/api/animais/buscar?q=predador&modo=Semantica"
```

A busca textual vai retornar apenas animais cujo texto contenha a palavra `predador` (ou variações normalizadas). A busca semântica vai retornar animais cujos fragmentos estejam próximos do conceito de predador, incluindo animais descritos apenas como `carnivoro` ou `caçador`.

Outro exemplo que ilustra bem a diferença:

- Textual com `vulneravel`: só encontra animais que têm essa palavra literalmente no texto.
- Semântica com `especie em risco`: encontra animais com qualquer nível de ameaça (Vulneravel, EmPerigo, CriticamenteEmPerigo), porque esses conceitos ficam próximos no espaço vetorial do `bge-m3`.

O próximo passo natural é combinar os dois modos em um **ranking híbrido** usando Reciprocal Rank Fusion (RRF), que é o assunto de outro tutorial.
