# Tutorial 02 — Camada de Domínio

Neste tutorial, vamos explorar a camada `Buscador.Domain`, que é o coração do projeto. Você vai entender por que ela não depende de nenhum pacote externo, como as classes base funcionam e por que as escolhas de design foram feitas dessa forma. Não é necessário ter o ambiente rodando para acompanhar — basta ler o código junto com as explicações.

---

## Por que o Domain não tem dependências externas?

Em uma arquitetura limpa (Clean Architecture), a camada de Domínio representa as regras de negócio puras. Ela não sabe nada sobre banco de dados, HTTP, filas de mensagens ou qualquer framework. Essa separação traz duas vantagens concretas:

1. **Testabilidade**: você pode testar toda a lógica de negócio sem precisar de banco de dados, Docker ou internet.
2. **Longevidade**: se amanhã o projeto migrar do PostgreSQL para outro banco, ou do ASP.NET para outro framework, o Domain não muda.

No Deep Sparrow, essa regra é garantida pelo próprio arquivo de projeto `Buscador.Domain.csproj`: ele não declara nenhuma referência a pacotes NuGet externos. Se alguém tentar adicionar `using Microsoft.EntityFrameworkCore` em qualquer arquivo do Domain, o compilador vai rejeitar. A arquitetura se defende sozinha.

A regra de dependência do projeto é:

```
Domain  <--  Application  <--  Infrastructure  <--  Api
```

As setas indicam quem pode referenciar quem. `Domain` não aponta para ninguém.

---

## As classes base do Domain

O projeto define três abstrações fundamentais, todas no namespace `Buscador.Domain.Comum`.

### Entidade

Uma entidade é um objeto que tem identidade própria. Dois animais com o mesmo nome ainda são entidades diferentes se tiverem IDs diferentes.

```csharp
public abstract class Entidade<TId>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entidade(TId id)
    {
        Id = id;
    }

    protected Entidade() { }

    public override bool Equals(object? obj)
    {
        if (obj is not Entidade<TId> outra)
            return false;

        if (ReferenceEquals(this, outra))
            return true;

        if (GetType() != outra.GetType())
            return false;

        return Id.Equals(outra.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entidade<TId>? a, Entidade<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entidade<TId>? a, Entidade<TId>? b) => !(a == b);
}
```

O ponto central é que `Equals` compara pelo `Id`, não pelos atributos. Isso significa que se você buscar o mesmo animal do banco duas vezes e obtiver dois objetos C# diferentes na memória, eles ainda vão ser considerados iguais — porque têm o mesmo ID. Os operadores `==` e `!=` foram sobrescritos para respeitar essa mesma lógica.

### RaizAgregada

```csharp
public abstract class RaizAgregada<TId> : Entidade<TId>
    where TId : notnull
{
    protected RaizAgregada(TId id) : base(id) { }

    protected RaizAgregada() { }
}
```

`RaizAgregada` é um marcador: ela herda `Entidade` e não acrescenta nenhum comportamento novo por enquanto. O papel dela é semântico — dizer que esta classe é a "porta de entrada" de um agregado do Domain-Driven Design. No nosso projeto, `Animal` é uma raiz de agregado. Isso significa que nenhuma outra parte do código deve salvar partes de um animal diretamente; tudo passa pela entidade `Animal`.

### ObjetoDeValor

Objetos de valor não têm identidade própria. Sua igualdade é determinada pelos seus atributos. Um bom exemplo é o ID de um animal: dois `AnimalId` que guardam o mesmo `Guid` são iguais, mesmo sendo instâncias diferentes na memória.

```csharp
public abstract class ObjetoDeValor
{
    protected abstract IEnumerable<object> ObterComponentesDeIgualdade();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return ((ObjetoDeValor)obj)
            .ObterComponentesDeIgualdade()
            .SequenceEqual(ObterComponentesDeIgualdade());
    }

    public override int GetHashCode()
    {
        return ObterComponentesDeIgualdade()
            .Aggregate(0, HashCode.Combine);
    }

    public static bool operator ==(ObjetoDeValor? a, ObjetoDeValor? b) { ... }
    public static bool operator !=(ObjetoDeValor? a, ObjetoDeValor? b) => !(a == b);
}
```

Cada classe concreta que herda `ObjetoDeValor` deve implementar `ObterComponentesDeIgualdade()`, retornando os valores que definem sua identidade. A classe base usa esses componentes para calcular `Equals` e `GetHashCode` automaticamente.

---

## AnimalId — por que encapsular um Guid?

Usar `Guid` diretamente como ID pode parecer mais simples, mas cria um problema sutil: é fácil passar o ID errado para uma função sem que o compilador reclame. Imagine dois métodos:

```csharp
// Sem encapsulamento — compilador aceita, mas é um bug em potencial:
void Processar(Guid animalId, Guid usuarioId) { ... }
Processar(usuarioId, animalId); // inverteu, compilador não vê

// Com encapsulamento — o compilador rejeita a inversão:
void Processar(AnimalId animalId) { ... }
```

O `AnimalId` resolve isso encapsulando o `Guid` em um tipo próprio:

```csharp
public sealed class AnimalId : ObjetoDeValor
{
    public Guid Valor { get; }

    private AnimalId(Guid valor)
    {
        Valor = valor;
    }

    public static AnimalId Novo() => new(Guid.NewGuid());

    public static AnimalId De(Guid valor) => new(valor);

    protected override IEnumerable<object> ObterComponentesDeIgualdade()
    {
        yield return Valor;
    }

    public override string ToString() => Valor.ToString();
}
```

