# TASKS — Plano de Execução (Buscador / Deep Sparrow)

> Plano derivado de [PLAN.md](PLAN.md), quebrado em **tasks atômicas** para execução
> por um modelo mais simples. Cada task tem: objetivo único, arquivos exatos, passos
> diretos, critério de aceite verificável e dependências.

## Modo de execução: HÍBRIDO

- O **modelo executa** todo o trabalho mecânico (criar arquivos, rodar comandos, testes).
- Cada **fase** abre com um bloco `🎓 Conceito` — explicação curta (3-6 linhas) do que aquela
  fase ensina, para o **usuário (iniciante em C#) acompanhar e aprender** observando.
- Ao concluir cada fase, o modelo deve escrever 2-3 frases resumindo o que foi feito e por quê,
  em linguagem acessível, antes de seguir.

## Como usar este documento

1. Execute as tasks **em ordem**. Não pule fases — cada uma depende da anterior.
2. Faça **uma task por vez**. Só comece a próxima quando o **Critério de Aceite** da atual passar.
3. Ao terminar uma task, marque o checkbox `[x]` e rode o comando de verificação indicado.
4. **Regras invioláveis** (ver `REGRAS GLOBAIS` abaixo) valem para todas as tasks.
5. Se um passo falhar 2x ou pedir uma decisão de arquitetura não descrita aqui:
   **PARE e peça ajuda ao usuário.** Não invente solução nova.
6. Após a **aprovação** de cada task, **commite** seguindo o `FLUXO GIT` abaixo. **Push ao fim de cada fase.**

## REGRAS GLOBAIS (valem para TODA task)

- **Idioma do código**: **português SEM acento** (classes, métodos, variáveis, propriedades, arquivos).
  Ex.: `NomeComum`, `BuscarAsync`, `RepositorioAnimal`. Mantenha as convenções de C#
  (PascalCase para tipos/métodos/propriedades, camelCase para variáveis locais). **Nunca** use
  acento ou cedilha em identificador (`Descricao`, não `Descrição`).
- **Idioma dos dados, documentação e comentários**: português (pode ter acento — é texto, não código).
- **Termos que NÃO se traduzem** (linguagem/biblioteca): `Task`, `CancellationToken`, `IRequest`,
  `DbContext`, `Guid`, palavras-chave do C#, nomes de pacotes NuGet. O sufixo `Dto` e a abreviação
  `Id` são mantidos. O termo técnico `Embedding` é mantido.
- **Nomenclatura padronizada:** use sempre os nomes do `GLOSSÁRIO` abaixo. Tudo em português,
  inclusive **rotas HTTP** (`/api/animais/buscar`) e **campos JSON** (`nomeComum`).
- **Regra de dependência (Clean Architecture)** — NUNCA viole:
  `Domain` → não depende de ninguém.
  `Application` → depende só de `Domain`.
  `Infrastructure` → depende de `Application` + `Domain`.
  `Api` → depende de `Application` + `Infrastructure` + `Domain`.
- `Buscador.Domain` **nunca** referencia EF Core, Npgsql, MediatR ou qualquer pacote externo.
- **Todo comando `dotnet` roda dentro de `backend/`** (onde está a `.sln`), salvo indicação.
- **Todo comando `npm`/`npx` roda dentro de `frontend/`.**
- Após criar/editar código C#: rode `dotnet build` e garanta **0 erros** antes de marcar a task.
- Nunca commitar segredos. As senhas de dev (`buscador/buscador`) são intencionais e locais.
- Plataforma: **Windows + PowerShell**. Use sintaxe PowerShell (`$env:VAR`, não `$VAR`).

## Legenda de status

- `[ ]` pendente · `[x]` concluída
- **Dep:** tasks que devem estar concluídas antes.
- **DoD** (Definition of Done): como provar que terminou.

## FLUXO GIT (controle de versão)

**Remoto:** `origin` → `https://github.com/junirojr/AnimalsSeach.git` · **Branch:** `main`.

- **Quando commitar:** ao concluir uma task, **só** depois que (a) o DoD passou (build/testes verdes)
  **e** (b) o usuário aprovou. **Um commit por task aprovada.**
- **Quando pushar:** ao concluir uma **fase** inteira (todas as tasks aprovadas) → `git push origin main`.
  (Se preferir manter o remoto sempre atualizado, pode pushar a cada commit.)
- **Nunca** commite com build quebrado, testes vermelhos ou segredos.
- **Mensagem (Conventional Commits):** `feat` / `test` / `docs` / `chore` / `fix`, com escopo opcional.
  Ex.: `feat(domain): adiciona aggregate Animal` · `test(domain): testes de Animal e AnimalId`.
  Termine **toda** mensagem com o trailer de coautoria do Claude.
- **Autenticação:** se `git push` falhar por credenciais, **PARE** e peça ao usuário para autenticar
  (`gh auth login` ou Personal Access Token). Não tente contornar.

> A configuração inicial do Git (init, `.gitignore`, remote, 1º commit) é a task **T0.9**.

---

## MAPA DE JANELAS E PARALELISMO

> Mesma estratégia da Fase 1 (janelas 1A/1B/1C), aplicada a todas as fases. Cada **janela** é uma
> sessão nova, executa 2-3 tasks, fecha em **build/teste verde + commit**; **push no fim da fase**.

### Janelas por fase

**Fase 2 — Infrastructure/Banco** · dep: F1
| Janela | Tasks | Fecha com |
|--------|-------|-----------|
| 2A | T2.1, T2.2, T2.3 | build 0 erros → commit |
| 2B | T2.4, T2.5, T2.8 | build 0 erros → commit |
| 2C | T2.6, T2.7, T2.9 | migration aplicada + teste de integração (Testcontainers) → commit → **push** |

**Fase 3 — Application/CQRS** · dep: F1 · **roda em paralelo com a Fase 2**
| Janela | Tasks | Fecha com |
|--------|-------|-----------|
| 3A | T3.1, T3.2, T3.3 | build → commit |
| 3B | T3.4, T3.5, T3.6 | build → commit |
| 3C | T3.7, T3.8 | testes unitários (Moq) verdes → commit → **push** |

**Fase 4 — Full-Text Search** · dep: F2, F3 · **roda em paralelo com a Fase 5**
| Janela | Tasks | Fecha com |
|--------|-------|-----------|
| 4A | T4.1, T4.2, T4.3 | build → commit |
| 4B | T4.4, T4.5, T4.6 | teste de integração FTS verde → commit → **push** |

**Fase 5 — Busca Semântica** · dep: F2, F3 · **roda em paralelo com a Fase 4**
| Janela | Tasks | Fecha com |
|--------|-------|-----------|
| 5A | T5.1, T5.2 | build → commit |
| 5B | T5.3, T5.4 | build → commit |
| 5C | T5.5, T5.6, T5.7 | teste de integração semântica (Ollama) verde → commit → **push** |

**Fase 6 — Busca Híbrida** · dep: F4 **e** F5
| Janela | Tasks | Fecha com |
|--------|-------|-----------|
| 6A | T6.1, T6.2 | build → commit |
| 6B | T6.3, T6.4 | teste comparativo dos 3 modos verde → commit → **push** |

**Fase 7 — API** · dep: F6 · **frontend (F8) pode começar em paralelo após o contrato congelado**
| Janela | Tasks | Fecha com |
|--------|-------|-----------|
| 7A | T7.1, T7.2, T7.3 | API sobe + Scalar lista endpoints → commit |
| 7B | T7.4, T8.0 (CORS), T7.5, T7.6 | testes de integração da API verdes → commit → **push** |

**Fase 8 — Frontend** · dep: contrato da API (rotas+JSON já no glossário)
| Janela | Tasks | Fecha com |
|--------|-------|-----------|
| 8A | T8.1, T8.2 | build → commit |
| 8B | T8.3, T8.4 (componentes — ver paralelismo abaixo) | build → commit |
| 8C | T8.5, T8.6 | testes de componente (MSW) verdes → commit |
| 8D | T8.7 | E2E Playwright verde → commit → **push** |

### O que PODE rodar em paralelo (janelas/devs simultâneos)

| Trilhas paralelas | Por quê é seguro | Pré-requisito |
|-------------------|------------------|---------------|
| **Fase 2 ∥ Fase 3** (maior ganho) | Projetos diferentes (Infrastructure vs Application); ambos só dependem do Domain (F1) | Interfaces de `Domain/IRepositorioAnimal` e `Application/Compartilhado/*` definidas (saem na F1 / início da 3A) |
| **Fase 4 ∥ Fase 5** | Serviços independentes em `Infrastructure/Busca` (textual vs semântica), arquivos distintos | F2 e F3 prontas |
| **Fase 7 ∥ Fase 8** | Backend (endpoints) e frontend (com MSW) em pastas/repos distintos | **Contrato da API congelado** (rotas + JSON do glossário) |
| **Dentro da 8B** | `BarraBusca`/`AlternadorModoBusca` ∥ `CartaoAnimal`/`DetalheAnimal` | Tipos/serviços (8A) prontos |

### Pontos de SERIALIZAÇÃO (NÃO paralelizar)

- **Migrations (F2)** — um único dono. Duas janelas gerando migration corrompem o snapshot do EF.
- **Switch de `ModoBusca`** no `BuscarAnimaisConsultaManipulador` — tocado em F4, F5 e F6. Evite editar em paralelo; combine na F6.
- **Arquivos de DI** (`AdicionarInfraestrutura`, `AdicionarAplicacao`) — registro de serviços; edição paralela exige merge manual.
- **Fase 6** só começa com **F4 e F5** concluídas.

### Como rodar trilhas em paralelo com segurança

- Cada trilha paralela em sua **branch** (`feat/fase-2-infra`, `feat/fase-3-application`), com merge na `main` ao fim — evita que janelas simultâneas pisem uma na outra no mesmo commit.
- Sequência recomendada: **F1 → (F2 ∥ F3) → (F4 ∥ F5) → F6 → (F7 ∥ F8)**.
- Sem branches (tudo direto na `main`): rode as trilhas **uma de cada vez** para não conflitar.

### Diagrama de dependências

```
            ┌─> F2 (banco) ──┐
F1 (domínio)┤                ├─> F4 (FTS) ─┐
            └─> F3 (CQRS) ───┤             ├─> F6 (híbrida) ─> F7 (API) ──┬─> [pronto]
                             └─> F5 (sem.) ┘                              └┄> F8 (frontend, ∥ após contrato)
   (F2 ∥ F3)                       (F4 ∥ F5)                                  (F7 ∥ F8)

Obs.: F4 e F5 dependem de F2 E F3 (ambas as setas convergem).
```

---

## GLOSSÁRIO DE NOMENCLATURA (EN → PT, sem acento)

> **Autoritativo.** Use exatamente estes nomes em todas as fases. Em caso de dúvida sobre um nome
> não listado, traduza para português sem acento mantendo o padrão (PascalCase em tipos/membros).
>
> **Exceções (mantidos em inglês):** os nomes das **camadas/projetos** já criados na Fase 0 —
> `Buscador.Domain`, `Buscador.Application`, `Buscador.Infrastructure`, `Buscador.Api` e os
> projetos de teste — **não** mudam (são termos de arquitetura e já existem). As **pastas internas**
> e todos os identificadores são em português: `Common`→`Comum`, `Animals`→`Animais`,
> `Persistence`→`Persistencia`, `Search`→`Busca`, `Features`→`Funcionalidades`, `Shared`→`Compartilhado`,
> `Configurations`→`Configuracoes`, `Contracts`→`Contratos` (mantém `Endpoints`, `Fixtures`).
>
> **Arquitetura da Application = Vertical Slices.** Organize por caso de uso, não por tipo técnico:
> `Application/Funcionalidades/<CasoDeUso>/` contém a `Consulta`/`Comando` + `Manipulador` + `Validador`
> juntos (ex.: `Funcionalidades/BuscarAnimais/`). Itens cross-cutting (DTOs, interfaces de serviço,
> `ValidationBehavior`) ficam em `Application/Compartilhado/`. **Não** crie pastas `Queries/`/`Commands/`.

**Padrões / sufixos de arquitetura**

| Inglês | Português |
|--------|-----------|
| `Entity<TId>` | `Entidade<TId>` |
| `AggregateRoot<TId>` | `RaizAgregada<TId>` |
| `ValueObject` | `ObjetoDeValor` |
| `Repository` / `IRepository` | `Repositorio` / `IRepositorio` |
| `Query` (CQRS) | `Consulta` |
| `Command` (CQRS) | `Comando` |
| `Handler` | `Manipulador` |
| `Service` / `IService` | `Servico` / `IServico` |
| `Configuration` (EF) | `Configuracao` |
| `Validator` | `Validador` |
| `Dto` | `Dto` (mantido) |

**Domínio — tipos**

| Inglês | Português |
|--------|-----------|
| `Animal` | `Animal` |
| `AnimalId` | `AnimalId` (mantido) |
| `Diet` | `Dieta` → `Carnivoro`, `Herbivoro`, `Onivoro` |
| `Habitat` | `Habitat` → `Floresta`, `Oceano`, `Deserto`, `Savana`, `Montanha`, `AguaDoce`, `Polar` |
| `ConservationStatus` | `StatusConservacao` → `PoucoPreocupante`, `QuaseAmeacado`, `Vulneravel`, `EmPerigo`, `CriticamenteEmPerigo`, `ExtintoNaNatureza`, `Extinto` |

**Animal — propriedades**

| Inglês | Português |
|--------|-----------|
| `CommonName` | `NomeComum` |
| `ScientificName` | `NomeCientifico` |
| `Description` | `Descricao` |
| `Characteristics` | `Caracteristicas` |
| `Diet` | `Dieta` |
| `Habitat` | `Habitat` |
| `GeographicDistribution` | `DistribuicaoGeografica` |
| `ConservationStatus` | `StatusConservacao` |
| `Tags` | `Tags` (mantido) |
| `Curiosities` | `Curiosidades` |
| `SearchVector` | `VetorBusca` (Fase 2, shadow) |
| `Embedding` | `Embedding` (mantido) |

**Métodos comuns**

| Inglês | Português |
|--------|-----------|
| `Create` | `Criar` |
| `New` / `From` / `Value` | `Novo` / `De` / `Valor` |
| `GetEqualityComponents` | `ObterComponentesDeIgualdade` |
| `GetByIdAsync` | `ObterPorIdAsync` |
| `AddAsync` / `AddRangeAsync` | `AdicionarAsync` / `AdicionarVariosAsync` |
| `GetPagedAsync` | `ObterPaginadoAsync` |
| `ToDto` | `ParaDto` |
| `SearchAsync` / `GenerateAsync` | `BuscarAsync` / `GerarAsync` |

**Casos de uso (CQRS) e busca**

| Inglês | Português |
|--------|-----------|
| `SearchAnimalsQuery` | `BuscarAnimaisConsulta` |
| `GetAnimalByIdQuery` | `ObterAnimalPorIdConsulta` |
| `GetAnimalsQuery` | `ObterAnimaisConsulta` |
| `SeedAnimalsCommand` | `PopularAnimaisComando` |
| `GenerateEmbeddingsCommand` | `GerarEmbeddingsComando` |
| `...Handler` | `...Manipulador` |
| `SearchMode` | `ModoBusca` → `Textual`, `Semantica`, `Hibrida` |
| `AnimalDto` | `AnimalDto` |
| `SearchResultDto` (`Score`) | `ResultadoBuscaDto` (`Pontuacao`) |
| `AnimalSeedData` | `DadosSementeAnimal` |

**Serviços e Infrastructure**

| Inglês | Português |
|--------|-----------|
| `IFullTextSearchService` / impl | `IServicoBuscaTextual` / `ServicoBuscaTextual` |
| `ISemanticSearchService` / impl | `IServicoBuscaSemantica` / `ServicoBuscaSemantica` |
| `IHybridSearchService` / impl | `IServicoBuscaHibrida` / `ServicoBuscaHibrida` |
| `IEmbeddingService` / impl Ollama | `IServicoEmbedding` / `ServicoEmbeddingOllama` |
| `AppDbContext` | `ContextoBanco` |
| `AnimalConfiguration` | `AnimalConfiguracao` |
| `AnimalRepository` | `RepositorioAnimal` |
| `AddInfrastructure` / `AddApplication` | `AdicionarInfraestrutura` / `AdicionarAplicacao` |
| `GlobalExceptionHandler` | `ManipuladorGlobalExcecoes` |

**API — rotas e contrato (tudo em português)**

| Inglês | Português |
|--------|-----------|
| `GET /api/animals/search?q=&mode=&limit=` | `GET /api/animais/buscar?q=&modo=&limite=` |
| `GET /api/animals/{id}` | `GET /api/animais/{id}` |
| `GET /api/animals?page=&size=` | `GET /api/animais?pagina=&tamanho=` |
| `POST /api/animals/seed` | `POST /api/animais/popular` |
| `POST /api/animals/embeddings/generate` | `POST /api/animais/embeddings/gerar` |
| modos: `fulltext`/`semantic`/`hybrid` | `textual`/`semantica`/`hibrida` |
| JSON: `items`, `commonName`, `score` | `itens`, `nomeComum`, `pontuacao` (camelCase) |

**Frontend (componentes e serviços)**

| Inglês | Português |
|--------|-----------|
| `SearchBar` / `SearchModeToggle` | `BarraBusca` / `AlternadorModoBusca` |
| `AnimalCard` / `AnimalDetail` | `CartaoAnimal` / `DetalheAnimal` |
| `searchAnimals` / `getAnimal` | `buscarAnimais` / `obterAnimal` |
| types `Animal` / `SearchResult` | `Animal` / `ResultadoBusca` |

---

# FASE 0 — Ambiente + Esqueleto + Documentação

> 🎓 **Conceito:** aqui montamos o "terreno". Uma *solution* (.sln) agrupa vários *projetos*
> (.csproj). Separamos em 4 projetos (Domain, Application, Infrastructure, Api) porque cada um
> tem uma responsabilidade — isso é a base da Clean Architecture. Docker sobe o banco e o Ollama
> sem instalar nada na sua máquina. Nada de regra de negócio nesta fase, só estrutura.

### [x] T0.1 — Verificar pré-requisitos instalados
**Objetivo:** confirmar que as ferramentas necessárias existem.
**Dep:** nenhuma.
**Passos:**
1. Rode cada comando e anote a versão:
   - `dotnet --version` (esperado: 10.x — ex.: 10.0.300)
   - `docker --version`
   - `node --version` (esperado: 22.x)
   - `ollama --version`
2. Se qualquer um faltar, **PARE** e informe o usuário qual instalar.
**DoD:** os 4 comandos retornam versão sem erro.

### [x] T0.2 — Criar `docker-compose.yml` (Postgres + Ollama)
**Objetivo:** infra local de banco e embeddings.
**Dep:** T0.1.
**Arquivo:** `docker-compose.yml` (raiz).
**Passos:** copiar exatamente o YAML da seção "Docker Compose" do [PLAN.md](PLAN.md) (serviços `postgres` com `pgvector/pgvector:pg16` e `ollama` com volume `ollama_data`).
**DoD:** `docker compose config` valida o arquivo sem erro.

### [x] T0.3 — Subir infra e baixar modelo de embeddings
**Objetivo:** containers no ar + modelo `nomic-embed-text` disponível.
**Dep:** T0.2.
**Passos:**
1. `docker compose up -d`
2. `docker compose ps` (ambos `running`).
3. `docker exec ollama ollama pull nomic-embed-text`
**DoD:** `docker exec ollama ollama list` mostra `nomic-embed-text`.

### [x] T0.4 — Criar solution e projetos backend
**Objetivo:** estrutura de 4 projetos + 3 de testes, com referências corretas.
**Dep:** T0.1.
**Passos (rodar em `backend/`):**
```powershell
dotnet new sln -n Buscador
dotnet new classlib -n Buscador.Domain -o src/Buscador.Domain
dotnet new classlib -n Buscador.Application -o src/Buscador.Application
dotnet new classlib -n Buscador.Infrastructure -o src/Buscador.Infrastructure
dotnet new web        -n Buscador.Api -o src/Buscador.Api
dotnet new xunit -n Buscador.Domain.Tests      -o tests/Buscador.Domain.Tests
dotnet new xunit -n Buscador.Application.Tests -o tests/Buscador.Application.Tests
dotnet new xunit -n Buscador.Api.Tests         -o tests/Buscador.Api.Tests
# adicionar todos à solution
dotnet sln add (Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object FullName)
# referências (regra de dependência)
dotnet add src/Buscador.Application reference src/Buscador.Domain
dotnet add src/Buscador.Infrastructure reference src/Buscador.Application src/Buscador.Domain
dotnet add src/Buscador.Api reference src/Buscador.Infrastructure src/Buscador.Application src/Buscador.Domain
dotnet add tests/Buscador.Domain.Tests reference src/Buscador.Domain
dotnet add tests/Buscador.Application.Tests reference src/Buscador.Application src/Buscador.Domain
dotnet add tests/Buscador.Api.Tests reference src/Buscador.Api src/Buscador.Infrastructure src/Buscador.Application src/Buscador.Domain
```
Apague os arquivos `Class1.cs` gerados em cada classlib.
**DoD:** `dotnet build` na pasta `backend/` retorna **0 erros**.

### [x] T0.5 — Inicializar frontend Next.js
**Objetivo:** app Next.js 15 com TS + Tailwind.
**Dep:** T0.1.
**Passos (na raiz):**
```powershell
npx create-next-app@latest frontend --typescript --tailwind --app --eslint --src-dir --use-npm --no-import-alias
```
**DoD:** `cd frontend; npm run build` conclui sem erro.

### [x] T0.6 — Criar pastas de documentação (Diataxis + docs/ai)
**Objetivo:** esqueleto de `docs/`.
**Dep:** nenhuma.
**Passos:** criar os diretórios `docs/tutorials`, `docs/how-to`, `docs/reference`, `docs/explanation`, `docs/ai` e os arquivos `.md` listados nas seções "Documentação — Diataxis" e "docs/ai/" do [PLAN.md](PLAN.md). Cada arquivo começa com um título `#` e a frase "TODO: preencher na fase correspondente." Não escrever conteúdo ainda.
**DoD:** todos os arquivos `.md` das duas seções existem (mesmo que stub).

### [x] T0.7 — Criar arquivos CLAUDE.md
**Objetivo:** contexto de IA por camada.
**Dep:** T0.4, T0.6.
**Arquivos:** raiz `/CLAUDE.md`, `backend/CLAUDE.md`, um `CLAUDE.md` em cada projeto `src/Buscador.*`, `backend/tests/CLAUDE.md`, `frontend/CLAUDE.md`.
**Passos:** preencher cada um com o "Conteúdo chave" descrito na seção "Engenharia de Contexto" do [PLAN.md](PLAN.md). Manter curto e objetivo (até ~20 linhas cada).
**DoD:** todos os CLAUDE.md existem e contêm a regra de dependência (no caso dos backend).

### [x] T0.8 — Preencher docs/ai iniciais
**Objetivo:** contexto semântico mínimo.
**Dep:** T0.6.
**Arquivos:** `docs/ai/PROJECT_CONTEXT.md`, `docs/ai/PROJECT_PLAYBOOK.md`, `docs/ai/DESIGN_PATTERNS.md`.
**Passos:** preencher com base nas seções "Context", "Stack" e "Engenharia de Contexto" do PLAN.md. (`PROJECT_DOMAIN_MAP.md` e `PROJECT_RISK_REGISTER.md` ficam como stub até a Fase 1.)
**DoD:** os 3 arquivos têm conteúdo real (não stub).

### [x] T0.9 — Inicializar Git, `.gitignore`, remote e primeiro commit
**Objetivo:** colocar o projeto sob versionamento e conectar ao GitHub. Como a Fase 0 já foi
executada, este **primeiro commit captura todo o trabalho dela**.
**Dep:** T0.1–T0.8 concluídas e aprovadas.
**Passos (na raiz do projeto):**
```powershell
git init
dotnet new gitignore        # gera .gitignore padrão .NET na raiz
```
1. Garanta que o `.gitignore` da raiz cobre também o frontend e segredos. Acrescente as linhas
   que faltarem: `node_modules/`, `.next/`, `.env*.local`, `.claude/settings.local.json`.
2. Conectar o remoto:
   ```powershell
   git remote add origin https://github.com/junirojr/AnimalsSeach.git
   ```
3. Primeiro commit (captura toda a Fase 0). Mensagem:
   `chore: estrutura inicial, docker-compose, projetos e docs (Fase 0)` + trailer de coautoria do Claude.
   ```powershell
   git add -A
   git commit   # com a mensagem acima
   ```
4. Definir branch e publicar:
   ```powershell
   git branch -M main
   git push -u origin main
   ```
**Guard rails:**
- Se o `push` falhar por **autenticação**, PARE e peça ao usuário para autenticar (`gh auth login` ou token).
- Se o remoto **já tiver conteúdo** (ex.: README criado no GitHub), rode antes:
  `git pull origin main --allow-unrelated-histories`, resolva conflitos simples e então faça o push.
- Confirme com `git status` que `bin/`, `obj/`, `node_modules/` e `.next/` **não** estão sendo rastreados.
**DoD:** `git status` limpo; `git remote -v` mostra `origin`; o commit aparece no GitHub.

---

# FASE 1 — Domain Layer

> Projeto: `src/Buscador.Domain`. **Zero dependências externas.**
>
> 🎓 **Conceito:** o Domain é o coração do sistema — só regras de negócio, sem banco, sem
> framework. *Entity* tem identidade (dois animais com mesmo Id são "o mesmo"). *Value Object*
> não tem identidade, vale pelo conteúdo (ex.: `AnimalId`). *Aggregate Root* é a entidade
> "porta de entrada" que controla suas invariantes. Usamos método de fábrica `Criar(...)` em
> vez de construtor público para garantir que um `Animal` nunca nasça inválido.

> 🪟 **Execução em janelas separadas** (para não esgotar o contexto e evitar alucinação nas tasks finais):
> esta fase é dividida em **3 janelas**. Cada janela é uma sessão nova, executa só seu lote, fecha em
> ponto seguro (build/testes verdes) e **commita**. A próxima janela começa "fresca" lendo o disco + git.
>
> | Janela | Tasks | Fecha com |
> |--------|-------|-----------|
> | **1A** | T1.1, T1.2, T1.3 (base classes, `AnimalId`, enums) | `dotnet build` 0 erros → commit |
> | **1B** | T1.4, T1.5 (`Animal` aggregate, `IRepositorioAnimal`) | `dotnet build` 0 erros → commit |
> | **1C** | T1.6, T1.7 (testes do Domain + docs) | `dotnet test` verde → commit → **push** (fim da fase) |
>
> Regra geral: fases longas devem ser quebradas assim — lotes de 2-3 tasks por janela, sempre
> terminando em build/teste verde + commit.

> 📌 **Estado atual:** a Fase 1 foi **resetada** (revertido o código adiantado) — voltamos ao estado limpo
> pós-Fase 0. O Domain e a Application estão **vazios** (só os `.csproj`). A Fase 1 será refeita do zero,
> em português sem acento e seguindo o `GLOSSÁRIO`, pelas janelas 1A/1B/1C. **Atenção:** o `Animal` (T1.4)
> deve nascer **completo**, com todos os campos do "Modelo de Domínio" do PLAN.md (`NomeComum`,
> `NomeCientifico`, `Descricao`, `Caracteristicas`, `Dieta`, `Habitat`, `DistribuicaoGeografica`,
> `StatusConservacao`, `Tags`, `Curiosidades`).

### [ ] T1.1 — Base classes: `Entidade<TId>`, `RaizAgregada<TId>`, `ObjetoDeValor`
**Objetivo:** blocos base de DDD.
**Dep:** T0.4.
**Arquivos:** `src/Buscador.Domain/Comum/Entidade.cs`, `RaizAgregada.cs`, `ObjetoDeValor.cs`.
**Passos:**
- `Entidade<TId>`: propriedade `Id` (tipo genérico `TId`), igualdade por `Id` (override `Equals`/`GetHashCode`).
- `RaizAgregada<TId>`: herda de `Entidade<TId>`.
- `ObjetoDeValor`: classe abstrata com igualdade estrutural (método abstrato `ObterComponentesDeIgualdade()` retornando `IEnumerable<object>`).
**DoD:** `dotnet build` 0 erros.

### [ ] T1.2 — Value Object `AnimalId`
**Objetivo:** identificador tipado.
**Dep:** T1.1.
**Arquivo:** `src/Buscador.Domain/Animais/AnimalId.cs`.
**Passos:** `record`/`ObjetoDeValor` que encapsula um `Guid Valor`. Método estático `Novo()` que gera novo Guid e `De(Guid)`.
**DoD:** build 0 erros.

### [ ] T1.3 — Enums do domínio
**Objetivo:** `Dieta`, `Habitat`, `StatusConservacao`.
**Dep:** T0.4.
**Arquivos:** `src/Buscador.Domain/Animais/Dieta.cs`, `Habitat.cs`, `StatusConservacao.cs`.
**Passos:**
- `Dieta`: `Carnivoro`, `Herbivoro`, `Onivoro`.
- `Habitat`: `Floresta`, `Oceano`, `Deserto`, `Savana`, `Montanha`, `AguaDoce`, `Polar` (cobrir os principais).
- `StatusConservacao`: escala IUCN — `PoucoPreocupante`, `QuaseAmeacado`, `Vulneravel`, `EmPerigo`, `CriticamenteEmPerigo`, `ExtintoNaNatureza`, `Extinto`.
**DoD:** build 0 erros.

### [ ] T1.4 — Aggregate `Animal`
**Objetivo:** entidade central.
**Dep:** T1.1, T1.2, T1.3.
**Arquivo:** `src/Buscador.Domain/Animais/Animal.cs`.
**Passos:** seguir o modelo da seção "Modelo de Domínio" do PLAN.md (propriedades em português: `NomeComum`,
`NomeCientifico`, `Descricao`, `Caracteristicas`, `Dieta`, `Habitat`, `DistribuicaoGeografica`,
`StatusConservacao`, `Tags`, `Curiosidades`), **MAS sem os campos `VetorBusca` e `Embedding`** (esses
são da Infrastructure e serão adicionados na Fase 2 — não referenciar Npgsql/pgvector aqui).
- Propriedades com `private set`.
- Construtor privado + método de fábrica estático `Criar(...)` que valida: `NomeComum` e `NomeCientifico` não vazios; lança `ArgumentException` se inválido.
**DoD:** build 0 erros; `Animal` não importa nenhum pacote externo.

### [ ] T1.5 — Interface `IRepositorioAnimal`
**Objetivo:** contrato de persistência (só assinatura).
**Dep:** T1.4.
**Arquivo:** `src/Buscador.Domain/Animais/IRepositorioAnimal.cs`.
**Passos:** métodos assíncronos: `Task<Animal?> ObterPorIdAsync(AnimalId id, CancellationToken)`, `Task AdicionarAsync(Animal, CancellationToken)`, `Task AdicionarVariosAsync(IEnumerable<Animal>, CancellationToken)`, `Task<IReadOnlyList<Animal>> ObterPaginadoAsync(int pagina, int tamanho, CancellationToken)`. (Métodos de busca serão adicionados nas fases 4-6.)
**DoD:** build 0 erros.

### [ ] T1.6 — Testes unitários do Domain
**Objetivo:** validar regras do aggregate e VO.
**Dep:** T1.4, T1.2.
**Passos:**
1. Em `tests/Buscador.Domain.Tests` adicionar pacote `FluentAssertions`: `dotnet add tests/Buscador.Domain.Tests package FluentAssertions`.
2. Criar `Animais/AnimalTests.cs` e `Animais/AnimalIdTests.cs`.
3. Testes (nome `Metodo_Cenario_Resultado`, em português):
   - `Criar_ComDadosValidos_RetornaAnimal`
   - `Criar_ComNomeComumVazio_LancaArgumentException`
   - `Novo_GeraValoresUnicos`
   - `AnimalId_ComMesmoValor_SaoIguais`
**DoD:** `dotnet test tests/Buscador.Domain.Tests` — todos verdes.

### [ ] T1.7 — Preencher `PROJECT_DOMAIN_MAP.md`
**Objetivo:** documentar linguagem ubíqua.
**Dep:** T1.4.
**Arquivo:** `docs/ai/PROJECT_DOMAIN_MAP.md` + `docs/reference/domain-model.md`.
**Passos:** descrever entidade `Animal`, objetos de valor, enums e glossário (em português), usando os
nomes do `GLOSSÁRIO`.
**DoD:** ambos os arquivos preenchidos.

---

# FASE 2 — Infrastructure: Banco

> Projeto: `src/Buscador.Infrastructure`.
>
> 🎓 **Conceito:** a Infrastructure implementa os contratos (interfaces) que o Domain definiu.
> *EF Core* é o ORM: traduz objetos C# em tabelas SQL. *Migrations* são scripts versionados que
> evoluem o schema do banco. *pgvector* adiciona um tipo `vector` ao Postgres para guardar
> embeddings. Mantemos o Domain limpo usando *shadow properties*: as colunas `search_vector` e
> `embedding` existem no banco e no mapeamento, mas **não** na classe `Animal`.

### [ ] T2.1 — Instalar pacotes NuGet de persistência
**Objetivo:** EF Core + Npgsql + pgvector.
**Dep:** T1.5.
**Passos (em `src/Buscador.Infrastructure`):**
```powershell
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Pgvector
dotnet add package Pgvector.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
```
**Atenção à versão:** o projeto é `net10.0`, então os pacotes do EF Core e o provider
`Npgsql.EntityFrameworkCore.PostgreSQL` devem ser da **versão 10.x** (o provider Npgsql acompanha
a major do EF Core). `dotnet add package` sem versão já resolve a última estável compatível — após
instalar, confira no `.csproj` que as versões do EF Core e do Npgsql começam com `10.`.
`Pgvector` e `Pgvector.EntityFrameworkCore` têm versionamento próprio (instale a última estável).
**DoD:** build 0 erros; versões do EF Core e Npgsql no `.csproj` começam com `10.`.

### [ ] T2.2 — Estender `Animal` com campos de busca
**Objetivo:** adicionar `SearchVector` (tsvector) e `Embedding` (vector).
**Dep:** T2.1.
**Decisão de arquitetura:** como `Domain` não pode referenciar Npgsql/pgvector, esses campos
ficam como propriedades **shadow** mapeadas via Fluent API na Infrastructure, OU como propriedades
em `Animal` usando tipos primitivos. **Use shadow properties** (mapeadas em `AnimalConfiguration`)
para manter o Domain limpo. Não adicione `NpgsqlTsVector`/`Vector` na classe `Animal`.
**DoD:** decisão registrada em `docs/ai/PROJECT_RISK_REGISTER.md`; `Animal` continua sem deps externas.

### [ ] T2.3 — `AppDbContext`
**Objetivo:** contexto EF.
**Dep:** T2.1.
**Arquivo:** `src/Buscador.Infrastructure/Persistence/AppDbContext.cs`.
**Passos:** `DbSet<Animal> Animals`. No `OnModelCreating`: `modelBuilder.HasPostgresExtension("vector")` e aplicar configurações via `ApplyConfigurationsFromAssembly`.
**DoD:** build 0 erros.

### [ ] T2.4 — `AnimalConfiguration` (Fluent API)
**Objetivo:** mapear tabela `animals`, incluindo coluna `search_vector` (tsvector) e `embedding` (vector(768)).
**Dep:** T2.3.
**Arquivo:** `src/Buscador.Infrastructure/Persistence/Configurations/AnimalConfiguration.cs`.
**Passos:** mapear todas as colunas; `embedding` como `vector(768)` nullable; `search_vector` como `tsvector` (shadow). Mapear `AnimalId` via conversão para `Guid`. Arrays (`Tags`) como `text[]`.
**DoD:** build 0 erros.

### [ ] T2.5 — Registro de DI da Infrastructure
**Objetivo:** método `AddInfrastructure(IServiceCollection, IConfiguration)`.
**Dep:** T2.3.
**Arquivo:** `src/Buscador.Infrastructure/DependencyInjection.cs`.
**Passos:** registrar `AppDbContext` com `UseNpgsql(...).UseVector()` lendo connection string `"Postgres"`; registrar `IAnimalRepository` → `AnimalRepository` (criado em T2.7).
**DoD:** build 0 erros (pode comentar o registro do repo até T2.7).

### [ ] T2.6 — Migration inicial + índices
**Objetivo:** criar schema com índice GIN (FTS) e HNSW (vetor).
**Dep:** T2.4, T2.5, e configuração da connection string na Api (T2.8 pode vir antes).
**Passos:**
```powershell
# em backend/
dotnet tool install --global dotnet-ef   # se ainda não tiver
dotnet ef migrations add InitialCreate --project src/Buscador.Infrastructure --startup-project src/Buscador.Api
```
Editar a migration para adicionar (via `migrationBuilder.Sql`):
- índice GIN em `search_vector`: `CREATE INDEX ix_animals_search_vector ON animals USING GIN (search_vector);`
- índice HNSW em `embedding`: `CREATE INDEX ix_animals_embedding ON animals USING hnsw (embedding vector_cosine_ops);`
Depois: `dotnet ef database update --project src/Buscador.Infrastructure --startup-project src/Buscador.Api`.
**DoD:** `docker exec -it <postgres> psql -U buscador -d buscador -c "\d animals"` mostra a tabela com os índices.

### [ ] T2.7 — `AnimalRepository`
**Objetivo:** implementar `IAnimalRepository`.
**Dep:** T2.3, T1.5.
**Arquivo:** `src/Buscador.Infrastructure/Persistence/AnimalRepository.cs`.
**Passos:** implementar todos os métodos da interface usando `AppDbContext`. (Métodos de busca virão depois.)
**DoD:** build 0 erros; descomentar registro de DI em T2.5.

### [ ] T2.8 — Connection string + chamar `AddInfrastructure` na Api
**Objetivo:** Api conhece o banco.
**Dep:** T2.5.
**Arquivos:** `src/Buscador.Api/appsettings.json`, `src/Buscador.Api/Program.cs`.
**Passos:** adicionar `ConnectionStrings:Postgres = "Host=localhost;Port=5432;Database=buscador;Username=buscador;Password=buscador"`; em `Program.cs` chamar `builder.Services.AddInfrastructure(builder.Configuration)`.
**DoD:** `dotnet run --project src/Buscador.Api` sobe sem erro de conexão.

### [ ] T2.9 — Teste de integração base (Testcontainers)
**Objetivo:** fixture que sobe Postgres real.
**Dep:** T2.7.
**Passos:**
1. Em `tests/Buscador.Api.Tests`: `dotnet add package Testcontainers.PostgreSql`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing`.
2. Criar `Fixtures/ApiTestFixture.cs`: inicia container `pgvector/pgvector:pg16`, aplica migrations, expõe `WebApplicationFactory` apontando para a connection string do container.
3. Criar 1 teste de fumaça `Repository_AddAndGetById_PersistsAnimal`.
**DoD:** `dotnet test tests/Buscador.Api.Tests` verde (Docker precisa estar rodando).

---

# FASE 3 — Application Layer (CQRS)

> Projeto: `src/Buscador.Application`.
>
> 🎓 **Conceito:** *CQRS* separa leitura (Query) de escrita (Command). Cada operação vira uma
> mensagem (ex.: `GetAnimalByIdQuery`) com um *Handler* que a executa. O *MediatR* é o "carteiro"
> que entrega cada mensagem ao handler certo, desacoplando quem pede de quem faz. *FluentValidation*
> valida a entrada antes de chegar no handler. *DTO* é um objeto de transporte — devolvemos um
> `AnimalDto` em vez do `Animal` do domínio para não vazar regras internas.

### [ ] T3.1 — Instalar MediatR + FluentValidation
**Dep:** T1.5.
**Passos (em `src/Buscador.Application`):**
```powershell
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```
Criar `src/Buscador.Application/DependencyInjection.cs` com `AddApplication()` que registra MediatR (assembly atual) e validators.
**DoD:** build 0 erros.

### [ ] T3.2 — DTO `AnimalDto` + mapeamento
**Objetivo:** contrato de saída da Application.
**Dep:** T3.1.
**Arquivo:** `src/Buscador.Application/Animals/AnimalDto.cs`.
**Passos:** `record` com os campos públicos de `Animal` (sem campos de busca). Método/extensão `ToDto(this Animal)`.
**DoD:** build 0 erros.

### [ ] T3.3 — `GetAnimalByIdQuery` + Handler
**Dep:** T3.2.
**Arquivos:** `src/Buscador.Application/Animals/Queries/GetAnimalById/GetAnimalByIdQuery.cs` (+ `Handler`).
**Passos:** query `record GetAnimalByIdQuery(Guid Id) : IRequest<AnimalDto?>`; handler usa `IAnimalRepository`.
**DoD:** build 0 erros.

### [ ] T3.4 — `GetAnimalsQuery` (paginada) + Handler + Validator
**Dep:** T3.2.
**Arquivos:** pasta `Queries/GetAnimals/`.
**Passos:** query `(int Page, int Size)`; validator: `Page >= 1`, `Size` entre 1 e 100.
**DoD:** build 0 erros.

### [ ] T3.5 — Dados-semente: 10 animais (MVP)
**Objetivo:** fonte de dados inicial para `SeedAnimalsCommand`. Começamos com **10** para validar
o pipeline ponta-a-ponta rápido; expandimos para 50 em T9.0 depois que tudo funcionar.
**Dep:** T1.4.
**Arquivo:** `src/Buscador.Application/Animals/Seed/AnimalSeedData.cs`.
**Passos:** lista estática de **10 animais** com dados **em português** ricos (descrição,
características, dieta, habitat, distribuição, status, tags, curiosidades). Garantir variedade de
habitat/dieta para os testes de busca terem contraste (ex.: leão, lobo, golfinho, águia, cobra,
sapo, tubarão, urso-polar, elefante, papagaio). Texto descritivo bom o suficiente para FTS e
embeddings (2-4 frases por campo de texto).
**DoD:** build 0 erros; lista tem exatamente 10 itens, cada um com todos os campos preenchidos.

### [ ] T3.6 — `SeedAnimalsCommand` + Handler
**Dep:** T3.5, T2.7.
**Arquivos:** pasta `Commands/SeedAnimals/`.
**Passos:** command `IRequest<int>` (retorna nº inserido); handler insere os 50 via `AddRangeAsync` se a tabela estiver vazia (idempotente).
**DoD:** build 0 erros.

### [ ] T3.7 — Chamar `AddApplication` na Api
**Dep:** T3.1, T2.8.
**Arquivo:** `src/Buscador.Api/Program.cs`.
**DoD:** `dotnet run --project src/Buscador.Api` sobe sem erro.

### [ ] T3.8 — Testes unitários da Application (Moq)
**Dep:** T3.3, T3.4, T3.6.
**Passos:**
1. Em `tests/Buscador.Application.Tests`: `dotnet add package Moq` e `FluentAssertions`.
2. Testar handlers com repositório mockado:
   - `GetAnimalById_WhenExists_ReturnsDto`
   - `GetAnimalById_WhenMissing_ReturnsNull`
   - `SeedAnimals_WhenEmpty_InsertsAllSeedData` (10 nesta fase)
   - `GetAnimals_WithInvalidPage_FailsValidation`
**DoD:** `dotnet test tests/Buscador.Application.Tests` verde.

---

# FASE 4 — Full-Text Search

> 🎓 **Conceito:** busca *full-text* (FTS) encontra documentos por palavras-chave, ignorando
> acentos e variações ("caça", "caçar", "caçando"). O Postgres transforma o texto num `tsvector`
> (lista de palavras normalizadas) e a busca num `tsquery`. Um *trigger* mantém o `tsvector`
> atualizado automaticamente. `ts_rank` dá uma nota de relevância para ordenar os resultados.
> Um índice GIN torna isso rápido. É busca por **palavra**, não por **significado** (isso é a Fase 5).

### [ ] T4.1 — Trigger de `search_vector`
**Objetivo:** popular `search_vector` automaticamente no banco.
**Dep:** T2.6.
**Passos:** nova migration `AddSearchVectorTrigger` com `migrationBuilder.Sql` criando função + trigger PostgreSQL que concatena `common_name`, `scientific_name`, `description`, `characteristics`, `curiosities` em `search_vector` usando `to_tsvector('portuguese', ...)` no `INSERT`/`UPDATE`. Aplicar com `database update`.
**DoD:** após seed, `SELECT common_name, search_vector FROM animals LIMIT 1;` mostra vetor não-nulo.

### [ ] T4.2 — Resultado de busca: `SearchResultDto`
**Objetivo:** DTO com score.
**Dep:** T3.2.
**Arquivo:** `src/Buscador.Application/Animals/Search/SearchResultDto.cs`.
**Passos:** `record` com `AnimalDto Animal` + `double Score`.
**DoD:** build 0 erros.

### [ ] T4.3 — Interface `IFullTextSearchService` + impl
**Objetivo:** busca FTS com `ts_rank`.
**Dep:** T4.1, T4.2.
**Arquivos:** interface em `Application/Animals/Search/IFullTextSearchService.cs`; impl em `Infrastructure/Search/FullTextSearchService.cs`.
**Passos:** método `Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int limit, CancellationToken)`. Usar SQL com `to_tsquery('portuguese', ...)` + `ts_rank` ordenando por score desc. Registrar no DI.
**DoD:** build 0 erros.

### [ ] T4.4 — `SearchAnimalsQuery` modo `fulltext`
**Objetivo:** query CQRS com enum de modo.
**Dep:** T4.3.
**Arquivos:** pasta `Queries/SearchAnimals/` + enum `SearchMode { FullText, Semantic, Hybrid }`.
**Passos:** query `(string Q, SearchMode Mode, int Limit)`; validator: `Q` não vazio. Handler: por enquanto só trata `FullText` (chama `IFullTextSearchService`); demais modos lançam `NotSupportedException` temporária.
**DoD:** build 0 erros.

### [ ] T4.5 — Teste de integração FTS
**Dep:** T4.4, T2.9.
**Passos:** teste que faz seed, busca por palavra presente numa descrição e verifica que o animal esperado aparece no topo. Ex.: `Search_FullText_WithKeyword_ReturnsRelevantAnimal`.
**DoD:** `dotnet test tests/Buscador.Api.Tests` verde.

### [ ] T4.6 — Doc `how-fts-works.md`
**Dep:** T4.4.
**Arquivo:** `docs/explanation/how-fts-works.md`.
**DoD:** preenchido (explica tsvector/tsquery/ts_rank).

---

# FASE 5 — Busca Semântica

> 🎓 **Conceito:** busca *semântica* encontra por **significado**, não por palavra exata. Um modelo
> de *embeddings* (o `nomic-embed-text` no Ollama) converte cada texto num vetor de 768 números que
> representa seu "sentido". Textos parecidos ficam perto no espaço vetorial. Medimos proximidade pela
> *cosine distance* (operador `<=>` do pgvector). Assim "animal que caça em bando" encontra o lobo
> mesmo que essa frase não apareça na descrição. Índice HNSW acelera a busca por vizinhos próximos.

### [ ] T5.1 — Pacotes de IA (Ollama)
**Dep:** T2.1.
**Passos (em `src/Buscador.Infrastructure`):**
```powershell
dotnet add package Microsoft.Extensions.AI
dotnet add package Microsoft.Extensions.AI.Ollama
```
**DoD:** build 0 erros.

### [ ] T5.2 — `IEmbeddingService` + `OllamaEmbeddingService`
**Objetivo:** gerar embedding de um texto via Ollama.
**Dep:** T5.1.
**Arquivos:** interface em `Application/Animals/Embeddings/IEmbeddingService.cs`; impl em `Infrastructure/Embeddings/OllamaEmbeddingService.cs`.
**Passos:** método `Task<float[]> GenerateAsync(string text, CancellationToken)` usando modelo `nomic-embed-text` (endpoint `http://localhost:11434`, configurável). Registrar no DI; ler base URL de config.
**DoD:** build 0 erros.

