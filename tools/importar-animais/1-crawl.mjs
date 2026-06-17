/**
 * Etapa 1 — Crawl Wikipedia/Wikidata
 *
 * Busca até 1200 táxons com status de conservação IUCN no Wikidata,
 * enriquece com o parágrafo introdutório da Wikipedia PT e salva
 * em dados-crawl.json.
 *
 * Dataset de DEMO/DEV — não afeta DadosSementeAnimal nem os testes.
 */

import { writeFile } from 'fs/promises';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __dirname = dirname(fileURLToPath(import.meta.url));

// ---------------------------------------------------------------------------
// Mapeamento: URI do Wikidata → código IUCN
// ---------------------------------------------------------------------------
const MAPA_STATUS_ID = {
  'http://www.wikidata.org/entity/Q211005':  'LC',
  'http://www.wikidata.org/entity/Q719675':  'NT',
  'http://www.wikidata.org/entity/Q278113':  'VU',
  'http://www.wikidata.org/entity/Q11394':   'EN',
  'http://www.wikidata.org/entity/Q219127':  'CR',
  'http://www.wikidata.org/entity/Q23058426':'EW',
  'http://www.wikidata.org/entity/Q237350':  'EX',
};

// ---------------------------------------------------------------------------
// Query SPARQL — Wikidata
// ---------------------------------------------------------------------------
const SPARQL = `
SELECT DISTINCT ?item ?itemLabel ?nomeCientifico ?statusConservacao ?artigo
WHERE {
  VALUES ?statusConservacao {
    wd:Q211005 wd:Q719675 wd:Q278113 wd:Q11394 wd:Q219127 wd:Q23058426 wd:Q237350
  }
  ?item wdt:P31 wd:Q16521;
        wdt:P141 ?statusConservacao;
        wdt:P225 ?nomeCientifico.
  OPTIONAL {
    ?artigo schema:about ?item;
            schema:inLanguage "pt";
            schema:isPartOf <https://pt.wikipedia.org/>.
  }
  SERVICE wikibase:label { bd:serviceParam wikibase:language "pt,en". }
}
LIMIT 1200
`.trim();

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * Extrai o título do artigo a partir da URL da Wikipedia PT.
 * Ex.: "https://pt.wikipedia.org/wiki/Panthera_leo" → "Panthera_leo"
 */
function extrairTitulo(url) {
  if (!url) return null;
  const match = url.match(/\/wiki\/(.+)$/);
  return match ? decodeURIComponent(match[1]) : null;
}

/**
 * Busca extracts de até 20 títulos via Wikipedia API.
 * Retorna um objeto { titulo: extract }.
 */
async function buscarExtracts(titulos) {
  const joined = titulos.map(encodeURIComponent).join('|');
  const url =
    `https://pt.wikipedia.org/w/api.php` +
    `?action=query&prop=extracts&exintro=true&explaintext=true` +
    `&format=json&titles=${joined}`;

  try {
    const resp = await fetch(url, {
      headers: {
        'User-Agent': 'Animalsearch-Importador/1.0 (manoel.junior@bnpsolucoes.com.br)',
      },
    });

    if (!resp.ok) {
      console.warn(`  [Wikipedia] HTTP ${resp.status} para lote de ${titulos.length} títulos`);
      return {};
    }

    const data = await resp.json();
    const pages = data?.query?.pages ?? {};
    const resultado = {};

    for (const page of Object.values(pages)) {
      if (page.title && page.extract) {
        resultado[page.title] = page.extract.trim();
      }
    }

    return resultado;
  } catch (err) {
    console.warn(`  [Wikipedia] Erro ao buscar lote: ${err.message}`);
    return {};
  }
}

// ---------------------------------------------------------------------------
// Etapa 1 — Wikidata SPARQL
// ---------------------------------------------------------------------------

console.log('=== Crawl Wikipedia/Wikidata ===\n');
console.log('1/3 Consultando Wikidata SPARQL...');

const sparqlUrl =
  'https://query.wikidata.org/sparql?query=' +
  encodeURIComponent(SPARQL) +
  '&format=json';

