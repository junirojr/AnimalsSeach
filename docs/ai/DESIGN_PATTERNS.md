# DESIGN_PATTERNS — Padrões Adotados no Buscador

## 1. Clean Architecture (Regra de Dependência)
```
Domain ← Application ← Infrastructure ← Api
```
**Quando usar**: toda vez que criar um arquivo, perguntar: "de qual camada ele depende?"
Se o Domain precisar referenciar EF Core → está errado. Mova para Infrastructure.

## 2. Domain-Driven Design (DDD)

### Entity
- Tem identidade (`Id`); dois objetos com mesmo `Id` são "o mesmo"
- Igualdade por `Id`, não por valor dos campos
- Exemplo: `Animal` — dois animais com mesmo `AnimalId` são o mesmo animal

### Value Object
- Sem identidade; igualdade pelo conteúdo
- Imutável após criação
- Exemplo: `AnimalId` — encapsula `Guid`, dois `AnimalId` com mesmo `Guid` são iguais

### Aggregate Root
- Entidade "porta de entrada" que controla suas invariantes
- Acesso sempre pela raiz, nunca direto a entidades filhas
- Exemplo: `Animal` é o aggregate root

### Método de fábrica estático (`Create`)
- Substitui construtor público
- Garante que o objeto nunca nasce inválido
- Lança `ArgumentException` para dados inválidos

## 3. CQRS com MediatR
- **Query**: retorna dados, sem efeito colateral (`GetAnimalByIdQuery`)
- **Command**: muda estado, pode retornar resultado (`SeedAnimalsCommand`)
- **Handler**: implementa `IRequestHandler<TRequest, TResponse>`
- **Por quê**: desacopla quem pede de quem executa; cada caso de uso é isolado e testável

## 4. Repository Pattern
- Interface definida no Domain (`IAnimalRepository`)
- Implementação na Infrastructure (`AnimalRepository`)
- A Application nunca sabe se é Postgres, MongoDB ou memória
- **Por quê**: testabilidade (Moq na Application, Testcontainers na Api)

## 5. Shadow Properties (EF Core)
- `search_vector` e `embedding` existem no banco e no mapeamento EF
- Mas NÃO existem na classe `Animal` do Domain
- Acessados via `entry.Property("search_vector").CurrentValue`
- **Por quê**: manter o Domain limpo, sem deps de Npgsql/pgvector

## 6. Reciprocal Rank Fusion (RRF)
```
score = Σ 1 / (k + rank_i)    onde k = 60 (padrão)
```
- Combina ranking FTS + ranking semântico usando posição, não score bruto
- `k=60` suaviza o peso excessivo das primeiras posições
- **Quando usar**: modo `hybrid` na `SearchAnimalsQuery`
- **Por quê**: FTS e semântico têm escalas diferentes; RRF normaliza pelo rank

## 7. Minimal API com extensões de endpoint
```csharp
app.MapAnimalEndpoints();  // extensão em AnimalEndpoints.cs
```
- Cada recurso tem seu próprio arquivo de extensão
- `Program.cs` fica limpo (só wiring)
- Endpoints fazem: ler → mediator.Send → retornar resultado HTTP