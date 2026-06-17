# Como funciona a busca full-text (FTS)

## O problema: por que não usar LIKE?

A primeira intuição de quem começa a trabalhar com buscas em banco de dados é usar `LIKE '%leão%'`. Funciona para o caso exato, mas tem dois problemas sérios. O primeiro é de desempenho: o `LIKE` com `%` no início força o banco a ler **todas** as linhas da tabela, uma a uma, comparando o texto. Isso é chamado de varredura sequencial (*sequential scan*) e fica cada vez mais lento conforme o número de registros cresce.

O segundo problema é semântico: `LIKE '%leão%'` não encontra "leões", "leonino" ou "felino". Cada variação da palavra exigiria uma condição separada. Além disso, o banco não entende que "caçar" e "caça" são formas do mesmo verbo — para o `LIKE`, são strings completamente diferentes.

A Busca Full-Text (FTS) do PostgreSQL resolve os dois problemas de uma vez. Ela normaliza as palavras para sua **forma base** (raiz linguística), o que permite que "leões", "leão" e "leonino" sejam comparados pela mesma raiz. E com o índice GIN — explicado mais abaixo — essa busca acontece em tempo logarítmico, não linear.

---

## tsvector — o índice invertido no banco

O `tsvector` é o resultado do processamento de um texto para fins de busca. Em vez de armazenar a frase original, ele armazena uma lista de **lexemas**: as formas normalizadas (radicais) de cada palavra, junto com a posição em que apareceu no texto.

Exemplo prático:

```sql
SELECT to_tsvector('portuguese', 'O leão caça gnus na savana');
-- Resultado: 'caç':3 'gnu':4 'leã':2 'savan':5
```

Perceba que:

- A palavra "O" foi removida. Palavras muito comuns que não agregam significado à busca (artigos, preposições) são chamadas de **stop words** e são descartadas automaticamente pelo dicionário do idioma.
- "leão" virou `'leã'` — o lexema radical para "leão" no dicionário português.
- "caça" virou `'caç'` — raiz compartilhada com "caçar", "caçador", "caçando".
- O número depois de cada lexema (`:3`, `:4`...) é a **posição** da palavra no texto original, usado para cálculos de relevância.

No projeto Buscador, a coluna `search_vector` de cada animal armazena exatamente esse `tsvector`, combinando os campos textuais e aplicando `unaccent` (para a busca ignorar acentos):

```sql
to_tsvector('portuguese', unaccent(
  nome_comum || ' ' || nome_cientifico || ' ' || descricao || ' ' ||
  caracteristicas || ' ' || curiosidades || ' ' ||
  distribuicao_geografica || ' ' || array_to_string(tags, ' ')
))
```

> **Recall:** indexamos **todos** os campos relevantes — inclusive `distribuicao_geografica` (onde está,
> p. ex., "oceanos") e as `tags`. Indexar de menos faz o FTS "não achar" termos que existem nos dados;
> foi o que segurava o modo Híbrido em consultas como "predador dos oceanos" (ver `how-embeddings-work.md`).

---

## tsquery — a consulta normalizada

Da mesma forma que o `tsvector` normaliza os documentos, o `tsquery` normaliza a **consulta** do usuário usando o mesmo dicionário.

```sql
SELECT to_tsquery('portuguese', 'caçador');
-- Resultado: 'caçad':*  (ou a raiz equivalente no dicionário português)
```

A consulta e o documento passam pelo mesmo processo de normalização, então uma busca por "caçador" pode encontrar documentos que contêm "caça", "caçando" ou "caçadora" — todos compartilham a mesma raiz.

O `tsquery` também suporta operadores lógicos:

| Operador | Significado | Exemplo |
|---|---|---|
| `&` | AND — ambos os termos devem estar presentes | `'leão' & 'caça'` |
| `\|` | OR — pelo menos um dos termos deve estar presente | `'leão' \| 'tigre'` |
| `!` | NOT — o termo não deve estar presente | `'felino' & !'doméstico'` |

---

## O operador @@ — correspondência

Com um `tsvector` e um `tsquery` em mãos, a comparação é feita com o operador `@@`:

```sql
SELECT nome_comum
FROM animais
WHERE search_vector @@ to_tsquery('portuguese', 'caçador');
```

O operador `@@` retorna `true` se o `tsvector` do documento contém algum lexema que case com o `tsquery`. É esse operador que faz o filtro: apenas os animais cujos textos compartilham a raiz da palavra buscada passam para o resultado.

---

## ts_rank — pontuação de relevância

Filtrar documentos que *contêm* a palavra é o primeiro passo. O segundo é **ordenar** os resultados por relevância. Para isso existe o `ts_rank`.

