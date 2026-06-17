# Como Resetar o Banco de Dados

O banco de dados e um PostgreSQL 16 rodando no container Docker chamado `postgres`.
Conexao: usuario `buscador`, senha `buscador`, banco `buscador`.

Escolha o cenario que corresponde ao que voce precisa fazer.

---

## Cenario A: Apagar so os dados e ressemear

Use quando quiser limpar os animais e embeddings mas manter a estrutura do banco (tabelas e migrations).

**1. Truncar os dados:**

```bash
docker exec postgres psql -U buscador -d buscador -c "TRUNCATE animais CASCADE;"
```

O `CASCADE` apaga tambem a tabela `fragmentos_animal`, que armazena os chunks de embeddings vinculados a cada animal.

**2. Subir a API:**

```bash
cd backend
dotnet run --project src/Buscador.Api
```

**3. Ressemear os animais:**

```bash
curl -X POST http://localhost:5024/api/animais/popular
```

**4. Regenerar os embeddings:**

```bash
curl -X POST http://localhost:5024/api/animais/embeddings/gerar
```

A geracao de embeddings usa o modelo `bge-m3` via Ollama e pode levar varios minutos.

---

## Cenario B: Destruir tudo e recriar do zero

Use quando quiser descartar completamente o volume do banco (dados + schema) e comecar do zero.

**1. Derrubar os containers e apagar os volumes:**

```bash
docker compose down -v
```

O flag `-v` remove os volumes Docker, apagando todos os dados persistidos.

**2. Subir a infra novamente:**

```bash
docker compose up -d
```

**3. Aplicar as migrations:**

```bash
cd backend
dotnet ef database update --no-build --project src/Buscador.Infrastructure --startup-project src/Buscador.Api
```

**4. Ressemear e regenerar embeddings** (igual ao Cenario A, passos 2 a 4):

```bash
dotnet run --project src/Buscador.Api
curl -X POST http://localhost:5024/api/animais/popular
curl -X POST http://localhost:5024/api/animais/embeddings/gerar
```

---

## Cenario C: Limpar apenas os embeddings para regenerar

Use quando os embeddings estiverem desatualizados (ex.: troca de modelo) mas os dados dos animais estiverem corretos.

**1. Apagar somente os fragmentos de embedding:**

```bash
docker exec postgres psql -U buscador -d buscador -c "TRUNCATE fragmentos_animal;"
```

Os registros da tabela `animais` sao mantidos intactos.

**2. Subir a API e regenerar:**

```bash
cd backend
dotnet run --project src/Buscador.Api
curl -X POST http://localhost:5024/api/animais/embeddings/gerar
```
