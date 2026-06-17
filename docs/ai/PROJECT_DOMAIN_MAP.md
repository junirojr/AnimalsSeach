# PROJECT_DOMAIN_MAP — Linguagem Ubíqua do Domínio

> Glossário autoritativo. Todos os identificadores de código, rotas HTTP e campos JSON
> seguem exatamente estes nomes. Em caso de conflito, este arquivo prevalece.

---

## Entidades e Aggregates

### Animal (Aggregate Root)
Entidade central do sistema. Representa um animal catalogado com suas informações
biológicas, ecológicas e de conservação. Criado exclusivamente via método de fábrica
`Criar(...)` para garantir que nunca nasça em estado inválido.

**Invariantes:**
- `NomeComum` não pode ser vazio ou apenas espaços.
- `NomeCientifico` não pode ser vazio ou apenas espaços.
- Identificado por `AnimalId` (nunca por `Guid` solto).

---

## Objetos de Valor (Value Objects)

### AnimalId
Encapsula o `Guid` que identifica um `Animal`. Dois `AnimalId` com o mesmo `Guid`
são iguais por valor (não por referência). Criado via `Novo()` (gera novo Guid) ou
`De(Guid)` (reconstrói a partir de um Guid existente, usado em persistência).

---

## Enums

### Dieta
Classifica o regime alimentar do animal.

| Valor | Significado |
|-------|-------------|
| `Carnivoro` | Come exclusivamente carne |
| `Herbivoro` | Come exclusivamente vegetais |
| `Onivoro` | Come carne e vegetais |

### Habitat
Classifica o ambiente natural principal do animal.

| Valor | Significado |
|-------|-------------|
| `Floresta` | Florestas tropicais, temperadas ou boreais |
| `Oceano` | Ambientes marinhos abertos |
| `Deserto` | Regiões áridas e semiáridas |
| `Savana` | Campos tropicais com vegetação esparsa |
| `Montanha` | Regiões de altitude elevada |
| `AguaDoce` | Rios, lagos e pântanos |
| `Polar` | Regiões árticas e antárticas |

### StatusConservacao
Escala IUCN de ameaça à extinção, do menos ao mais crítico.

| Valor | Significado IUCN |
|-------|------------------|
| `PoucoPreocupante` | Least Concern (LC) |
| `QuaseAmeacado` | Near Threatened (NT) |
| `Vulneravel` | Vulnerable (VU) |
| `EmPerigo` | Endangered (EN) |
| `CriticamenteEmPerigo` | Critically Endangered (CR) |
| `ExtintoNaNatureza` | Extinct in the Wild (EW) |
| `Extinto` | Extinct (EX) |

---

## Interfaces de Repositório

### IRepositorioAnimal
Contrato de persistência definido no Domain. A implementação concreta (`RepositorioAnimal`)
fica na Infrastructure e nunca vaza para o Domain.

**Métodos:**

| Assinatura | Descrição |
|-----------|-----------|
| `ObterPorIdAsync(AnimalId, CancellationToken)` | Retorna o animal pelo Id ou `null` |
| `AdicionarAsync(Animal, CancellationToken)` | Persiste um único animal |
| `AdicionarVariosAsync(IEnumerable<Animal>, CancellationToken)` | Persiste vários animais (seed) |
| `ObterPaginadoAsync(int pagina, int tamanho, CancellationToken)` | Lista paginada de animais |

---

## Propriedades de Animal

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Id` | `AnimalId` | Identificador único tipado |
| `NomeComum` | `string` | Nome popular (ex.: "Onça-pintada") |
| `NomeCientifico` | `string` | Nome científico binomial (ex.: "Panthera onca") |
| `Descricao` | `string` | Descrição geral do animal |
| `Caracteristicas` | `string` | Características morfológicas e comportamentais |
| `Dieta` | `Dieta` | Regime alimentar (enum) |
| `Habitat` | `Habitat` | Ambiente natural (enum) |
| `DistribuicaoGeografica` | `string` | Regiões onde o animal é encontrado |
| `StatusConservacao` | `StatusConservacao` | Grau de ameaça IUCN (enum) |
| `Tags` | `string[]` | Palavras-chave para busca e categorização |
| `Curiosidades` | `string` | Fatos curiosos sobre o animal |

> **Campos de busca ausentes do Domain:** `VetorBusca` (tsvector, shadow property mapeada na Infra) e
> `Embedding` (vector 1024d, bge-m3 — acesso via **SQL cru**, não mapeado no EF; idem `fragmentos_animal`).
> O Domain permanece limpo nos dois casos.

---

## Mapeamento de Nomenclatura (Inglês → Português)

| Inglês (padrão DDD) | Português (este projeto) |
|---------------------|--------------------------|
| `Animal` | `Animal` |
| `AnimalId` | `AnimalId` |
| `Diet` | `Dieta` |
| `Habitat` | `Habitat` |
| `ConservationStatus` | `StatusConservacao` |
| `CommonName` | `NomeComum` |
| `ScientificName` | `NomeCientifico` |
| `Description` | `Descricao` |
| `Characteristics` | `Caracteristicas` |
| `GeographicDistribution` | `DistribuicaoGeografica` |
| `Tags` | `Tags` |
| `Curiosities` | `Curiosidades` |
| `Create` | `Criar` |
| `New` / `From` | `Novo` / `De` |
| `GetEqualityComponents` | `ObterComponentesDeIgualdade` |
| `Repository` / `IRepository` | `Repositorio` / `IRepositorio` |
| `Entity` | `Entidade` |
| `AggregateRoot` | `RaizAgregada` |
| `ValueObject` | `ObjetoDeValor` |