```sql
SELECT nome_comum, ts_rank(search_vector, query) AS relevancia
FROM animais, to_tsquery('portuguese', 'predador') query
WHERE search_vector @@ query
ORDER BY relevancia DESC;
```

O `ts_rank` retorna um número `float` entre 0 e 1. Quanto mais vezes o termo aparece no documento, e quanto mais próximo do início ele estiver, maior a pontuação. Isso garante que um animal cuja descrição menciona "predador" seis vezes apareça antes de um que menciona a palavra apenas uma vez.

> **Como o `ServicoBuscaTextual` monta a consulta:** ele sanitiza a entrada, junta os termos com **OR**
> (`predador | oceanos`) e envolve com `unaccent`. O OR maximiza a *recall* (acha quem casa **qualquer**
> termo) sem perder precisão, porque o `ts_rank` já ordena na frente quem casa **mais** termos — e, no modo
> Híbrido, o RRF reordena por cima. Por isso "predador dos oceanos" passou a achar o Tubarão (casa "oceanos"),
> em vez de voltar vazio como acontecia com a semântica de AND.

---

## O trigger — manutenção automática

Manter o `search_vector` atualizado manualmente seria trabalhoso: seria necessário chamar `to_tsvector` no código C# sempre que um animal fosse inserido ou atualizado. Para evitar esse acoplamento, usamos um **trigger** no banco de dados.

O trigger `tg_atualizar_vetor_busca` é declarado como `BEFORE INSERT OR UPDATE` na tabela `animais`. Antes de qualquer inserção ou atualização ser persistida, o PostgreSQL chama automaticamente a função `fn_atualizar_vetor_busca`, que calcula o novo `tsvector` e preenche a coluna `search_vector`.

Benefícios desta abordagem:

- O código C# não precisa saber que `search_vector` existe. Ele apenas salva o animal normalmente pelo EF Core.
- Não há risco de inconsistência: o vetor de busca é sempre recalculado junto com os dados, na mesma transação.
- Migrações futuras que alterem os campos indexados só precisam ajustar a função do banco, sem mudar o código da aplicação.

---

## O índice GIN — performance

Mesmo com o `@@` funcionando corretamente, sem um índice o PostgreSQL precisaria abrir cada linha da tabela, ler o `tsvector` e verificar se a correspondência existe. Com milhares ou milhões de animais, isso seria impraticável.

O índice **GIN** (*Generalized Inverted Index*) resolve isso. Ele cria uma estrutura de dados que mapeia cada lexema para a lista de linhas que o contêm — o conceito é o mesmo de um índice remissivo no final de um livro, onde cada palavra aponta para as páginas em que aparece.

```sql
CREATE INDEX ix_animais_search_vector ON animais USING GIN(search_vector);
```

Com o GIN, a busca passa de **O(n)** — varredura de todas as linhas — para **O(log n)** — navegação pela árvore do índice. Na prática, a diferença é de segundos para milissegundos em tabelas grandes.

---

## Limitações desta abordagem

A FTS com `tsvector`/`tsquery` é poderosa, mas tem limites que vale conhecer:

1. **Não entende significado.** Uma busca por "predador veloz" não encontra um animal descrito como "caçador ágil", mesmo que o significado seja equivalente. A FTS trabalha com forma linguística (radicais), não com semântica.

2. **Dependente do dicionário de idioma.** O dicionário `'portuguese'` define quais palavras são stop words e como cada raiz é calculada. Um texto misturado com termos em inglês ou latim (nomes científicos, por exemplo) pode não ser indexado ou buscado corretamente sem configuração adicional.

3. **Sem tolerância a erros ortográficos.** O `unaccent` resolve o caso de acento ("leao" acha "leão"), mas se o usuário digitar "leãoo" ou "caçadoor", a busca não retorna resultados. O FTS nativo do PostgreSQL não implementa busca aproximada nem correção ortográfica.

4. **Stemming preso ao dicionário.** O dicionário `portuguese` colapsa flexões pela raiz, mas nem sempre como esperamos — ex.: "voam" e "voar" caem em radicais diferentes, então buscar "voam" pode não achar quem só tem "voar".

A **busca semântica** (embeddings, ver `how-embeddings-work.md`) complementa a FTS no que falta: em vez de comparar radicais, converte o texto em vetores via `bge-m3` no Ollama e usa o `pgvector` para achar animais semanticamente próximos — mesmo sem coincidência de palavras. E o **modo Híbrido** funde os dois (FTS + semântica) por *Reciprocal Rank Fusion*, ficando com o melhor de cada: a FTS ancora a palavra literal e a semântica entra para o conceito.
