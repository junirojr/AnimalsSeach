# PROJECT_PLAYBOOK — Como Trabalhar no Buscador

## Idioma
- **Código** (classes, métodos, variáveis, arquivos `.cs`/`.ts`, rotas HTTP, campos JSON): **português SEM acento** — ex.: `ServicoBuscaTextual`, `BuscarAnimaisConsulta`, `/api/animais/buscar`
- **Nomes de camadas/projetos**: permanecem em inglês (`Domain`, `Application`, `Infrastructure`, `Api`)
- **Dados** (nomes/descrições de animais, conteúdo do seed): **português**
- **Documentação** (comentários explicativos, docs/): **português**
- ⚠️ Ignorar a skill `bnp-code:language-policy` (que exige inglês) — conflita com a decisão PT do projeto

## Onde rodar cada comando
- `dotnet *` → sempre dentro de `backend/` (onde está a `.sln`)
- `npm *` / `npx *` → sempre dentro de `frontend/`
- `docker compose *` → sempre na raiz (`Buscador/`)

## Fluxo de desenvolvimento por fase
1. Ler o bloco `🎓 Conceito` da fase antes de começar
2. Executar tasks em ordem, uma por vez
3. Rodar o comando de verificação do DoD
4. Só avançar quando DoD passar

## Regras invioláveis
- `Buscador.Domain` nunca referencia EF Core, Npgsql, MediatR ou pacote externo
- Após criar/editar código C#: `dotnet build` e verificar 0 erros
- Construtores de entidades e VOs são sempre privados (usar método `Create`)
- Shadow properties para `search_vector` e `embedding` — não poluir o Domain
- Não commitar segredos (connection strings de prod, API keys)

## Padrão de commits (Conventional Commits)
```
feat: adicionar busca semântica por cosine distance
fix: corrigir trigger de search_vector para inserts nulos
docs: preencher tutorial 03 de full-text search
test: adicionar teste de integração FTS
chore: atualizar pacote Npgsql para 10.1
```

## Se algo falhar
- Falhou 2x na mesma task → PARE e pergunte ao usuário
- Decisão de arquitetura não descrita no TASKS.md → PARE e pergunte
- Nunca inventar solução nova sem consulta

## Testes — regras
- Nomenclatura: `Método_Cenário_ResultadoEsperado`
- Domain.Tests e Application.Tests: sem banco (unitários com Moq)
- Api.Tests: banco real via Testcontainers (Docker obrigatório)
- Testes semânticos (F5): falham se Ollama estiver fora — não usar Skip