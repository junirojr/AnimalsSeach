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
- **Consulta (Query)**: retorna dados, sem efeito colateral (`ObterAnimalPorIdConsulta`, `BuscarAnimaisConsulta`)
- **Comando (Command)**: muda estado, pode retornar resultado (`PopularAnimaisComando`, `GerarEmbeddingsComando`)
- **Manipulador (Handler)**: implementa `IRequestHandler<TRequest, TResponse>` (ex.: `ObterAnimalPorIdConsultaManipulador`)
- **Por quê**: desacopla quem pede de quem executa; cada caso de uso é isolado e testável

## 4. Repository Pattern
- Interface definida no Domain (`IRepositorioAnimal`)
- Implementação na Infrastructure (`RepositorioAnimal`)
- A Application nunca sabe se é Postgres, MongoDB ou memória
- **Por quê**: testabilidade (Moq na Application, Testcontainers na Api)

## 5. Shadow Property + acesso via SQL cru
- `search_vector` (tsvector) é **shadow property**: existe no banco e no mapeamento EF, mas NÃO na classe `Animal`.
- `embedding` (vector) e a tabela `fragmentos_animal` **NÃO são mapeados no EF** — acesso sempre por **SQL cru**
  (`ExecuteSqlRaw`/`SqlQuery`), para usar os operadores do pgvector (`<=>`) sem acoplar o ORM à extensão.
- **Por quê**: manter o Domain limpo (sem deps de Npgsql/pgvector) e evitar rework de migration em coluna criada via SQL.

## 6. Reciprocal Rank Fusion (RRF)
```
score = Σ 1 / (k + rank_i)    onde k = 60 (padrão)
```
- Combina ranking FTS + ranking semântico usando posição, não score bruto
- `k=60` suaviza o peso excessivo das primeiras posições
- **Quando usar**: modo `Hibrida` em `BuscarAnimaisConsulta`; a fusão pura fica em `FusaoRrf` (Application, testável)
- **Por quê**: FTS e semântico têm escalas diferentes; RRF normaliza pelo rank

## 7. Minimal API com extensões de endpoint
```csharp
app.MapearEndpointsAnimais();  // extensao em Endpoints/EndpointsAnimais.cs (planejado na F7)
```
- Cada recurso tem seu próprio arquivo de extensão
- `Program.cs` fica limpo (só wiring)
- Endpoints fazem: ler → mediator.Send → retornar resultado HTTP
- **Estado atual:** os endpoints ainda estão **inline no `Program.cs`** (versão mínima); a extração para
  `EndpointsAnimais.cs` é tarefa da Fase 7.

## 8. Multi-vetor por fragmentos + max-sim (busca semântica)
- Cada animal vira **vários** embeddings (chunks): nome + cada frase + cada tag **contextualizada** (`"Lobo: predador"`).
- `FragmentadorAnimal` (Application, pura/testável) faz a divisão; os vetores ficam em `fragmentos_animal`.
- Busca por **max-sim**: o animal pontua pela **menor distância** entre seus fragmentos
  (`MIN(embedding <=> q) GROUP BY animal`) — vence pelo *melhor* fragmento, não pela média.
- **Por quê**: um vetor único dilui atributos na média; o multi-vetor isola cada atributo. Contextualizar a
  tag com o nome evita chunks idênticos entre animais que compartilham a mesma tag (ex.: "predador").