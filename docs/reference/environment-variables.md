# Variaveis de Ambiente

Referencia completa de todas as variaveis de configuracao do projeto Deep Sparrow.

---

## Backend (.NET — `appsettings.json` / variaveis de ambiente do SO)

O backend le configuracoes via `IConfiguration` do ASP.NET Core, que funde em ordem de prioridade:
`appsettings.json` -> `appsettings.{Ambiente}.json` -> variaveis de ambiente do SO -> secrets do usuario.

| Variavel | Descricao | Valor padrao de dev | Onde e lida | Obrigatoria |
|---|---|---|---|---|
| `ConnectionStrings:Postgres` | String de conexao ADO.NET para o PostgreSQL. Inclui host, porta, banco, usuario e senha. | `Host=localhost;Port=5432;Database=buscador;Username=buscador;Password=buscador` | `InjecaoDependencia.cs` via `configuracao.GetConnectionString("Postgres")` | Sim |
| `Ollama:BaseUrl` | URL base da API REST do Ollama. O container Docker expoe a porta `11434`. | `http://localhost:11434` | `ServicoEmbeddingOllama.cs` via `configuracao["Ollama:BaseUrl"]` | Sim |
| `Cors:AllowedOrigin` | Origem HTTP permitida na politica de CORS. Deve corresponder exatamente a origem do frontend (protocolo + host + porta). | `http://localhost:3000` | `Program.cs` via `builder.Configuration["Cors:AllowedOrigin"]` | Nao (tem fallback) |

### Detalhes

#### `ConnectionStrings:Postgres`

```json
"ConnectionStrings": {
  "Postgres": "Host=localhost;Port=5432;Database=buscador;Username=buscador;Password=buscador"
}
```

Em producao, substitua por uma string que aponte para o servidor real. Nunca commite credenciais reais no repositorio.
O formato segue a convencao Npgsql (driver .NET para PostgreSQL).

#### `Ollama:BaseUrl`

```json
"Ollama": {
  "BaseUrl": "http://localhost:11434"
}
```

Deve ser acessivel pelo processo do backend. Em Docker Compose, use o nome do servico como host (ex.: `http://ollama:11434`).
O modelo `bge-m3` precisa estar baixado no Ollama antes de gerar embeddings:

```bash
docker exec ollama ollama pull bge-m3
```

#### `Cors:AllowedOrigin`

```json
"Cors": {
  "AllowedOrigin": "http://localhost:3000"
}
```

Em producao, substitua pelo dominio publico do frontend. A politica permite qualquer metodo e qualquer cabecalho da origem configurada.

---

## Frontend (Next.js — `.env.local`)

O frontend le variaveis de ambiente via mecanismo nativo do Next.js.
Variaveis com prefixo `NEXT_PUBLIC_` sao embutidas no bundle do cliente (browser); as demais ficam apenas no servidor Node.js.

| Variavel | Descricao | Valor padrao de dev | Onde e usada | Obrigatoria |
|---|---|---|---|---|
| `NEXT_PUBLIC_API_URL` | URL base da API .NET. Usada pelo cliente para montar as URLs das requisicoes HTTP. | `http://localhost:5024` | Arquivo `.env.local`; consumida via `process.env.NEXT_PUBLIC_API_URL` | Sim |

### Detalhes

#### `NEXT_PUBLIC_API_URL`

```ini
NEXT_PUBLIC_API_URL=http://localhost:5024
```

Aponta para a porta que o ASP.NET Core escuta em desenvolvimento (configurada em `launchSettings.json`).
Em producao, deve ser a URL publica da API (ex.: `https://api.buscador.example.com`).

Crie o arquivo `frontend/.env.local` a partir do exemplo:

```bash
cp frontend/.env.example frontend/.env.local
# edite conforme o ambiente
```

---

## Resumo rapido — portas locais

| Servico | Porta padrao |
|---|---|
| PostgreSQL (Docker) | `5432` |
| Ollama (Docker) | `11434` |
| Backend ASP.NET Core | `5024` |
| Frontend Next.js | `3000` |
