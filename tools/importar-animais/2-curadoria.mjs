/**
 * Etapa 2 — Curadoria
 *
 * Lê dados-crawl.json, valida cada entrada (nome binomial, descrição,
 * enums de dieta e habitat por heurística) e gera:
 *   dados-validados.json  — prontos para carga na API
 *   rejeitados.json       — com motivo de rejeição
 *
 * Dataset de DEMO/DEV — não afeta DadosSementeAnimal nem os testes.
 */

import { readFile, writeFile } from 'fs/promises';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __dirname = dirname(fileURLToPath(import.meta.url));

// ---------------------------------------------------------------------------
// Mapeamento IUCN → enum da API
// ---------------------------------------------------------------------------
const MAPA_STATUS = {
  LC: 'PoucoPreocupante',
  NT: 'QuaseAmeacado',
  VU: 'Vulneravel',
  EN: 'EmPerigo',
  CR: 'CriticamenteEmPerigo',
  EW: 'ExtintoNaNatureza',
  EX: 'Extinto',
};

// ---------------------------------------------------------------------------
// Heurísticas
// ---------------------------------------------------------------------------

/**
 * Infere a dieta a partir do texto da descrição.
 * Retorna 'Carnivoro' | 'Herbivoro' | 'Onivoro' | null
 */
function inferirDieta(descricao) {
  const t = descricao.toLowerCase();
  if (/carnív|carnivoro|carnivore|predador|come carne|come peixe|come inseto|insetívoro/.test(t)) return 'Carnivoro';
  if (/herbív|herbivoro|herbivore|come plantas|come folhas|come frutos|come sementes|pastador/.test(t)) return 'Herbivoro';
  if (/onívoro|onivoro|omnivore|come tudo|dieta variada/.test(t)) return 'Onivoro';
  return null;
}

/**
 * Infere o habitat a partir do texto da descrição.
 * Retorna um dos valores do enum Habitat da API, ou null.
 */
function inferirHabitat(descricao) {
  const t = descricao.toLowerCase();
  if (/oceano|marinho|mar aberto|pelágico/.test(t))                              return 'Oceano';
  if (/deserto|árido|semiárido|região seca/.test(t))                             return 'Deserto';
  if (/savana|cerrado|campos abertos|pastagens|estepe/.test(t))                  return 'Savana';
  if (/montanha|alpino|altitude|cordilheira|andino/.test(t))                     return 'Montanha';
  if (/água doce|rio|lago|fluvial|aquático de água doce|banhado/.test(t))        return 'AguaDoce';
  if (/polar|ártico|antártico|tundra/.test(t))                                   return 'Polar';
  if (/floresta|mata|selva|bosque|amazônia|atlântica/.test(t))                   return 'Floresta';
  return null;
}

// ---------------------------------------------------------------------------
// Leitura
// ---------------------------------------------------------------------------

console.log('=== Curadoria ===\n');

const caminhoEntrada = join(__dirname, 'dados-crawl.json');
let entrada;

try {
  const conteudo = await readFile(caminhoEntrada, 'utf-8');
  entrada = JSON.parse(conteudo);
} catch (err) {
  console.error(`Erro ao ler dados-crawl.json: ${err.message}`);
  console.error('Execute "npm run crawl" primeiro.');
  process.exit(1);
}

console.log(`Lidos ${entrada.length} animais de dados-crawl.json\n`);

// ---------------------------------------------------------------------------
// Validação
// ---------------------------------------------------------------------------

const validados   = [];
const rejeitados  = [];

// Contadores de motivos de rejeição
const contMotivos = {
  'sem nome binomial':     0,
  'descricao vazia':       0,
  'status invalido':       0,
  'dieta nao id':          0,
  'habitat nao id':        0,
  'duplicata':             0,
};

// Controle de deduplcação
const nomesVistos = new Set();

for (const item of entrada) {
  const { wikidataId, nomeComum, nomeCientifico, statusIucn, descricao, tags } = item;

  // Regra 1: nome científico binomial (ex: "Panthera leo")
  if (!/^[A-Z][a-z]+ [a-z]+/.test(nomeCientifico ?? '')) {
    rejeitados.push({ ...item, motivoRejeicao: 'sem nome binomial' });
    contMotivos['sem nome binomial']++;
    continue;
  }

  // Regra 2: descrição não pode ser vazia ou muito curta
  if (!descricao || descricao.length <= 20) {
    rejeitados.push({ ...item, motivoRejeicao: 'descricao vazia' });
    contMotivos['descricao vazia']++;
    continue;
  }

  // Regra 3: status de conservação deve mapear
  const statusConservacao = MAPA_STATUS[statusIucn];
  if (!statusConservacao) {
    rejeitados.push({ ...item, motivoRejeicao: 'status invalido' });
    contMotivos['status invalido']++;
    continue;
  }

  // Regra 4: dieta inferível
  const dieta = inferirDieta(descricao);
  if (!dieta) {
    rejeitados.push({ ...item, motivoRejeicao: 'dieta nao id' });
    contMotivos['dieta nao id']++;
    continue;
  }

  // Regra 5: habitat inferível
  const habitat = inferirHabitat(descricao);
  if (!habitat) {
    rejeitados.push({ ...item, motivoRejeicao: 'habitat nao id' });
    contMotivos['habitat nao id']++;
    continue;
  }

  // Regra 6: deduplcação por nome científico
  const chave = nomeCientifico.toLowerCase();
  if (nomesVistos.has(chave)) {
    rejeitados.push({ ...item, motivoRejeicao: 'duplicata' });
    contMotivos['duplicata']++;
    continue;
  }
  nomesVistos.add(chave);

  // Aprovado — montar objeto para a API
  validados.push({
    nomeComum,
    nomeCientifico,
    descricao,
    caracteristicas: '',
    dieta,
    habitat,
    distribuicaoGeografica: '',
    statusConservacao,
    tags: tags ?? [],
    curiosidades: '',
  });
}

// ---------------------------------------------------------------------------
// Salvar
// ---------------------------------------------------------------------------

const caminhoValidados  = join(__dirname, 'dados-validados.json');
const caminhoRejeitados = join(__dirname, 'rejeitados.json');

await writeFile(caminhoValidados,  JSON.stringify(validados,  null, 2), 'utf-8');
await writeFile(caminhoRejeitados, JSON.stringify(rejeitados, null, 2), 'utf-8');

// ---------------------------------------------------------------------------
// Relatório
// ---------------------------------------------------------------------------
const totalMotivos = Object.values(contMotivos).reduce((s, v) => s + v, 0);

console.log('=== Resultado da Curadoria ===');
console.log(`Total entrada     : ${entrada.length}`);
console.log(`Validos           : ${validados.length}`);
console.log(`Rejeitados        : ${rejeitados.length}`);

for (const [motivo, qtd] of Object.entries(contMotivos)) {
  if (qtd > 0) {
    console.log(`  ${motivo.padEnd(22)}: ${qtd}`);
  }
}

console.log(`\nArquivos gerados:`);
console.log(`  ${caminhoValidados}`);
console.log(`  ${caminhoRejeitados}`);
