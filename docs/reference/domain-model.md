# Referência — Modelo de Domínio

> Referência técnica da camada Domain (`Buscador.Domain`).
> Para o glossário de linguagem ubíqua, veja [docs/ai/PROJECT_DOMAIN_MAP.md](../ai/PROJECT_DOMAIN_MAP.md).

---

## Visão Geral

O Domain é o núcleo da Clean Architecture — sem dependências externas, sem EF Core,
sem pacotes NuGet. Contém apenas regras de negócio expressas em C# puro.

```
Buscador.Domain/
├── Comum/
│   ├── Entidade.cs          # Base: igualdade por Id
│   ├── RaizAgregada.cs      # Herda Entidade; marca o aggregate root
│   └── ObjetoDeValor.cs     # Base: igualdade estrutural por valor
└── Animais/
    ├── Animal.cs             # Aggregate root principal
    ├── AnimalId.cs           # Value Object: identificador tipado
    ├── Dieta.cs              # Enum: regime alimentar
    ├── Habitat.cs            # Enum: ambiente natural
    ├── StatusConservacao.cs  # Enum: grau de ameaça IUCN
    └── IRepositorioAnimal.cs # Contrato de persistência
```

---

## Classes Base (`Comum/`)

### `Entidade<TId>`
Classe abstrata base para entidades DDD. Igualdade determinada pelo `Id`.

```csharp
public abstract class Entidade<TId> where TId : notnull
{
    public TId Id { get; protected set; }

    public override bool Equals(object? obj) { ... } // compara por Id
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Entidade<TId>? a, Entidade<TId>? b) { ... }
    public static bool operator !=(Entidade<TId>? a, Entidade<TId>? b) { ... }
}
```

### `RaizAgregada<TId>`
Herda `Entidade<TId>`. Marca a classe como ponto de entrada do aggregate.
Não adiciona comportamento além da herança — serve como marcador semântico.

```csharp
public abstract class RaizAgregada<TId> : Entidade<TId> where TId : notnull { }
```

### `ObjetoDeValor`
Classe abstrata base para Value Objects. Igualdade por valor via `ObterComponentesDeIgualdade()`.

```csharp
public abstract class ObjetoDeValor
{
    protected abstract IEnumerable<object> ObterComponentesDeIgualdade();

    public override bool Equals(object? obj) { ... } // compara componentes
    public override int GetHashCode() { ... }         // hash dos componentes
    public static bool operator ==(ObjetoDeValor? a, ObjetoDeValor? b) { ... }
    public static bool operator !=(ObjetoDeValor? a, ObjetoDeValor? b) { ... }
}
```

---

## Aggregate Root: `Animal`

**Arquivo:** `Animais/Animal.cs`
**Herança:** `RaizAgregada<AnimalId>`
**Modificador:** `sealed` (não pode ser herdado)

### Propriedades

| Propriedade | Tipo | Acesso | Obrigatório |
|-------------|------|--------|-------------|
| `Id` | `AnimalId` | `get` / `protected set` (herdado) | Sim |
| `NomeComum` | `string` | `get` / `private set` | Sim |
| `NomeCientifico` | `string` | `get` / `private set` | Sim |
| `Descricao` | `string` | `get` / `private set` | Não (pode vazio) |
| `Caracteristicas` | `string` | `get` / `private set` | Não |
| `Dieta` | `Dieta` | `get` / `private set` | Sim |
| `Habitat` | `Habitat` | `get` / `private set` | Sim |
| `DistribuicaoGeografica` | `string` | `get` / `private set` | Não |
| `StatusConservacao` | `StatusConservacao` | `get` / `private set` | Sim |
| `Tags` | `string[]` | `get` / `private set` | Não (padrão `[]`) |
| `Curiosidades` | `string` | `get` / `private set` | Não |

### Método de Fábrica: `Criar(...)`

```csharp
public static Animal Criar(
    string nomeComum,
    string nomeCientifico,
    string descricao,
    string caracteristicas,
    Dieta dieta,
    Habitat habitat,
    string distribuicaoGeografica,
    StatusConservacao statusConservacao,
    string[] tags,
    string curiosidades) { ... }
```

**Validações (lança `ArgumentException`):**
- `nomeComum` vazio ou só espaços → `paramName: "nomeComum"`
- `nomeCientifico` vazio ou só espaços → `paramName: "nomeCientifico"`

**Comportamento:**
- Gera um novo `AnimalId` via `AnimalId.Novo()` automaticamente.
- Construtor privado — criação sempre passa pelo método de fábrica.

---

## Value Object: `AnimalId`

**Arquivo:** `Animais/AnimalId.cs`
**Herança:** `ObjetoDeValor`
**Modificador:** `sealed`

### Propriedades

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Valor` | `Guid` | O Guid subjacente |

### Métodos

| Método | Retorno | Descrição |
|--------|---------|-----------|
| `AnimalId.Novo()` | `AnimalId` | Cria com novo `Guid.NewGuid()` |
| `AnimalId.De(Guid)` | `AnimalId` | Reconstrói a partir de Guid existente |
| `ToString()` | `string` | Retorna `Valor.ToString()` |

**Igualdade:** dois `AnimalId` com o mesmo `Guid Valor` são iguais (`==` e `Equals`).

---

## Enums

### `Dieta` — `Animais/Dieta.cs`

```csharp
public enum Dieta { Carnivoro, Herbivoro, Onivoro }
```

### `Habitat` — `Animais/Habitat.cs`

```csharp
public enum Habitat { Floresta, Oceano, Deserto, Savana, Montanha, AguaDoce, Polar }
```

### `StatusConservacao` — `Animais/StatusConservacao.cs`

```csharp
public enum StatusConservacao
{
    PoucoPreocupante,      // LC - Least Concern
    QuaseAmeacado,         // NT - Near Threatened
    Vulneravel,            // VU - Vulnerable
    EmPerigo,              // EN - Endangered
    CriticamenteEmPerigo,  // CR - Critically Endangered
    ExtintoNaNatureza,     // EW - Extinct in the Wild
    Extinto                // EX - Extinct
}
```

---

## Interface: `IRepositorioAnimal`

**Arquivo:** `Animais/IRepositorioAnimal.cs`
**Camada:** Domain (contrato). Implementação em `Buscador.Infrastructure`.

```csharp
public interface IRepositorioAnimal
{
    Task<Animal?> ObterPorIdAsync(AnimalId id, CancellationToken cancellationToken);
    Task AdicionarAsync(Animal animal, CancellationToken cancellationToken);
    Task AdicionarVariosAsync(IEnumerable<Animal> animais, CancellationToken cancellationToken);
    Task<IReadOnlyList<Animal>> ObterPaginadoAsync(int pagina, int tamanho, CancellationToken cancellationToken);
}
```

> Métodos de busca (`BuscarTextualAsync`, `BuscarSemanticaAsync`) serão adicionados nas Fases 4–6
> quando os serviços correspondentes forem implementados na Infrastructure.

---

## Regras de Dependência

```
Buscador.Domain   ←── não depende de ninguém
Buscador.Application  ←── depende só do Domain
Buscador.Infrastructure ←── depende de Application + Domain
Buscador.Api ←── depende de todos acima
```

O Domain **nunca** importa: EF Core, Npgsql, pgvector, MediatR, FluentValidation ou qualquer NuGet.
