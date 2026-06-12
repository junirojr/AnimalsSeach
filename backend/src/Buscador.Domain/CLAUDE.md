# Buscador.Domain

## Regra fundamental
Este projeto NÃO tem dependências externas. Nunca adicione EF Core, Npgsql, MediatR
ou qualquer pacote NuGet aqui. O Domain existe para expressar regras de negócio puras.

## O que vive aqui
- **Entidades / Aggregate Roots**: `Animal` (aggregate root)
- **Value Objects**: `AnimalId`
- **Enums**: `Diet`, `Habitat`, `ConservationStatus`
- **Interfaces de repositório**: `IAnimalRepository` (contrato — a impl fica na Infrastructure)
- **Classes base**: `Entity<TId>`, `AggregateRoot<TId>`, `ValueObject`

## Convenções
- Construtores privados + método estático `Create(...)` para garantir invariantes
- Propriedades com `private set` — nunca mutação direta de fora
- Lançar `ArgumentException` quando dados inválidos no `Create`
- `AnimalId` encapsula `Guid` — nunca usar `Guid` solto para identificar animais

## O que NÃO pertence aqui
- Campos `SearchVector` e `Embedding` — ficam como shadow properties na Infrastructure
- Lógica de persistência, migrations, conexão com banco
- Chamadas HTTP, I/O, logging de framework