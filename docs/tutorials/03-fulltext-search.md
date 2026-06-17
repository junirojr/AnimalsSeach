# Tutorial 03 — Busca Full-Text

Neste tutorial, vamos entender como o Deep Sparrow realiza buscas por palavras-chave usando os recursos nativos do PostgreSQL. Você vai aprender o que é um `tsvector`, como o gatilho mantém o índice atualizado automaticamente e como o `ServicoBuscaTextual` transforma uma consulta digitada pelo usuário em SQL eficiente.

Para acompanhar as demonstrações práticas, o ambiente precisa estar rodando (veja o Tutorial 01).

---

## O que é busca full-text no PostgreSQL?

Quando você armazena texto em uma coluna `text`, o banco de dados pode buscar nela com `LIKE '%palavra%'`, mas isso é lento: ele precisa ler cada linha inteira e comparar caractere por caractere. Para textos maiores, isso não escala.

O PostgreSQL oferece uma alternativa muito mais eficiente: o tipo `tsvector`. Em vez de guardar o texto original, o banco guarda um **documento indexado**: uma lista de lexemas (raízes de palavras) com suas posições no texto. Por exemplo, o texto "O leão é um grande predador" vira algo como:

```
'grande':5 'leao':2 'predador':7
```

Algumas palavras foram removidas (artigos, preposições — chamadas de *stopwords*) e outras foram reduzidas à raiz (`leão` vira `leao`). O dicionário `portuguese` do PostgreSQL sabe fazer essa normalização para o idioma português.

Para buscar nesse índice, usamos um `tsquery`: uma expressão de busca com os mesmos lexemas normalizados. A operação `@@` verifica se um documento (`tsvector`) satisfaz uma consulta (`tsquery`). A função `ts_rank` atribui uma pontuação numérica com base em quantas vezes e em quais posições os termos aparecem no documento.

---

## O gatilho que mantém o índice atualizado

No Deep Sparrow, a coluna `search_vector` da tabela `animais` guarda o `tsvector` de cada animal. Essa coluna precisa ser recalculada toda vez que um animal é inserido ou atualizado.

Em vez de fazer isso manualmente na aplicação, usamos um **gatilho (trigger) do PostgreSQL**. O gatilho é uma função que o banco executa automaticamente antes de cada `INSERT` ou `UPDATE`. Ele foi criado pela migration `GatilhoVetorBusca`:

```sql
CREATE OR REPLACE FUNCTION fn_atualizar_vetor_busca()
RETURNS trigger AS $$
BEGIN
  NEW.search_vector :=
    to_tsvector('portuguese',
      coalesce(NEW.nome_comum, '') || ' ' ||
      coalesce(NEW.nome_cientifico, '') || ' ' ||
      coalesce(NEW.descricao, '') || ' ' ||
      coalesce(NEW.caracteristicas, '') || ' ' ||
      coalesce(NEW.curiosidades, '')
    );
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tg_atualizar_vetor_busca
BEFORE INSERT OR UPDATE ON animais
FOR EACH ROW EXECUTE FUNCTION fn_atualizar_vetor_busca();
```

A função concatena cinco campos do animal (usando `coalesce` para evitar erros com valores nulos), passa o texto resultante para `to_tsvector('portuguese', ...)` e armazena o resultado em `NEW.search_vector` — o `NEW` é a linha que está sendo inserida ou atualizada.

O `BEFORE` na criação do trigger é importante: a função modifica `NEW` antes que a linha seja gravada, então o `search_vector` já chega correto ao disco.

---

## A shadow property: o Domain não sabe que ela existe

A coluna `search_vector` é o que o Entity Framework Core chama de *shadow property*: uma coluna que existe no banco de dados mas não tem propriedade correspondente na entidade C# `Animal`. A classe `Animal` no Domain não tem nenhum campo `SearchVector`.

Isso é intencional. A coluna é um detalhe de implementação da busca textual — ela pertence à Infrastructure, não ao Domain. O EF Core foi configurado (no `ContextoBanco`) para saber que essa coluna existe, mas a entidade permanece limpa.

O resultado prático é que, quando você chama `Animal.Criar(...)` e salva no banco via `AdicionarAsync`, o gatilho do PostgreSQL preenche `search_vector` automaticamente. A aplicação não precisa se preocupar com isso.

---

## Como o ServicoBuscaTextual funciona

O arquivo `backend/src/Buscador.Infrastructure/Busca/ServicoBuscaTextual.cs` contém toda a lógica de busca textual. Vamos acompanhar o que acontece quando um usuário digita `leao carnivoro`:

