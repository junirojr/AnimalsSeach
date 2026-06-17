# Como Adicionar um Novo Animal ao Catalogo

## Pre-requisito

O Docker deve estar rodando com a infra do projeto:

```bash
docker compose up -d
```

---

## 1. Abrir o arquivo de semente

O arquivo que define todos os animais do catalogo e:

```
backend/src/Buscador.Application/Funcionalidades/PopularAnimais/DadosSementeAnimal.cs
```

---

## 2. Adicionar a entrada do novo animal

Dentro da lista `Animais`, copie o bloco abaixo e preencha com os dados do novo animal:

```csharp
Animal.Criar(
    nomeComum: "Nome popular do animal",
    nomeCientifico: "Genus species",
    descricao: "Descricao geral do animal em um ou dois paragrafos.",
    caracteristicas: "Descricao fisica: tamanho, pelagem, coloracao, etc.",
    dieta: Dieta.Carnivoro,
    habitat: Habitat.Floresta,
    distribuicaoGeografica: "Regioes geograficas onde o animal e encontrado.",
    statusConservacao: StatusConservacao.PoucoPreocupante,
    tags: new[] { "tag1", "tag2", "tag3" },
    curiosidades: "Fatos curiosos e comportamentos notaveis do animal."
),
```

Todos os parametros sao obrigatorios. Os campos de texto livres (`nomeComum`, `nomeCientifico`, `descricao`, `caracteristicas`, `distribuicaoGeografica`, `curiosidades`) aceitam qualquer string. Os tres enums e o array de tags seguem os valores listados abaixo.

---

## 3. Valores validos dos enums

### Dieta

| Valor | Significado |
|-------|-------------|
| `Dieta.Carnivoro` | Come apenas animais |
| `Dieta.Herbivoro` | Come apenas plantas |
| `Dieta.Onivoro` | Come animais e plantas |

### Habitat

| Valor | Exemplo de uso |
|-------|---------------|
| `Habitat.Floresta` | Florestas tropicais, boreais, temperadas |
| `Habitat.Oceano` | Mares e oceanos abertos |
| `Habitat.Deserto` | Regioes aridas e semiaridas |
| `Habitat.Savana` | Savanas e pradarias abertas |
| `Habitat.Montanha` | Altas altitudes, cadeias montanhosas |
| `Habitat.AguaDoce` | Rios, lagos, pantanos |
| `Habitat.Polar` | Artico e Antartica |

### StatusConservacao

| Valor | Equivalente na IUCN |
|-------|---------------------|
| `StatusConservacao.PoucoPreocupante` | Least Concern (LC) |
| `StatusConservacao.QuaseAmeacado` | Near Threatened (NT) |
| `StatusConservacao.Vulneravel` | Vulnerable (VU) |
| `StatusConservacao.EmPerigo` | Endangered (EN) |
| `StatusConservacao.CriticamenteEmPerigo` | Critically Endangered (CR) |
| `StatusConservacao.ExtintoNaNatureza` | Extinct in the Wild (EW) |
| `StatusConservacao.Extinto` | Extinct (EX) |

---

## 4. Limpar o banco e reinserir os dados

Apos salvar o arquivo, limpe os animais existentes e reinsira tudo para incluir o novo registro:

```bash
# Truncar os dados (apaga animais e fragmentos de embeddings)
docker exec postgres psql -U buscador -d buscador -c "TRUNCATE animais CASCADE;"

# Subir a API (em outro terminal, dentro de backend/)
dotnet run --project src/Buscador.Api

# Chamar o endpoint de semente
curl -X POST http://localhost:5024/api/animais/popular
```

---

## 5. Regenerar os embeddings

Apos popular o banco, gere os embeddings semanticos para o novo animal:

```bash
curl -X POST http://localhost:5024/api/animais/embeddings/gerar
```

Este passo utiliza o modelo `bge-m3` via Ollama e pode levar alguns minutos dependendo do numero de animais e do hardware disponivel.

---

## 6. Confirmar que os testes passam

```bash
cd backend
dotnet test
```

Todos os testes devem passar sem erros. Se houver falha, verifique se os campos obrigatorios do novo animal estao preenchidos corretamente.