### [ ] T5.3 — `GenerateEmbeddingsCommand` + Handler (batch)
**Objetivo:** popular coluna `embedding` de todos os animais.
**Dep:** T5.2, T2.7.
**Arquivos:** pasta `Commands/GenerateEmbeddings/`.
**Passos:** handler busca animais sem embedding, gera embedding concatenando `description + characteristics + curiosities`, salva no banco. Retorna nº processado. Processar em lotes para não sobrecarregar Ollama.
**DoD:** build 0 erros.

### [ ] T5.4 — `ISemanticSearchService` + impl (cosine)
**Objetivo:** busca por `<=>` (cosine distance) no pgvector.
**Dep:** T5.2, T4.2.
**Arquivos:** interface em Application; impl em `Infrastructure/Search/SemanticSearchService.cs`.
**Passos:** gera embedding da query, ordena por `embedding <=> @queryVector` asc, converte distância em score `1 - distance`. Registrar DI.
**DoD:** build 0 erros.

### [ ] T5.5 — `SearchAnimalsQuery` modo `semantic`
**Dep:** T5.4, T4.4.
**Passos:** no handler de `SearchAnimalsQuery`, tratar `SearchMode.Semantic` chamando `ISemanticSearchService`.
**DoD:** build 0 erros.

### [ ] T5.6 — Teste de integração semântica (OBRIGATÓRIO)
**Dep:** T5.5, T2.9.
**Passos:** teste faz seed + gera embeddings + busca por consulta conceitual (ex.: "animal que caça
em bando") e verifica que retorna resultados relevantes (ex.: o lobo entre os primeiros).
**Atenção:** este teste é **obrigatório** — depende de Ollama no ar e **deve falhar** se o Ollama
estiver indisponível (não marcar como skip). Documentar em `docs/how-to/run-integration-tests.md`
que Docker + Ollama (`docker compose up -d` e modelo já baixado em T0.3) são pré-requisitos para
rodar a suíte de integração.
**DoD:** teste passa com Ollama rodando; falha (não pula) se Ollama estiver fora.

### [ ] T5.7 — Docs de embeddings
**Dep:** T5.5.
**Arquivos:** `docs/explanation/how-embeddings-work.md`, `docs/how-to/configure-ollama-gpu.md`.
**DoD:** preenchidos.

---

# FASE 6 — Busca Híbrida (RRF)

> 🎓 **Conceito:** FTS é ótima em termos exatos; a semântica é ótima em intenção. A busca *híbrida*
> junta as duas. *RRF* (Reciprocal Rank Fusion) é uma fórmula simples que combina as duas listas
> usando a **posição** (rank) de cada resultado, não as notas brutas (que estão em escalas diferentes):
> `score = Σ 1/(k + rank)`. O `k` (≈60) suaviza o peso das primeiras posições. Resultado: o que
> aparece bem nas duas buscas sobe ao topo.

### [ ] T6.1 — `HybridSearchService` com RRF
**Objetivo:** fundir rankings FTS + semântico via Reciprocal Rank Fusion.
**Dep:** T4.3, T5.4.
**Arquivo:** `src/Buscador.Infrastructure/Search/HybridSearchService.cs` (+ interface em Application).
**Passos:** executa FTS e semântico, calcula `score = Σ 1/(k + rank_i)` (k=60 padrão, configurável), ordena desc. Pesos configuráveis.
**DoD:** build 0 erros.

### [ ] T6.2 — `SearchAnimalsQuery` modo `hybrid`
**Dep:** T6.1, T5.5.
**Passos:** tratar `SearchMode.Hybrid` no handler; remover o `NotSupportedException`.
**DoD:** build 0 erros; todos os 3 modos funcionam.

### [ ] T6.3 — Teste comparativo dos 3 modos
**Dep:** T6.2.
**Passos:** teste que roda a mesma query nos 3 modos e valida que cada um retorna resultados (não precisa comparar qualidade, só que funcionam).
**DoD:** `dotnet test tests/Buscador.Api.Tests` verde.

### [ ] T6.4 — Docs de busca
**Dep:** T6.2.
**Arquivos:** `docs/explanation/rrf-algorithm.md`, `docs/reference/search-modes.md`, `docs/how-to/tune-search-weights.md`.
**DoD:** preenchidos.

---

# FASE 7 — API Layer

> Projeto: `src/Buscador.Api`. **Sem lógica de negócio — só HTTP.**
>
> 🎓 **Conceito:** a Api é a "casca" HTTP. *Minimal API* mapeia rotas com poucas linhas. Cada endpoint
> só faz três coisas: ler a requisição, mandar a mensagem ao MediatR e devolver a resposta — nenhuma
> regra de negócio aqui. *Scalar* gera uma UI de documentação a partir do OpenAPI. O `IExceptionHandler`
> centraliza o tratamento de erros (ex.: validação → 400, erro inesperado → 500) em vez de espalhar
> try/catch.

### [ ] T7.1 — Pacote MediatR na Api + DTOs de contrato
**Dep:** T3.7.
**Passos:** garantir que a Api resolve `IMediator`. Criar pasta `Contracts/` com requests/responses HTTP se diferirem dos DTOs da Application.
**DoD:** build 0 erros.

### [ ] T7.2 — Endpoints de animais
**Objetivo:** mapear os 5 endpoints do PLAN.md.
**Dep:** T7.1, T6.2.
**Arquivo:** `src/Buscador.Api/Endpoints/AnimalEndpoints.cs` (extensão `MapAnimalEndpoints`).
**Endpoints:**
- `GET /api/animals/search?q=&mode=&limit=`
- `GET /api/animals/{id}`
- `GET /api/animals?page=&size=`
- `POST /api/animals/seed`
- `POST /api/animals/embeddings/generate`
Cada um só faz: receber input → `mediator.Send(...)` → retornar `Results.Ok/NotFound`.
**DoD:** build 0 erros; endpoints aparecem ao rodar a Api.

### [ ] T7.3 — Scalar (OpenAPI UI)
**Dep:** T7.2.
**Passos:** `dotnet add src/Buscador.Api package Scalar.AspNetCore`; adicionar `AddOpenApi()` + `MapOpenApi()` + `MapScalarApiReference()`.
**DoD:** abrir `/scalar` no navegador mostra os endpoints.

### [ ] T7.4 — Global error handling
**Objetivo:** tratar `ValidationException` (400) e exceções genéricas (500).
**Dep:** T7.2.
**Arquivo:** `src/Buscador.Api/ExceptionHandling/GlobalExceptionHandler.cs` (`IExceptionHandler`).
**Passos:** registrar com `AddExceptionHandler` + `AddProblemDetails`; mapear `ValidationException` → 400 com detalhes; resto → 500.
**DoD:** chamar `/api/animals/search?q=` (vazio) retorna 400.

### [ ] T7.5 — Testes de integração da API (WebApplicationFactory)
**Dep:** T7.4, T2.9.
**Arquivos:** `tests/Buscador.Api.Tests/Animals/SearchEndpointTests.cs`, `SeedEndpointTests.cs`.
**Passos:** usar `ApiTestFixture`. Testar: seed retorna a contagem do `AnimalSeedData` (10 nesta fase — não hard-code o número, leia de `AnimalSeedData.Count`); search fulltext retorna 200 com itens; search com q vazio retorna 400; get by id inexistente retorna 404.
**DoD:** `dotnet test` (toda a solution) verde.

### [ ] T7.6 — Doc `api-endpoints.md`
**Dep:** T7.2.
**Arquivo:** `docs/reference/api-endpoints.md`.
**DoD:** todos os endpoints documentados (params, exemplos de resposta).

---

# FASE 8 — Frontend Next.js

> Pasta: `frontend/`.
>
> 🎓 **Conceito:** (Aprender Next.js/TS — aqui o foco é integrar com a API .NET). *TanStack Query*
> cuida de fetch, cache, loading e refetch da busca. O *toggle* de modo (fulltext/semantic/hybrid) é só
> um parâmetro na query. *MSW* intercepta as chamadas HTTP nos testes para você não depender do backend
> real. *Playwright* valida o fluxo de ponta a ponta no navegador. Lembre do **CORS**: o backend precisa
> liberar `http://localhost:3000` (ver T8.0).

### [ ] T8.0 — Habilitar CORS no backend
**Objetivo:** permitir que o frontend (`http://localhost:3000`) chame a API.
**Dep:** T7.2.
**Arquivo:** `src/Buscador.Api/Program.cs`.
**Passos:** registrar uma política CORS nomeada (ex.: `"frontend"`) liberando origem
`http://localhost:3000`, qualquer header e método; aplicar com `app.UseCors("frontend")`.
A origem permitida deve vir de configuração (`appsettings.json`), não hard-coded.
**DoD:** `GET /api/animals` chamado do navegador em `localhost:3000` não dá erro de CORS.

### [ ] T8.1 — Configurar TanStack Query
**Dep:** T0.5, T8.0.
**Passos:** `npm i @tanstack/react-query`; criar provider em `src/app/providers.tsx` e envolver o layout. Definir `NEXT_PUBLIC_API_URL` em `.env.local`.
**DoD:** `npm run build` ok.

### [ ] T8.2 — Tipos e serviço de API
**Dep:** T8.1, T7.2.
**Arquivos:** `src/types/animal.ts`, `src/services/animals.ts`.
**Passos:** tipos `Animal`, `SearchResult`, `SearchMode`; funções `searchAnimals(q, mode)`, `getAnimal(id)` usando `fetch` na API.
**DoD:** `npm run build` ok; tipos batem com os contratos da API.

### [ ] T8.3 — `SearchBar` + `SearchModeToggle`
**Dep:** T8.2.
**Arquivos:** `src/components/search/SearchBar.tsx`, `SearchModeToggle.tsx`.
**Passos:** input com debounce (~400ms); toggle entre `fulltext|semantic|hybrid`. Estado elevado para a página.
**DoD:** `npm run build` ok.

### [ ] T8.4 — `AnimalCard` + grid + `AnimalDetail`
**Dep:** T8.2.
**Arquivos:** `src/components/animals/AnimalCard.tsx`, `AnimalDetail.tsx`.
**Passos:** card exibe nome comum/científico, habitat, dieta, tags; detalhe em modal/drawer com descrição e curiosidades.
**DoD:** `npm run build` ok.

### [ ] T8.5 — Página principal de busca
**Dep:** T8.3, T8.4.
**Arquivo:** `src/app/page.tsx`.
**Passos:** integra `SearchBar` + `SearchModeToggle` + grid de `AnimalCard` usando `useQuery`. Mostra loading/empty states.
**DoD:** com backend rodando (`docker compose up`, Api e seed feitos), `npm run dev` → busca funciona no navegador nos 3 modos.

### [ ] T8.6 — Testes de componente (Jest + RTL + MSW)
**Dep:** T8.3, T8.4.
**Passos:**
1. `npm i -D jest @testing-library/react @testing-library/jest-dom jest-environment-jsdom @types/jest ts-node msw`
2. Criar `jest.config.ts`, setup, e `tests/mocks/handlers.ts` (mock de `/api/animals/search`).
3. `SearchBar.test.tsx` (input/debounce dispara busca), `AnimalCard.test.tsx` (renderiza dados).
**DoD:** `npm test` verde.

### [ ] T8.7 — Teste E2E (Playwright)
**Dep:** T8.5.
**Passos:**
1. `npm init playwright@latest` (ou `npm i -D @playwright/test; npx playwright install`).
2. `e2e/search.spec.ts`: abre a home, digita uma busca, vê resultados, troca de modo, vê resultados de novo.
3. `playwright.config.ts` aponta para `http://localhost:3000`.
**DoD:** `npx playwright test` verde (com backend + frontend no ar).

---

# FASE 9 — Verificação Final (E2E manual)

### [ ] T9.0 — Expandir seed de 10 → 50 animais
**Objetivo:** completar o catálogo agora que o pipeline está validado.
**Dep:** T8.7 (tudo funcionando com 10).
**Arquivo:** `src/Buscador.Application/Animals/Seed/AnimalSeedData.cs`.
**Passos:** adicionar mais 40 animais (total 50) mantendo o mesmo padrão de qualidade e variedade
(mamíferos, aves, répteis, peixes, anfíbios, marinhos). Após editar, resetar o banco e re-seed +
re-gerar embeddings (ver `docs/how-to/reset-database.md`).
**DoD:** `POST /api/animals/seed` insere 50; testes de integração continuam verdes.

### [ ] T9.1 — Roteiro de verificação completa
**Dep:** todas anteriores.
**Passos:** seguir os 10 passos da seção "Verificação" do [PLAN.md](PLAN.md): subir infra → migrations → seed → embeddings → testar FTS/semântica/híbrido via Scalar → `dotnet test` → frontend → `npm test` + Playwright.
**DoD:** todos os 10 passos passam; registrar qualquer débito em `docs/ai/PROJECT_RISK_REGISTER.md`.

### [ ] T9.2 — Finalizar documentação Diataxis
**Dep:** T9.1.
**Passos:** preencher os tutoriais (`01`-`04`) e how-tos restantes (`add-new-animal-species.md`, `run-integration-tests.md`, `reset-database.md`) e `docs/reference/` (`environment-variables.md`, `database-schema.md`) e `docs/explanation/architecture-decisions.md`, `why-pgvector.md`.
**DoD:** nenhum arquivo `.md` em `docs/` continua como stub "TODO".

---

## Mapa de dependências entre fases

```
F0 (ambiente) ─► F1 (domain) ─► F2 (infra/banco) ─► F3 (application)
                                                        │
                                       ┌────────────────┘
                                       ▼
                                 F4 (FTS) ─► F5 (semântica) ─► F6 (híbrida)
                                                                   │
                                                                   ▼
                                                              F7 (API) ─► F8 (frontend) ─► F9 (verificação)
```

## Checklist de "pronto para a próxima fase"

- **Fim F1:** `dotnet test tests/Buscador.Domain.Tests` verde.
- **Fim F2:** migration aplicada, fixture Testcontainers verde.
- **Fim F3:** seed insere 10 (MVP); testes de Application verdes. (Expande p/ 50 em T9.0.)
- **Fim F4:** FTS retorna resultado relevante em teste de integração.
- **Fim F5:** embeddings gerados; busca semântica retorna resultados.
- **Fim F6:** os 3 modos respondem.
- **Fim F7:** Scalar lista endpoints; testes de API verdes.
- **Fim F8:** busca funciona no navegador; `npm test` + Playwright verdes.
- **Fim F9:** roteiro de 10 passos do PLAN.md 100% ok.
