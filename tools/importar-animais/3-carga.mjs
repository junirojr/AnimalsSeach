/**
 * Etapa 3 — Carga na API
 *
 * Lê dados-validados.json e faz POST /api/animais para cada animal.
 * Ao final, dispara POST /api/animais/embeddings/gerar.
 *
 * Dataset de DEMO/DEV — não afeta DadosSementeAnimal nem os testes.
 *
 * Variável de ambiente opcional:
 *   API_URL=http://localhost:5024  (padrão)
 */

import { readFile } from 'fs/promises';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __dirname = dirname(fileURLToPath(import.meta.url));

const API_URL = process.env.API_URL ?? 'http://localhost:5024';

// ---------------------------------------------------------------------------
// Leitura
// ---------------------------------------------------------------------------

console.log('=== Carga na API ===\n');
console.log(`API: ${API_URL}\n`);

const caminhoValidados = join(__dirname, 'dados-validados.json');
let animais;

try {
  const conteudo = await readFile(caminhoValidados, 'utf-8');
  animais = JSON.parse(conteudo);
} catch (err) {
  console.error(`Erro ao ler dados-validados.json: ${err.message}`);
  console.error('Execute "npm run curadoria" primeiro.');
  process.exit(1);
}

console.log(`${animais.length} animais para carregar.\n`);

// ---------------------------------------------------------------------------
// Dedupe: pré-carrega nomes científicos já no banco (a API não tem unicidade)
// ---------------------------------------------------------------------------

const existentes = new Set();
try {
  let pagina = 1;
  const tamanho = 200;
  for (;;) {
    const resp = await fetch(`${API_URL}/api/animais?pagina=${pagina}&tamanho=${tamanho}`);
    if (!resp.ok) break;
    const lote = await resp.json();
    if (!Array.isArray(lote) || lote.length === 0) break;
    for (const a of lote) existentes.add((a.nomeCientifico ?? '').trim().toLowerCase());
    if (lote.length < tamanho) break;
    pagina++;
  }
  console.log(`${existentes.size} animais já no banco — duplicatas serão puladas.\n`);
} catch (err) {
  console.warn(`Não foi possível pré-carregar existentes (${err.message}); seguindo sem dedupe.\n`);
}

// ---------------------------------------------------------------------------
// POST /api/animais — um por vez para não saturar a API
// ---------------------------------------------------------------------------

let totalInseridos = 0;
let totalPulados   = 0;
let totalErros     = 0;

for (let i = 0; i < animais.length; i++) {
  const animal = animais[i];
  const progresso = `[${String(i + 1).padStart(String(animais.length).length)}/${animais.length}]`;

  const chave = (animal.nomeCientifico ?? '').trim().toLowerCase();
  if (chave && existentes.has(chave)) {
    totalPulados++;
    console.log(`${progresso} ${animal.nomeComum} (${animal.nomeCientifico}) → pulado (já existe)`);
    continue;
  }
  existentes.add(chave);

  try {
    const resp = await fetch(`${API_URL}/api/animais`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(animal),
    });

    const status = resp.status;
    const label  = resp.statusText || '';

    if (status === 201) {
      totalInseridos++;
      console.log(`${progresso} ${animal.nomeComum} (${animal.nomeCientifico}) → ${status} Created`);
    } else {
      totalErros++;
      let detalhe = '';
      try {
        const body = await resp.text();
        detalhe = body.slice(0, 120);
      } catch {
        // ignora erro de leitura do body
      }
      console.warn(`${progresso} ${animal.nomeComum} (${animal.nomeCientifico}) → ${status} ${label} ${detalhe}`);
    }
  } catch (err) {
    totalErros++;
    console.error(`${progresso} ${animal.nomeComum} (${animal.nomeCientifico}) → ERRO: ${err.message}`);
  }
}

// ---------------------------------------------------------------------------
// Relatório de carga
// ---------------------------------------------------------------------------

console.log('\n=== Resultado da Carga ===');
console.log(`Total tentados : ${animais.length}`);
console.log(`Inseridos (201): ${totalInseridos}`);
console.log(`Pulados (dupes): ${totalPulados}`);
console.log(`Erros          : ${totalErros}`);

// ---------------------------------------------------------------------------
// Gerar embeddings
// ---------------------------------------------------------------------------

console.log('\n=== Gerando Embeddings ===');
console.log('⚠️  Aviso: gerar embeddings de ~1000 animais no Ollama/CPU pode levar 2-4 HORAS.');
console.log('   Não interrompa o processo e não execute durante a suíte de testes.\n');
console.log('Chamando POST /api/animais/embeddings/gerar...');

try {
  const resp = await fetch(`${API_URL}/api/animais/embeddings/gerar`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
  });

  const status = resp.status;
  let body = '';
  try {
    body = await resp.text();
  } catch {
    // ignora
  }

  console.log(`→ ${status} ${resp.statusText}`);
  if (body) {
    console.log(body.slice(0, 500));
  }

  if (status >= 200 && status < 300) {
    console.log('\nEmbeddings gerados (ou geração iniciada em background).');
  } else {
    console.warn('\nAtenção: a geração de embeddings retornou um status inesperado.');
  }
} catch (err) {
  console.error(`Erro ao chamar endpoint de embeddings: ${err.message}`);
  console.error('Os animais foram inseridos, mas os embeddings precisam ser gerados manualmente.');
  console.error(`  curl -X POST ${API_URL}/api/animais/embeddings/gerar`);
}

console.log('\nCarga concluída.');
