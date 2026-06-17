# Importador de Animais da Wikipedia (Demo/Dev)

Conjunto de scripts Node.js para buscar dados de animais na Wikipedia/Wikidata e carregá-los na API local do Animalsearch.

## ⚠️ AVISO

Este é um dataset exclusivo de **DEMO/DEV**.

- Os dados importados por estes scripts **NÃO entram** em `DadosSementeAnimal` (o seed oficial de testes).
- A suíte de integração **NÃO roda** com esses dados — ela usa apenas o seed controlado em `DadosSementeAnimal`.
- Use estes scripts apenas para popular o banco local com um volume maior de animais e experimentar o motor de busca.

## O que faz

O importador funciona em 3 etapas independentes:

### Etapa 1 — Crawl (`1-crawl.mjs`)

- Consulta o **Wikidata SPARQL** para obter até 1 200 táxons com status de conservação IUCN e nome científico.
- Para cada táxon que possui artigo em **Wikipedia PT**, busca o parágrafo introdutório via Wikipedia API (em lotes de 20, com pausa de 500 ms entre lotes).
- Salva o resultado em **`dados-crawl.json`**.

### Etapa 2 — Curadoria (`2-curadoria.mjs`)

- Lê `dados-crawl.json`.
- Valida nome científico binomial, descrição, enums de dieta e habitat (inferidos por heurística de texto).
- Deduplicа por nome científico.
- Salva **`dados-validados.json`** (prontos para carga) e **`rejeitados.json`** (com motivo de rejeição).

### Etapa 3 — Carga (`3-carga.mjs`)

- Lê `dados-validados.json` e faz `POST /api/animais` para cada animal.
- Ao final, chama `POST /api/animais/embeddings/gerar` para gerar os embeddings semânticos.

## Pré-requisitos

- **Node.js 18+** (recomendado: Node 24 com ES Modules e fetch global)
- **API rodando** em `http://localhost:5024` (`cd backend && dotnet run`)
- **Docker com Ollama + bge-m3** em execução (`docker compose up -d`)

## Como usar

```bash
cd tools/importar-animais

npm run crawl      # Etapa 1: busca Wikipedia/Wikidata (~2-5 min)
npm run curadoria  # Etapa 2: valida os dados (~instantâneo)
npm run carga      # Etapa 3: envia para a API (~5 min + embeddings)
```

Para apontar para outra URL de API:

```bash
API_URL=http://localhost:7080 npm run carga
```

## ⚠️ Tempo de embeddings

Gerar embeddings de ~1 000 animais no **Ollama rodando na CPU** pode levar **2–4 horas**.

- Execute a etapa de carga apenas em ambiente de desenvolvimento.
- **Não execute durante a suíte de testes** — o processo consome CPU intensamente e pode causar timeouts.
- Se quiser apenas testar o motor de busca sem embeddings semânticos, comente a chamada `POST /api/animais/embeddings/gerar` no final de `3-carga.mjs`.

## Idempotência

A carga respeita erros por item: se um animal já existir (a API retornar 409 ou 400), o script pula e continua. Isso torna re-execuções seguras.

Para rodar completamente do zero (limpar todos os dados de demo antes de re-importar), execute no banco local:

```sql
DELETE FROM animal_fragmentos;
DELETE FROM animais;
```

> **Atenção:** esses comandos removem **todos** os animais, incluindo o seed de testes se você rodou as migrations com `dotnet ef database update`. Prefira rodar o importador em um banco separado de demo ou restaurar o seed após a limpeza (`dotnet ef database update` re-executa `DadosSementeAnimal`).