O construtor é `private`, então você não pode criar um `AnimalId` sem passar pelos factory methods:

- `AnimalId.Novo()` — cria um ID novo com um `Guid` aleatório. Usado ao criar um animal.
- `AnimalId.De(guid)` — reconstrói um `AnimalId` a partir de um `Guid` já existente. Usado ao carregar do banco.

---

## Animal — o Aggregate Root

`Animal` é a entidade principal do sistema. Ela herda `RaizAgregada<AnimalId>` e concentra todas as propriedades de um animal:

```csharp
public sealed class Animal : RaizAgregada<AnimalId>
{
    public string NomeComum { get; private set; } = string.Empty;
    public string NomeCientifico { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Caracteristicas { get; private set; } = string.Empty;
    public Dieta Dieta { get; private set; }
    public Habitat Habitat { get; private set; }
    public string DistribuicaoGeografica { get; private set; } = string.Empty;
    public StatusConservacao StatusConservacao { get; private set; }
    public string[] Tags { get; private set; } = [];
    public string Curiosidades { get; private set; } = string.Empty;

    private Animal() { }

    private Animal(AnimalId id) : base(id) { }

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
        string curiosidades)
    {
        if (string.IsNullOrWhiteSpace(nomeComum))
            throw new ArgumentException("Nome comum não pode ser vazio.", nameof(nomeComum));

        if (string.IsNullOrWhiteSpace(nomeCientifico))
            throw new ArgumentException("Nome científico não pode ser vazio.", nameof(nomeCientifico));

        return new Animal(AnimalId.Novo())
        {
            NomeComum = nomeComum,
            NomeCientifico = nomeCientifico,
            Descricao = descricao,
            Caracteristicas = caracteristicas,
            Dieta = dieta,
            Habitat = habitat,
            DistribuicaoGeografica = distribuicaoGeografica,
            StatusConservacao = statusConservacao,
            Tags = tags,
            Curiosidades = curiosidades
        };
    }
}
```

Algumas decisões de design importantes aqui:

**Construtor privado**: ninguém de fora pode instanciar `new Animal(...)`. Toda criação passa pelo método `Criar()`. Isso garante que um animal nunca existirá em estado inválido — sem nome comum ou sem nome científico, por exemplo.

**Setters privados**: todas as propriedades têm `private set`. Uma vez criado, o objeto só pode ser modificado por métodos que o próprio `Animal` expõe. Se amanhã precisarmos de um método `AtualizarDescricao(string novaDescricao)`, ele ficará aqui, com a validação necessária.

**Validação no `Criar()`**: o método lança `ArgumentException` se `NomeComum` ou `NomeCientifico` forem vazios. Isso garante integridade no nível do Domain, antes de qualquer banco de dados ou validação de API.

---

## Os enums do Domain

O Domain define três enumerações que descrevem características de um animal:

```csharp
public enum Dieta
{
    Carnivoro,
    Herbivoro,
    Onivoro
}
```

```csharp
public enum Habitat
{
    Floresta,
    Oceano,
    Deserto,
    Savana,
    Montanha,
    AguaDoce,
    Polar
}
```

```csharp
public enum StatusConservacao
{
    PoucoPreocupante,
    QuaseAmeacado,
    Vulneravel,
    EmPerigo,
    CriticamenteEmPerigo,
    ExtintoNaNatureza,
    Extinto
}
```

Os valores de `StatusConservacao` seguem a escala da IUCN (União Internacional para a Conservação da Natureza). Usar enums em vez de strings garante que o código não aceite valores inválidos — o compilador rejeita qualquer valor fora da lista.

---

## IRepositorioAnimal — o contrato de persistência

O Domain define a interface do repositório, mas não sabe como ela é implementada:

```csharp
public interface IRepositorioAnimal
{
    Task<Animal?> ObterPorIdAsync(AnimalId id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Animal animal, CancellationToken cancellationToken = default);
    Task AdicionarVariosAsync(IEnumerable<Animal> animais, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Animal>> ObterPaginadoAsync(int pagina, int tamanho, CancellationToken cancellationToken = default);
}
```

O Domain diz "preciso de um repositório com esses métodos". A camada `Infrastructure` é quem implementa essa interface usando o Entity Framework Core e o PostgreSQL — mas o Domain nunca vê esse detalhe. Essa inversão de dependência é o que permite trocar o banco de dados sem tocar no Domain.

---

## Rodando os testes do Domain

O projeto já tem testes unitários para a camada de Domain. Execute o seguinte comando a partir do diretório `backend/`:

```bash
dotnet test --project tests/Buscador.Domain.Tests
```

Os testes cobrem comportamentos como: criação de animal com dados válidos, rejeição de animal sem nome, igualdade de entidades pelo ID e igualdade de objetos de valor pelos componentes. Como o Domain não tem dependências externas, esses testes rodam instantaneamente sem precisar de banco de dados ou Docker.

---

## Resumo

Você viu como o Domain do Deep Sparrow é estruturado em três níveis:

- **Abstrações base** (`Entidade`, `RaizAgregada`, `ObjetoDeValor`) definem os padrões de igualdade
- **Tipos concretos** (`AnimalId`, `Animal`) aplicam esses padrões ao problema real
- **Contratos** (`IRepositorioAnimal`) declaram o que o Domain precisa sem ditar como implementar

Essa estrutura garante que a lógica de negócio permaneça isolada, testável e independente de qualquer decisão de infraestrutura.