let bindings;
try {
  const resp = await fetch(sparqlUrl, {
    headers: {
      Accept: 'application/sparql-results+json',
      'User-Agent': 'Animalsearch-Importador/1.0 (manoel.junior@bnpsolucoes.com.br)',
    },
  });

  if (!resp.ok) {
    throw new Error(`HTTP ${resp.status}: ${resp.statusText}`);
  }

  const data = await resp.json();
  bindings = data?.results?.bindings ?? [];
} catch (err) {
  console.error(`Erro ao consultar Wikidata: ${err.message}`);
  process.exit(1);
}

console.log(`   → ${bindings.length} resultados do Wikidata\n`);

// ---------------------------------------------------------------------------
// Etapa 2 — Enriquecer com Wikipedia PT
// ---------------------------------------------------------------------------

// Separar itens com e sem artigo PT
const comArtigo    = [];
const semArtigo    = [];

for (const b of bindings) {
  const tituloRaw = b.artigo?.value ?? null;
  const titulo    = extrairTitulo(tituloRaw);

  if (titulo) {
    comArtigo.push({ binding: b, titulo });
  } else {
    semArtigo.push({ binding: b, titulo: null });
  }
}

console.log(`2/3 Buscando extracts da Wikipedia PT (${comArtigo.length} artigos em lotes de 20)...`);

// Buscar em lotes de 20
const TAMANHO_LOTE = 20;
const extractMap = {};

for (let i = 0; i < comArtigo.length; i += TAMANHO_LOTE) {
  const lote  = comArtigo.slice(i, i + TAMANHO_LOTE);
  const titulos = lote.map((x) => x.titulo);

  const loteNum = Math.floor(i / TAMANHO_LOTE) + 1;
  const totalLotes = Math.ceil(comArtigo.length / TAMANHO_LOTE);
  process.stdout.write(`   Lote ${loteNum}/${totalLotes}...\r`);

  const resultado = await buscarExtracts(titulos);
  Object.assign(extractMap, resultado);

  if (i + TAMANHO_LOTE < comArtigo.length) {
    await sleep(500);
  }
}

console.log(`   → Extracts obtidos: ${Object.keys(extractMap).length}           `);

// ---------------------------------------------------------------------------
// Etapa 3 — Montar objetos finais
// ---------------------------------------------------------------------------

console.log('\n3/3 Montando dataset...');

const animais = [];

for (const { binding, titulo } of [...comArtigo, ...semArtigo]) {
  const wikidataId      = binding.item?.value?.replace('http://www.wikidata.org/entity/', '') ?? '';
  const nomeComum       = binding.itemLabel?.value ?? '';
  const nomeCientifico  = binding.nomeCientifico?.value ?? '';
  const statusUri       = binding.statusConservacao?.value ?? '';
  const statusIucn      = MAPA_STATUS_ID[statusUri] ?? '';

  // Primeiro parágrafo do artigo PT (ou string vazia)
  let descricao = '';
  if (titulo && extractMap[titulo]) {
    // Pega apenas o primeiro parágrafo não-vazio
    const paragrafos = extractMap[titulo]
      .split('\n')
      .map((p) => p.trim())
      .filter((p) => p.length > 0);
    descricao = paragrafos[0] ?? '';
  }

  animais.push({
    wikidataId,
    nomeComum,
    nomeCientifico,
    statusIucn,
    descricao,
    tags: [],
  });
}

// ---------------------------------------------------------------------------
// Salvar
// ---------------------------------------------------------------------------

const caminho = join(__dirname, 'dados-crawl.json');
await writeFile(caminho, JSON.stringify(animais, null, 2), 'utf-8');

// ---------------------------------------------------------------------------
// Relatório final
// ---------------------------------------------------------------------------
const totalComExtract = animais.filter((a) => a.descricao.length > 0).length;

console.log('\n=== Resultado do Crawl ===');
console.log(`Total buscados no Wikidata : ${bindings.length}`);
console.log(`Total com artigo PT        : ${comArtigo.length}`);
console.log(`Total sem artigo PT        : ${semArtigo.length}`);
console.log(`Total com extract obtido   : ${totalComExtract}`);
console.log(`Total salvo                : ${animais.length}`);
console.log(`\nArquivo gerado: ${caminho}`);
