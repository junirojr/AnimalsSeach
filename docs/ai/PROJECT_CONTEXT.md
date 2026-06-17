# PROJECT_CONTEXT — Buscador de Animais (Deep Sparrow)

## Objetivo do projeto
Construir um motor de busca híbrido (full-text + semântico) sobre um catálogo de animais,
como projeto de aprendizado progressivo em C# / .NET 10 com Clean Architecture e DDD.

## Público-alvo (desenvolvedor)
- Experiente em Next.js / TypeScript
- Iniciante em C# e ecossistema .NET
- Aprende melhor com projetos reais e progressivos

## Stack completa
| Camada | Tecnologia | Por quê |
|--------|-----------|---------|
| Linguagem | C# / .NET 10 | LTS, alta adoção enterprise |
| API | ASP.NET Core Minimal API | Leve, moderno, ideal para aprender |
| ORM | EF Core 10 | Padrão do ecossistema .NET |
| Banco | PostgreSQL 16 + pgvector | FTS nativo + vetores sem infra extra |
| Embeddings | Ollama + bge-m3 | Local, gratuito, 1024 dimensões, multilíngue |
| CQRS | MediatR 12 | Desacoplamento, testabilidade |
| Frontend | Next.js 15 App Router | Stack conhecida pelo dev |
| Infra | Docker Desktop | Sem instalação de serviços no SO |

## Metas de aprendizado (por fase)
- **F0**: tooling, estrutura de solution, Docker, docs
- **F1**: DDD — entidades, value objects, aggregate roots, interfaces
- **F2**: EF Core, migrations, pgvector, Testcontainers
- **F3**: CQRS com MediatR, FluentValidation, seed de dados
- **F4**: Full-Text Search — tsvector, tsquery, ts_rank, trigger
- **F5**: Busca semântica — embeddings, cosine distance, Ollama
- **F6**: Busca híbrida — Reciprocal Rank Fusion (RRF)
- **F7**: Minimal API, Scalar, error handling global
- **F8**: Integração frontend (já conhecido) com backend .NET
- **F9**: Verificação E2E, documentação final

## Restrições
- Tudo local e gratuito (sem OpenAI, sem Elastic, sem cloud pago)
- Senhas de dev (`buscador/buscador`) são intencionais e locais
- Seed começa com 10 animais (MVP); expande para 50 em T9.0
- `SearchVector` é shadow property; `Embedding` e os fragmentos (`fragmentos_animal`) são acessados via SQL cru (não mapeados no EF). Domain limpo nos dois casos.