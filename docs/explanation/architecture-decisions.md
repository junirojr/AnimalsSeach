# Decisoes Arquiteturais — Por Que Fizemos Assim

Este documento explica o raciocinio por tras das escolhas de design do projeto Buscador.
O objetivo nao e descrever *o que* existe (isso esta nos CLAUDE.md de cada camada), mas sim
*por que* cada escolha foi feita — especialmente para quem esta aprendendo C# e Clean Architecture.

---

## 1. Clean Architecture + DDD: projetos separados por camada

### O problema que isso resolve

Imagine um projeto onde a logica de negocio fica misturada com o acesso ao banco. Se um dia voce
precisar trocar PostgreSQL por SQL Server, ou Ollama por outro provedor de embeddings, voce teria
que revisar o codigo de regras de negocio — que nao deveria saber que banco existe.

A regra de dependencia do projeto e:

```
Domain  <--  Application  <--  Infrastructure  <--  Api
```

As setas indicam quem pode depender de quem. `Domain` nao conhece ninguem. `Infrastructure`
conhece `Domain` e `Application`, mas o inverso nunca e verdade.

### O que cada camada faz no projeto

| Camada | Responsabilidade | Exemplo concreto |
|---|---|---|
| `Buscador.Domain` | Regras de negocio puras, entidades, value objects | `Animal`, `AnimalId`, `IAnimalRepository` (so o contrato) |
| `Buscador.Application` | Orquestra casos de uso via CQRS (MediatR) | `SearchAnimalsQuery`, `SearchAnimalsHandler` |
| `Buscador.Infrastructure` | Implementa os contratos: banco, embeddings, busca | `ServicoBuscaSemantica`, `RepositorioAnimal`, `ServicoEmbeddingOllama` |
| `Buscador.Api` | Camada HTTP: recebe requisicao, chama MediatR, retorna resposta | `EndpointsAnimais.cs`, `Program.cs` |

### Por que isso ajuda no aprendizado

Com camadas separadas, fica visivel o que pertence a cada responsabilidade. Quando voce esta
em `Buscador.Domain` e tenta adicionar `using Microsoft.EntityFrameworkCore`, o projeto nem
compila — a separacao em projetos .NET distintos torna a violacao da regra de dependencia
um erro de build, nao apenas uma convencao esquecida.

Outro beneficio: os testes ficam cirurgicos. Os testes de `Buscador.Domain.Tests` nao precisam
de banco de dados nem de containers. Os testes de integracao em `Buscador.Api.Tests` usam
Testcontainers para subir um PostgreSQL real em memória.

---

## 2. SQL cru para pgvector: por que nao usar LINQ?

### A limitacao do LINQ com pgvector

O EF Core gera SQL a partir de expressoes LINQ. Para operacoes comuns como `.Where(a => a.Nome == "leao")`
isso funciona muito bem. Mas a busca vetorial exige operadores proprios do pgvector:

```sql
-- distancia coseno entre dois vetores
embedding <=> '[0.1, 0.2, ...]'::vector

-- busca fulltext
search_vector @@ to_tsquery('portuguese', 'leao')
```

O provedor Npgsql do EF Core nao traduz esses operadores a partir de LINQ no momento em que o
projeto foi desenvolvido. Tentar usar `.OrderBy(f => f.Embedding.CosineDistance(query))` nao
gera o SQL correto ou gera uma excecao em tempo de execucao.

### O padrao adotado: dois passos

Os servicos de busca usam consistentemente o mesmo padrao de dois passos:

**Passo 1** — SQL cru para busca e score (onde LINQ nao chega):

```csharp
// ServicoBuscaSemantica.cs
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

**Passo 2** — LINQ para carregar as entidades com enums e value objects corretamente:

```csharp
var animais = await _contexto.Animais
    .Where(a => animalIds.Contains(a.Id))
    .AsNoTracking()
    .ToListAsync(cancellationToken);