### Passo 1 — Sanitização da consulta

```csharp
var termos = Regex
    .Replace(consulta, @"[^\p{L}\p{N}\s]", " ")
    .Split(' ', StringSplitOptions.RemoveEmptyEntries);
```

A expressão regular remove qualquer caractere que não seja letra, número ou espaço. Isso evita que caracteres como `'`, `(` ou `|` quebrem a sintaxe do `to_tsquery`. Após a remoção, a string é dividida em termos individuais.

Para a entrada `leao carnivoro`, o resultado é `["leao", "carnivoro"]`.

### Passo 2 — Montagem do tsquery com OR

```csharp
var consultaPreparada = string.Join(" | ", termos);
// resultado: "leao | carnivoro"
```

Os termos são unidos com ` | `, o operador de OU do `tsquery`. A escolha do OR em vez do AND é deliberada: queremos maximizar o recall (encontrar o máximo de resultados relevantes), e a função `ts_rank` já vai colocar no topo os animais que satisfazem mais termos. O modo híbrido (se usado) reordena ainda mais os resultados.

### Passo 3 — Execução do SQL

```csharp
var scores = await _contexto.Database
    .SqlQuery<IdComPontuacao>(
        $"""
        SELECT a.id AS "Id", ts_rank(a.search_vector, q) AS "Pontuacao"
        FROM animais a,
             to_tsquery('portuguese', unaccent({consultaPreparada})) q
        WHERE a.search_vector @@ q
        ORDER BY ts_rank(a.search_vector, q) DESC
        LIMIT {limite}
        """)
    .ToListAsync(cancellationToken);
```

O SQL faz três coisas relevantes:

- `unaccent(...)` normaliza acentos antes de passar para o `to_tsquery`. Isso torna a busca insensível a acento: `leão` e `leao` encontram os mesmos resultados.
- `WHERE a.search_vector @@ q` filtra apenas os animais cujo índice satisfaz a consulta.
- `ts_rank(...)` calcula uma pontuação para cada resultado, levando em conta frequência e posição dos termos.

O resultado desse passo é uma lista de IDs com suas pontuações — não os objetos `Animal` completos.

### Passo 4 — Carregamento das entidades via EF Core

```csharp
var animalIds = scores.Select(s => AnimalId.De(s.Id)).ToList();
var animais = await _contexto.Animais
    .Where(a => animalIds.Contains(a.Id))
    .AsNoTracking()
    .ToListAsync(cancellationToken);
```

Com os IDs em mãos, o EF Core carrega as entidades `Animal` completas. Esse segundo passo existe porque o EF Core sabe converter enums do banco para C# (por exemplo, `Dieta = 0` vira `Dieta.Carnivoro`), enquanto o SQL cru do passo anterior retorna apenas tipos primitivos.

### Passo 5 — Combinação e ordenação

```csharp
return scores
    .Join(animais, s => s.Id, a => a.Id.Valor,
        (s, a) => new ResultadoBuscaDto(a.ParaDto(), s.Pontuacao))
    .ToList();
```

Os objetos `Animal` são combinados com os scores do SQL, mantendo a ordem de relevância original.

---

## Testando a busca textual

Com o ambiente rodando (API na porta 5024), você pode testar diretamente:

```bash
# Buscar animais carnívoros
curl "http://localhost:5024/api/animais/buscar?q=carnivoro&modo=Textual"

# Buscar animais de oceano, limitando a 5 resultados
curl "http://localhost:5024/api/animais/buscar?q=oceano&modo=Textual&limite=5"

# Busca com duas palavras — OR entre os termos
curl "http://localhost:5024/api/animais/buscar?q=leao floresta&modo=Textual"
```

A resposta será uma lista JSON de animais com um campo `pontuacao` que indica a relevância textual.

---

## O que a busca textual NAO faz

A busca textual é excelente para encontrar palavras exatas (ou suas variações morfológicas normalizadas pelo dicionário português). Mas ela não entende significado. Isso significa:

- Buscar `predador` **não vai encontrar** animais que usam apenas `carnivoro` no texto, mesmo sendo sinônimos.
- Buscar `especie ameacada` **não vai encontrar** automaticamente animais com `StatusConservacao` igual a `EmPerigo` — a menos que essa string apareça literalmente no texto.
- Buscar `animal grande` **não vai encontrar** "elefante" se a descrição usar apenas "mamífero de grande porte" — depende dos lexemas.

Para esse tipo de busca por significado, o projeto implementa a **busca semântica com embeddings**, que é o assunto do Tutorial 04.
