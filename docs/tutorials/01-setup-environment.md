# Tutorial 01 — Configurar o Ambiente

Neste tutorial, vamos colocar o projeto Deep Sparrow funcionando do zero na sua máquina. Ao final, você terá o banco de dados rodando, o modelo de linguagem carregado, a API respondendo e o frontend acessível no navegador. Vamos fazer isso passo a passo.

---

## Pré-requisitos

Antes de começar, você precisa ter instalado:

- **.NET 10 SDK** — compilador e runtime do backend C#
- **Docker Desktop** — para rodar o PostgreSQL e o Ollama em containers
- **Node.js 20 ou superior** — para o frontend Next.js

Vamos verificar cada um no terminal:

```bash
dotnet --version
# Esperado: 10.x.x

docker --version
# Esperado: Docker version 24.x ou superior

node --version
# Esperado: v20.x.x ou superior
```

Se algum desses comandos falhar, instale a ferramenta correspondente antes de continuar.

---

## Passo 1 — Subir os containers de infraestrutura

O projeto usa dois serviços via Docker:

- **postgres** — banco de dados PostgreSQL 16 com a extensão pgvector para busca vetorial
- **ollama** — servidor local de modelos de linguagem, que vai gerar os embeddings

Na raiz do projeto (onde está o arquivo `docker-compose.yml`), execute:

```bash
docker compose up -d
```

A flag `-d` faz os containers rodarem em segundo plano, liberando o terminal para os próximos passos.

---

## Passo 2 — Confirmar que os containers estão saudáveis

```bash
docker ps
```

Você deve ver duas linhas, algo assim:

```
CONTAINER ID   IMAGE                        COMMAND                  STATUS
a1b2c3d4e5f6   pgvector/pgvector:pg16       "docker-entrypoint.s…"   Up 30 seconds
f6e5d4c3b2a1   ollama/ollama                "/bin/ollama serve"      Up 30 seconds
```

Se o status de algum container for `Exiting` ou `Restarting`, verifique os logs com `docker logs postgres` ou `docker logs ollama` para identificar o problema.

---

## Passo 3 — Baixar o modelo de embeddings

O modelo `bge-m3` é o responsável por transformar texto em vetores numéricos. Ele ocupa cerca de 1 GB e precisa ser baixado uma única vez para dentro do container do Ollama:

```bash
docker exec ollama ollama pull bge-m3
```

Este comando pode demorar alguns minutos dependendo da sua conexão. Você verá uma barra de progresso durante o download. Aguarde até aparecer a mensagem confirmando que o pull foi concluído.

---

## Passo 4 — Aplicar as migrations do banco de dados

As migrations criam as tabelas, índices, funções e gatilhos necessários no PostgreSQL. Execute o comando abaixo **a partir do diretório `backend/`**:

```bash
dotnet ef database update --project src/Buscador.Infrastructure --startup-project src/Buscador.Api
```

Esse comando conecta ao PostgreSQL (na porta 5432, banco `buscador`, usuário `buscador`, senha `buscador`) e aplica todas as migrations pendentes em ordem. Ao terminar, você terá as tabelas `animais` e `fragmentos_animal` criadas, além do gatilho que mantém o índice de busca textual atualizado automaticamente.

---

## Passo 5 — Iniciar a API

Ainda no diretório `backend/`, inicie a API:

```bash
dotnet run --project src/Buscador.Api
```

Aguarde até ver no terminal uma mensagem indicando que a aplicação está ouvindo. A API ficará disponível em:

```
http://localhost:5024
```

Você pode confirmar abrindo `http://localhost:5024/scalar` no navegador para ver a documentação interativa da API.

---

## Passo 6 — Popular o banco com dados de exemplo

Com a API rodando, abra um novo terminal e execute:

```bash
curl -X POST http://localhost:5024/api/animais/popular
```

Esse endpoint executa o seed do projeto, inserindo 52 animais com dados reais: nome comum, nome científico, descrição, características, habitat, dieta, status de conservação, tags e curiosidades. A resposta vai confirmar quantos animais foram inseridos.

---

## Passo 7 — Gerar os embeddings

Agora vamos instruir o sistema a fragmentar cada animal em pedaços menores e gerar um vetor de 1024 dimensões para cada fragmento, usando o modelo `bge-m3`:

```bash
curl -X POST http://localhost:5024/api/animais/embeddings/gerar
```

Este processo pode demorar de alguns minutos a meia hora, dependendo da sua máquina, pois o Ollama processa cada animal localmente (sem internet). Você verá a resposta somente quando todos os animais sem fragmentos tiverem sido processados. A resposta indica quantos animais foram processados.

---

## Passo 8 — Iniciar o frontend

Em um terminal separado, acesse o diretório `frontend/` e execute:

```bash
npm install
npm run dev
```

O `npm install` instala as dependências na primeira vez. O `npm run dev` inicia o servidor de desenvolvimento do Next.js. Após alguns segundos, o frontend estará disponível em:

```
http://localhost:3000
```

---

## Passo 9 — Verificar que tudo funciona

Abra `http://localhost:3000` no navegador. Você verá a interface de busca do Deep Sparrow. Digite algo como `leao` ou `oceano` e pressione Enter. Os resultados devem aparecer em menos de um segundo para a busca textual.

Para testar a busca semântica diretamente na API:

```bash
curl "http://localhost:5024/api/animais/buscar?q=predador&modo=Semantica"
```

Se você receber uma lista de animais com pontuações, o ambiente está completamente configurado.

---

## O que acabou de acontecer

| Passo | O que foi configurado |
|-------|-----------------------|
| 1-2   | Dois containers Docker: o PostgreSQL 16 com pgvector (busca vetorial) e o Ollama (servidor de modelos de IA local) |
| 3     | Modelo bge-m3 baixado para o container do Ollama — multilíngue, 1024 dimensões, roda offline |
| 4     | Banco de dados criado com as tabelas `animais` e `fragmentos_animal`, índices, extensões `unaccent` e `vector`, e o gatilho que mantém o índice full-text sempre atualizado |
| 5     | API .NET 10 iniciada na porta 5024, com todos os endpoints disponíveis |
| 6     | 52 animais inseridos no banco com dados reais |
| 7     | Cada animal fragmentado em pedaços de texto, com um vetor de 1024 números por fragmento, armazenado na tabela `fragmentos_animal` — isso permite a busca semântica por significado |
| 8-9   | Frontend Next.js disponível na porta 3000, conectado à API |

A partir daqui, você tem um motor de busca híbrido funcionando completamente na sua máquina, sem depender de nenhum serviço externo pago.