```

Essa divisao e intencional: o SQL cru e usado so onde nao existe alternativa (operadores
especiais de pgvector), e o EF Core cuida do que faz melhor — mapeamento de tipos complexos
como enums (`Dieta`, `Habitat`, `StatusConservacao`) e value objects (`AnimalId`).

O mesmo padrao aparece em `ServicoBuscaTextual.cs` com `ts_rank` e `@@`, e em
`ServicoPersistenciaFragmentos.cs` com `ExecuteSqlRawAsync` para inserir o vetor de 1024
dimensoes via cast `::vector`.

---

## 3. Multi-vetor por fragmentos: por que nao um unico embedding por animal?

### O problema da diluicao semantica

Imagine que um animal tem esta descricao (simplificada):

> "O lobo-gua e um mamifero semiaquatico. Tem pelagem densa. Habita rios da Amazonia.
> Sua dieta e baseada em peixes. E uma especie vulneravel. Os filhotes nascem cegos."

Se voce gerar um unico embedding para esse texto inteiro, o vetor resultante e uma media de
todos esses conceitos: mamifero, aquatico, pelagem, Amazonia, peixe, vulneravel, filhotes.

Agora o usuario busca por "especie em risco de extincao". A parte relevante e apenas
"E uma especie vulneravel" — mas ela esta diluida dentro do vetor medio. O resultado pode
ter pontuacao baixa mesmo sendo a resposta certa.

### A solucao: fragmentos + max-sim

A tabela `fragmentos_animal` guarda cada trecho do animal separadamente:

```sql
CREATE TABLE fragmentos_animal (
    id uuid NOT NULL,
    animal_id uuid NOT NULL,
    texto text NOT NULL,
    embedding vector(1024) NULL,
    ...
);
```

Cada animal tem entre 10 e 20 fragmentos (um por campo: nome, descricao, caracteristicas,
curiosidades, tags, etc.).

Na busca semantica, o SQL usa `MIN(f.embedding <=> vetor_consulta)` agrupado por `animal_id`.
Isso e o algoritmo **max-sim** (maximum similarity): para cada animal, pega o fragmento mais
proximo da consulta e usa esse score como representante do animal.

O resultado: "especie em risco de extincao" encontra o fragmento que fala sobre status de
conservacao com alta precisao, mesmo que os outros 14 fragmentos daquele animal nao tenham
nada a ver com o tema.

---

## 4. bge-m3: por que esse modelo e nao um em ingles?

### O conteudo e em portugues

Todos os animais do catalogo foram descritos em portugues:

- `nome_comum`: "Lobo-gua", "Arara-azul", "Onca-pintada"
- `descricao`, `caracteristicas`, `curiosidades`: textos em portugues

Modelos de embedding treinados apenas em ingles (como `text-embedding-ada-002` da OpenAI no
modo padrao) representam textos em portugues num espaco vetorial diferente do espaco das
consultas. O resultado pratico: buscar "predador da floresta" pode nao encontrar "especie
carnivora da Mata Atlantica" porque os vetores ficam distantes mesmo com sentido similar.

### bge-m3 e multilingue

O modelo `bge-m3` foi treinado com textos de mais de 100 linguas, incluindo portugues.
Isso significa que a consulta em portugues e os textos armazenados em portugues ficam no
mesmo espaco semantico — palavras com sentido similar ficam proximas, independente de
estar em ingles ou portugues.

Outras vantagens do bge-m3 neste projeto:

- **1024 dimensoes**: alta capacidade de representacao semantica
- **Roda local via Ollama**: nao precisa de chave de API, nao tem custo por requisicao,
  funciona sem internet
- **Contexto de 8192 tokens**: suporta textos longos (util para descricoes detalhadas)

A configuracao fica no `appsettings.json` como `"Modelo": "bge-m3"` dentro da secao `Ollama`,
e `ServicoEmbeddingOllama` usa `Microsoft.Extensions.AI.Ollama` para chamar o endpoint local.

---

## 5. RRF (Reciprocal Rank Fusion): por que nao media simples dos scores?

### O problema de combinar escalas diferentes

A busca textual retorna `ts_rank` — um score sem unidade definida, que pode ser `0.0759`,
`0.5`, ou `2.3` dependendo de quantas vezes o termo aparece no documento.

A busca semantica retorna `1 - distancia_coseno` — um valor entre `0` e `1`, onde `1` e
identico e `0.5` ja e muito diferente.

Se voce calcular uma media dos dois scores, esta comparando laranjas com laranjas de tamanho
diferente. Um animal com `ts_rank = 0.3` e `similaridade = 0.95` receberia a mesma media
que um com `ts_rank = 0.95` e `similaridade = 0.3`, mas eles nao sao igualmente relevantes.

### O que RRF faz

RRF nao usa os scores em si — usa apenas a *posicao* (rank) de cada resultado em cada lista.
A formula e:

```
score_rrf(animal) = soma de 1 / (k + posicao_na_lista_i)
```

Onde `k = 60` (constante de suavizacao padrao). Um animal que aparece em 1o lugar em ambas
as listas recebe `1/(60+1) + 1/(60+1) = 0.0328`. Um que aparece so em uma lista recebe
metade disso.

A propriedade mais importante: **RRF e robusto a outliers**. Se a busca textual der um score
absurdamente alto para um animal que menciona o termo 50 vezes, isso nao "sequestra" o
resultado hibrido — o que conta e apenas que ele estava em primeiro lugar, nao o quanto.

### Como esta implementado

`ServicoBuscaHibrida` busca um pool de candidatos maior que o limite final (no minimo 20):

```csharp
var tamanhoPool = Math.Max(limite, 20);
var textual   = await _textual.BuscarAsync(consulta, tamanhoPool, cancellationToken);
var semantica = await _semantica.BuscarAsync(consulta, tamanhoPool, cancellationToken);
return FusaoRrf.Fundir([textual, semantica], limite);
```

O pool maior garante que `FusaoRrf` tenha material dos dois lados para fusionar antes de
cortar no `limite` final. A classe `FusaoRrf` recebe as duas listas, atribui posicoes, aplica
a formula e devolve os `limite` melhores resultados ja ordenados.

---

## Resumo das decisoes

| Decisao | Alternativa descartada | Motivo da escolha |
|---|---|---|
| Clean Architecture com projetos separados | Tudo num unico projeto | Separacao de responsabilidades forcada pelo compilador, testabilidade |
| SQL cru para pgvector | LINQ puro | Operadores `<=>` e `@@` nao tem traducao LINQ no Npgsql |
| Multi-vetor por fragmentos | Um embedding por animal | Evita diluicao semantica em descricoes longas |
| bge-m3 multilingue | Modelo apenas em ingles | Conteudo em portugues; bge-m3 representa PT corretamente |
| RRF para fusao hibrida | Media ponderada dos scores | Robusto a escalas diferentes; nao precisa normalizar |
