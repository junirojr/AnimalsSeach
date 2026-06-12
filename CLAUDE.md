# Deep Sparrow — Buscador de Animais

## Objetivo
Motor de busca híbrido (full-text + semântico) sobre um catálogo de animais.
Projeto de aprendizado: C# / .NET 10, Clean Architecture, DDD, pgvector, Ollama.

## Stack
- **Backend**: .NET 10, ASP.NET Core Minimal API, EF Core 10, PostgreSQL 16 + pgvector
- **Busca FTS**: `tsvector` / `tsquery` nativo do Postgres
- **Busca Semântica**: pgvector + Ollama (`nomic-embed-text`, 768 dim)
- **CQRS**: MediatR 12 | **Validação**: FluentValidation | **Docs API**: Scalar
- **Frontend**: Next.js 15 (App Router), TypeScript, Tailwind CSS, TanStack Query
- **Infra local**: Docker Desktop (PostgreSQL + Ollama em containers)

## Estrutura
```
Buscador/
├── backend/        # Solution .NET (src/ + tests/)
├── frontend/       # Next.js App
├── docs/           # Documentação Diataxis + docs/ai/
└── docker-compose.yml
```

## Comandos essenciais
```bash
# Infra
docker compose up -d
docker exec ollama ollama pull nomic-embed-text

# Backend (rodar em backend/)
dotnet build
dotnet test
dotnet ef database update --project src/Buscador.Infrastructure --startup-project src/Buscador.Api

# Frontend (rodar em frontend/)
npm run dev
npm test
npx playwright test
```

## Convenções globais
- **Código**: inglês (classes, métodos, variáveis, arquivos)
- **Dados e docs**: português (conteúdo dos animais, comentários explicativos)
- **Regra de dependência**: `Domain` ← `Application` ← `Infrastructure` ← `Api`
- `Buscador.Domain` nunca referencia EF Core, Npgsql, MediatR ou pacote externo
- Senhas de dev (`buscador/buscador`) são locais e intencionais — não commitar segredos