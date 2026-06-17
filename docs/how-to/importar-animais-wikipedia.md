# Como importar animais da Wikipedia (dataset de demo)

## Contexto

Este how-to descreve como popular o banco local do Animalsearch com um dataset maior de animais obtido da **Wikipedia PT** e **Wikidata**, usando os scripts em `tools/importar-animais/`.

> **DEMO/DEV apenas.** Os dados importados por estes scripts **não entram** em `DadosSementeAnimal` (o seed controlado dos testes de integração). A suíte de testes **não roda** com esses dados e não é afetada por esta importação.

Use este fluxo quando quiser:
- Explorar o motor de busca com um catálogo realista de centenas de animais.
- Avaliar o ranking de busca híbrida (FTS + semântico) em escala.
- Demonstrar a aplicação para stakeholders sem os poucos animais do seed de testes.

---

## Pré-requisitos

- [ ] **API rodando** em `http://localhost:5024`
  ```bash
  cd backend
  dotnet run --project src/Buscador.Api
  ```
- [ ] **Docker com Ollama + bge-m3** em execução
  ```bash
  docker compose up -d
  docker exec ollama ollama pull bge-m3
  ```
- [ ] **Node.js 18+** instalado (recomendado: Node 24)
  ```bash
  node --version  # deve ser >= 18
  ```

---

## Passo a passo

```bash
# Entre no diretório dos scripts
cd tools/importar-animais

# Etapa 1 — Crawl (~2-5 min)
# Consulta Wikidata SPARQL e busca extracts da Wikipedia PT.
# Gera: dados-crawl.json
npm run crawl

# Etapa 2 — Curadoria (~instantâneo)
# Valida enums de dieta/habitat por heurística de texto,
# verifica nome binomial, remove duplicatas.
# Gera: dados-validados.json + rejeitados.json
npm run curadoria

# Etapa 3 — Carga (~5 min para os POSTs + horas para embeddings)
# Envia cada animal via POST /api/animais e
# dispara POST /api/animais/embeddings/gerar ao final.
npm run carga
```

Para usar uma URL de API diferente da padrão:

```bash
API_URL=http://localhost:7080 npm run carga
```

---

## Inspecionar resultados

Após o crawl e a curadoria, inspecione os arquivos gerados:

```bash
# Ver quantos animais foram validados
node -e "const d = JSON.parse(require('fs').readFileSync('dados-validados.json','utf8')); console.log(d.length + ' validados')"

# Ver os primeiros 3 validados
node -e "const d = JSON.parse(require('fs').readFileSync('dados-validados.json','utf8')); console.log(JSON.stringify(d.slice(0,3), null, 2))"

# Ver os 5 primeiros rejeitados e seus motivos
node -e "const d = JSON.parse(require('fs').readFileSync('rejeitados.json','utf8')); console.log(JSON.stringify(d.slice(0,5).map(r=>({nome:r.nomeCientifico,motivo:r.motivoRejeicao})), null, 2))"
```

Você também pode abrir os arquivos `.json` diretamente em qualquer editor.

---

## ⚠️ Embeddings

> **Gerar embeddings de ~1 000 animais no Ollama rodando na CPU pode levar 2–4 HORAS.**
>
> - Execute apenas em desenvolvimento.
> - **Não execute durante a suíte de testes** — o processo consome CPU intensamente e pode causar timeouts nas queries do Postgres.
> - Se quiser testar apenas a busca full-text (FTS), comente a chamada ao endpoint de embeddings no final de `3-carga.mjs` e execute a carga normalmente.

Se a geração de embeddings for interrompida, reexecute apenas esse passo:

```bash
curl -X POST http://localhost:5024/api/animais/embeddings/gerar
```

---

## Idempotência

A carga respeita erros por item: se a API retornar 400 ou 409 para um animal, o script registra o erro, pula para o próximo e **não aborta o lote**.

Para re-importar do zero (limpar todos os dados de demo):

```sql
-- Execute no banco local (psql ou DBeaver)
DELETE FROM animal_fragmentos;
DELETE FROM animais;
```

> **Cuidado:** esses comandos removem **todos** os animais do banco, inclusive os do seed de testes (`DadosSementeAnimal`).
> Após a limpeza, o seed é restaurado automaticamente ao rodar as migrations de novo:
>
> ```bash
> cd backend
> dotnet ef database update --project src/Buscador.Infrastructure --startup-project src/Buscador.Api
> ```
>
> Depois do seed restaurado, rode `npm run carga` novamente para recarregar os dados de demo.
