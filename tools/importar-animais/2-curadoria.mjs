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
 * Retorna true se a descrição indica que o item é uma planta (não animal).
 */
function ePlanta(descricao) {
  const t = descricao.toLowerCase();
  return /\bplanta\b|\bplantas\b|arbusto|árvore|arvore|arvoreta|liana|trepadeira|erva\b|cipó|cipo|mato\b|vegetal\b|espécie de árvore|espécie de planta|espécie de arbusto|angiosperma|gimnosperma|pteridofita|pteridófita|briofita|briófita|monocotiledônea|dicotiledônea|fabaceae|rosaceae|poaceae|asteraceae|myrtaceae|sapindaceae|moraceae|apocynaceae|euphorbiaceae|orchidaceae|arecaceae|bromeliaceae|cactaceae|bignoniaceae|rutaceae|rutáceas|ericaceae|solanaceae|lamiaceae|apiaceae|piperaceae|lauraceae|jambeiro|jambos|caducifólia|caducifolia|espécie de fungo|fungo\b|cogumelo|botânicos|género acer\b|\bácer\b|segundo alguns botân|tremoço\b|lupinus\b|allium\b/.test(t);
}

/**
 * Infere a dieta a partir do texto da descrição.
 * Retorna 'Carnivoro' | 'Herbivoro' | 'Onivoro' | null
 */
function inferirDieta(descricao) {
  const t = descricao.toLowerCase();

  // --- Carnivoro ---
  if (/carnív|carnivoro|carnivore/.test(t)) return 'Carnivoro';
  if (/predador|predadora|predadores|predatório/.test(t)) return 'Carnivoro';
  if (/insetívoro|insetivoro|insetívora|insectívoro/.test(t)) return 'Carnivoro';
  if (/piscívoro|piscivoro|piscívora/.test(t)) return 'Carnivoro';
  if (/come carne|come peixe|come inseto|come insetos|come pequenos mamiferos/.test(t)) return 'Carnivoro';
  if (/alimenta.se de inseto|alimenta.se de peixe|alimenta.se de mamifero|alimenta.se de roedor|alimenta.se de anfibio|alimenta.se de reptil|alimenta.se de ave\b|alimenta.se de aves|alimenta.se de molusco|alimenta.se de crustaceo|alimenta.se de.*animais|alimenta.se de pequenos/.test(t)) return 'Carnivoro';
  if (/dieta.*inseto|dieta.*peixe|dieta.*mamifero|dieta.*carne/.test(t)) return 'Carnivoro';
  if (/rapina|ave de rapina|ave rapina/.test(t)) return 'Carnivoro';
  if (/\blarvas\b|larvas de inseto|se alimenta de larva/.test(t)) return 'Carnivoro';
  if (/invertebrados|pequenos vertebrados|pequenos animais/.test(t)) return 'Carnivoro';
  // português europeu: insecto/insectos
  if (/insecto|insectos|insectívoro|insectivoro/.test(t)) return 'Carnivoro';
  // alimentação/alimenta-se com variações
  if (/alimentação à base de insecto|alimentacao a base de inseto|alimentação a base de inseto/.test(t)) return 'Carnivoro';
  // familias/ordens de aves de rapina e piscivoras (Latin + PT)
  if (/falconidae|falconídeos|falconideo/.test(t)) return 'Carnivoro';
  if (/accipitridae|accipitrideos|acipitrídeos/.test(t)) return 'Carnivoro';
  if (/strigidae|tytonidae|cathartidae/.test(t)) return 'Carnivoro';
  // kingfishers: piscivoros
  if (/alcedinidae|alcedinídeos|alcedinídeo|halcyoninae|halcyonidae|cerylinae/.test(t)) return 'Carnivoro';
  if (/martim-caçador|guarda-rios|martim caçador/.test(t)) return 'Carnivoro';
  // abelharucos (bee-eaters): insetos
  if (/meropidae/.test(t)) return 'Carnivoro';
  // andorinhões (swifts): insetos em voo
  if (/apodidae|hemiprocnidae/.test(t)) return 'Carnivoro';
  // noitibós (nightjars): insetos
  if (/caprimulgidae/.test(t)) return 'Carnivoro';
  // pica-paus (woodpeckers): insetos
  if (/picidae/.test(t)) return 'Carnivoro';
  // serpentes e crocodilos
  if (/viperidae|colubridae|elapidae|boidae|pythonidae/.test(t)) return 'Carnivoro';
  if (/crocodilian|crocodilo|jacaré|jacares|caimão/.test(t)) return 'Carnivoro';
  // tubarões e peixes piscivoros
  if (/\btubarão|\btubaroes|\bcação/.test(t)) return 'Carnivoro';
  // mamíferos marinhos carnivoros
  if (/\bfoca\b|\bfocas\b|leão-marinho|leao-marinho|lobo-marinho/.test(t)) return 'Carnivoro';
  if (/\borca\b|\bbaleia assassina/.test(t)) return 'Carnivoro';
  // gaviões
  if (/\bgavião\b|\bgavioes\b/.test(t)) return 'Carnivoro';
  // noitibós (bacuraus)
  if (/\bnoitibó\b|\bnoitibos\b|\bbacurau\b/.test(t)) return 'Carnivoro';
  // peixes piscivoros/carnivoros
  if (/chaetodontidae|latidae|sparidae|serranidae|lutjanidae|scaridae|acanthuridae|balistidae|molidae/.test(t)) return 'Carnivoro';
  if (/scolopacidae|maçarico|limícola/.test(t)) return 'Carnivoro';
  // serpente venenosa é sempre carnívora
  if (/serpente|cobra\b|peçonhenta|venenosa/.test(t)) return 'Carnivoro';
  // gaivotas/terns/petrels/pinguins: carnivoros marinhos — formas PT+latim
  if (/laridae|larídeos|gaivota|gaivotão|sternidae|sternídeos|hydrobatidae|procellariidae|spheniscidae/.test(t)) return 'Carnivoro';
  if (/charadriidae|alcidae|rynchopidae|recurvirostridae/.test(t)) return 'Carnivoro';
  // garças, cegonhas, mergulhões, fragatas — formas PT+latim
  if (/ardeidae|ardeídeos|gaviidae|podicipedidae|phalacrocoracidae|sulidae|fregatidae|ciconiidae|gruidae/.test(t)) return 'Carnivoro';
  // cucos: insetos — formas PT+latim
  if (/cuculidae|cuculídeos/.test(t)) return 'Carnivoro';
  // thamnophilidae: insetivoros do neotrópico
  if (/thamnophilidae/.test(t)) return 'Carnivoro';
  // muscicapidae (papa-moscas), laniidae (picanços), motacillidae (alvéolas): insetivoros
  if (/muscicapidae|muscicapídeos|laniidae|laníideos|motacillidae|motacilídeos/.test(t)) return 'Carnivoro';
  // acrocephalidae, sylviidae, regulidae: insetivoros
  if (/acrocephalidae|sylviidae|phylloscopidae|cettiidae|regulidae/.test(t)) return 'Carnivoro';
  // mustelidae: carnivoros — formas PT+latim
  if (/mustelidae|mustelideos|mustelídeos/.test(t)) return 'Carnivoro';
  // phocoenidae (polvos/golfinho): carnivoros
  if (/phocoenidae|phocidae/.test(t)) return 'Carnivoro';
  // andorinhões — formas PT
  if (/apodídeos/.test(t)) return 'Carnivoro';
  // troglodytidae (carriças): insetivoros
  if (/troglodytidae|trogloditídeos/.test(t)) return 'Carnivoro';
  // coraciidae (roleiros): insetivoros/carnivoros
  if (/coraciidae|coraciídeos/.test(t)) return 'Carnivoro';
  // bucerotidae (calaus/tucanos): formas PT
  if (/bucerotídeos|bucerotidae/.test(t)) return 'Onivoro';
  // psittaculidae (periquitos): herbivoros
  if (/psittaculidae/.test(t)) return 'Herbivoro';
  // sturnidae/esturnídeos (estorninhos): onivoros
  if (/sturnidae|esturnídeos/.test(t)) return 'Onivoro';
  // tityridae (tiritiris): onivoros
  if (/tityridae/.test(t)) return 'Onivoro';
  // canidae (raposas, coiotes): omnivoros
  if (/canidae|canídeos/.test(t)) return 'Onivoro';
  // nomes comuns sem familia
  if (/\bsagui\b/.test(t)) return 'Onivoro';
  if (/\braposa\b|\braposas\b/.test(t)) return 'Onivoro';
  if (/\bcastor\b|\bcastores\b/.test(t)) return 'Herbivoro';
  // pomba/rolinha: granivoros
  if (/\bpomba\b|\bpombas\b|\brolinha\b|\brola\b|\brolas\b/.test(t)) return 'Herbivoro';
  // garça: piscivora
  if (/\bgarça\b|\bgarças\b/.test(t)) return 'Carnivoro';
  // tetrazes: onivoros
  if (/\btetrazes\b|\btetraz\b/.test(t)) return 'Onivoro';
  // tucunaré/ciclídeo: carnivoros
  if (/tucunaré|ciclídeo|ciclideos/.test(t)) return 'Carnivoro';
  // pelecaniformes: carnivoros (garças, pelicanos)
  if (/pelecaniformes/.test(t)) return 'Carnivoro';
  // passeriformes → onivoro (já está); pá de passeriformes sem habitat → Floresta
  if (/passeriforme/.test(t)) return 'Onivoro';

  // --- Herbivoro ---
  if (/herbív|herbivoro|herbivore/.test(t)) return 'Herbivoro';
  if (/granívoro|granivoro|granívora/.test(t)) return 'Herbivoro';
  if (/frugívoro|frugívora|frugívoros|frugívoras/.test(t)) return 'Herbivoro';
  if (/nectarívoro|nectarívora|nectarívoros/.test(t)) return 'Herbivoro';
  if (/folívoro|folívora|folívoros/.test(t)) return 'Herbivoro';
  if (/pastador|pastadora|pastadores|pastagem/.test(t)) return 'Herbivoro';
  if (/come plantas|come folhas|come frutos|come sementes|come capim|come grama|come algas/.test(t)) return 'Herbivoro';
  if (/alimenta.se de plantas|alimenta.se de sementes|alimenta.se de frutos|alimenta.se de folhas|alimenta.se de néctar|alimenta.se de nectar|alimenta.se de algas|alimenta.se de grama|alimenta.se de capim/.test(t)) return 'Herbivoro';
  if (/dieta.*planta|dieta.*semente|dieta.*fruto|dieta.*folha|dieta.*vegetal|dieta.*néctar/.test(t)) return 'Herbivoro';
  // grupos taxonomicos classicamente herbivoros
  if (/cervídeo|cervideo|cervidae|veado\b|veados\b|alce\b|rena\b|bisão|bison/.test(t)) return 'Herbivoro';
  if (/bovidae|camelidae|equidae|tapiridae|rhinocerotidae/.test(t)) return 'Herbivoro';
  if (/\belefante|\belefantes|\bmamute/.test(t)) return 'Herbivoro';
  if (/\bgirafas?\b/.test(t)) return 'Herbivoro';
  if (/\bcoelho\b|\bcoelhos\b|leporidae/.test(t)) return 'Herbivoro';
  // beija-flores (hummingbirds): néctar
  if (/trochilidae|beija.flor/.test(t)) return 'Herbivoro';
  // estrildideos (finches): sementes — formas PT e latim
  if (/estrildidae|estrildídeos|estrildideo/.test(t)) return 'Herbivoro';
  // ciprinídeos herbívoros (carpa-do-limo)
  if (/ctenopharyngodon|carpa.do.limo/.test(t)) return 'Herbivoro';
  // patos/gansos: anatidae — majoritariamente herbivoros/onivoros
  if (/anatidae/.test(t)) return 'Herbivoro';
  // sandgrouse: pteroclididae — granivoros
  if (/pteroclididae|pteroclidae/.test(t)) return 'Herbivoro';
  // tentilhões: fringillidae — granivoros
  if (/fringillidae/.test(t)) return 'Herbivoro';
  // avestruzes: struthionidae — herbivoros
  if (/struthionidae/.test(t)) return 'Herbivoro';
  // pombos: columbidae — granivoros/frugívoros
  if (/columbidae|columbídeos/.test(t)) return 'Herbivoro';
  // bombicilídeos (cedar waxwing): frugívoros
  if (/bombicilídeos|bombycillidae/.test(t)) return 'Herbivoro';
  // patos formas PT
  if (/anatídeos/.test(t)) return 'Herbivoro';
  // psittacideos (parrots): sementes/frutos
  if (/psittacidae|psittaciformes/.test(t)) return 'Herbivoro';

  // --- Onivoro ---
  if (/onívoro|onivoro|omnivore|omnívoro|omnivoro|omnívora|dieta variada|dieta mista|come tudo/.test(t)) return 'Onivoro';
  if (/alimenta.se de.*tanto|alimenta.se de frutos.*inseto|alimenta.se de inseto.*fruto/.test(t)) return 'Onivoro';
  // grupos taxonomicos classicamente onivoros
  if (/suidae|tayassuidae|\bporco\b|\bporcos\b|\bpecari\b/.test(t)) return 'Onivoro';
  if (/\burso\b|\bursos\b|ursidae/.test(t)) return 'Onivoro';
  if (/corvidae|\bcorvo\b/.test(t)) return 'Onivoro';
  if (/\broedor\b|\broedores\b|muridae|cricetidae|sciuridae|cuniculidae/.test(t)) return 'Onivoro';
  if (/\bguaxinim|\braccoon|procyonidae/.test(t)) return 'Onivoro';
  if (/\bprimata\b|\bprimatas\b|\bmacaco\b|\bmacacos\b|\bchimpanze\b|\bgorila\b/.test(t)) return 'Onivoro';
  // pipridae/manakins: frugívoros
  if (/pipridae/.test(t)) return 'Herbivoro';

  // passeriformes sem familia especifica: onivoros por padrao
  if (/passeriforme|passeriformes/.test(t)) return 'Onivoro';
  // turdidae (tordo/melro): onivoros
  if (/turdidae/.test(t)) return 'Onivoro';
  // phasianidae (faisões, pavões): onivoros
  if (/phasianidae/.test(t)) return 'Onivoro';
  // rallidae (frango-d'agua, galinholas): onivoros
  if (/rallidae/.test(t)) return 'Onivoro';
  // alaudidae (cotovias): onivoros (sementes + insetos)
  if (/alaudidae/.test(t)) return 'Onivoro';
  // passeridae (pardais): onivoros
  if (/passeridae/.test(t)) return 'Onivoro';
  // phasianidae formas PT
  if (/fasianídeos/.test(t)) return 'Onivoro';
  // muridae/cricetidae formas PT
  if (/murídeos|cricetídeos/.test(t)) return 'Onivoro';
  // parídeos (chapins): onivoros
  if (/paridae|parídeos/.test(t)) return 'Onivoro';
  // prunelídeos (accentors): onivoros (insetos + sementes)
  if (/prunelídeos|prunellidae/.test(t)) return 'Onivoro';
  // larídeos formas PT — se chegou aqui sem ser carnívoro → onivoro costeiro
  if (/larídeos/.test(t)) return 'Carnivoro';

  return null;
}

/**
 * Infere o habitat a partir do texto da descrição.
 * Retorna um dos valores do enum Habitat da API, ou null.
 */
function inferirHabitat(descricao) {
  const t = descricao.toLowerCase();

  if (/oceano|marinho|mar aberto|pelágico|pelagico|oceânico|oceanico|oceânicas|oceanicas|litoral|costeiro|coralino|recife\b|recifes\b|recife de coral|estuário|estuario|plataforma continental|águas costeiras|aguas costeiras|arrecife/.test(t)) return 'Oceano';
  if (/deserto|árido|arido|semiárid|semiarid|região seca|regiao seca|estepe árida|savana árida|arid/.test(t))             return 'Deserto';
  if (/savana|cerrado|campos abertos|pastagens\b|estepe|campina\b|campos naturais|campos de altitude/.test(t))            return 'Savana';
  if (/montan|alpino|altitude|cordilheira|andino|andina|alpes|rochoso|penhascos|zona montanhosa|zonas montanhosas|região montanhosa/.test(t)) return 'Montanha';
  if (/água doce|agua doce|fluvial|aquático de água doce|banhado|pantanal|várzea|varzea|brejo|pântano|pantano|corpo.d.água|corpos d.água|corpos de água|ao longo de rios|ao longo de lagos|cursos d.água/.test(t)) return 'AguaDoce';
  if (/\brio\b|\brios\b|\blago\b|\blagos\b/.test(t)) return 'AguaDoce';
  if (/polar|ártico|artico|antártico|antartico|tundra|glacial|geleira/.test(t))                                           return 'Polar';
  if (/floresta|mata\b|matas\b|selva|bosque|amazônia|amazonia|atlântica|atlantica|tropical úmida|tropical umida|subtropical|galeria|silvestre|florestal|área florestada|floresta de madagáscar|floresta de comores/.test(t)) return 'Floresta';

  // semi-aquático → AguaDoce
  if (/semi.aquático|semi.aquatico|semiaquático|semiaquatico/.test(t)) return 'AguaDoce';
  // proxies geograficos: regioes tropicais → Floresta (aproximação razoavel para dados DEMO)
  if (/ásia tropical|asia tropical|sudeste asiático|sueste asiático|sul da ásia|subcontinente indiano|africa tropical|africa subsaariana|america do sul\b|america central\b|região tropical|regioes tropicais|trópico\b|tropicais\b|tropical\b/.test(t)) return 'Floresta';
  // "das Américas", "nativo das Américas" → Floresta (proxy amplo)
  if (/\bamericas\b|\bamérica do sul|\bamérica central|\bamérica do norte/.test(t)) return 'Floresta';

  // familias de aves cujo habitat principal e inferivel
  if (/alcedinidae|alcedinídeos|halcyoninae|cerylinae/.test(t)) return 'AguaDoce'; // martim-caçador/guarda-rios
  if (/scolopacidae|maçarico|limícola/.test(t)) return 'AguaDoce'; // limícolas
  if (/meropidae/.test(t)) return 'Savana'; // abelharucos: campos abertos
  if (/estrildidae|estrildídeos/.test(t)) return 'Savana'; // tentilhões tropicais
  if (/pteroclididae|pteroclidae/.test(t)) return 'Deserto'; // sandgrouse: regioes aridas
  if (/alaudidae/.test(t)) return 'Savana'; // cotovias: campos abertos
  if (/passeridae/.test(t)) return 'Savana'; // pardais
  if (/fringillidae/.test(t)) return 'Floresta'; // tentilhoes: florestas e campos
  // aves aquáticas: Oceano ou AguaDoce
  if (/laridae|gaivota|alcidae|rynchopidae|fregatidae|sulidae|phalacrocoracidae/.test(t)) return 'Oceano';
  if (/ardeidae|gaviidae|podicipedidae|rallidae|anatidae/.test(t)) return 'AguaDoce';
  if (/gruidae/.test(t)) return 'AguaDoce'; // garças/grous
  if (/charadriidae|scolopacidae/.test(t)) return 'AguaDoce'; // limícolas
  if (/ciconiidae/.test(t)) return 'AguaDoce'; // cegonhas
  if (/thamnophilidae/.test(t)) return 'Floresta'; // insetivoros do neotrópico
  if (/acrocephalidae|sylviidae/.test(t)) return 'AguaDoce'; // tordos de cana
  if (/columbidae|columbídeos/.test(t)) return 'Floresta'; // pombos
  if (/mustelidae|mustelideos|mustelídeos/.test(t)) return 'Floresta'; // mustelídeos: florestas/rios
  if (/muscicapidae|muscicapídeos|laniidae/.test(t)) return 'Floresta'; // papa-moscas, picanços
  if (/motacillidae|motacilídeos/.test(t)) return 'AguaDoce'; // alvéolas: à beira de rios
  if (/regulidae|paridae|parídeos|prunelídeos/.test(t)) return 'Floresta'; // tetas e ouriços: florestas
  if (/bombicilídeos|bombycillidae/.test(t)) return 'Floresta'; // cedar waxwing
  if (/laridae|larídeos|sternidae|hydrobatidae|procellariidae/.test(t)) return 'Oceano';
  if (/phocoenidae/.test(t)) return 'Oceano'; // polvos/golfinhos
  if (/cuculidae|cuculídeos/.test(t)) return 'Floresta'; // cucos
  if (/fasianídeos|phasianidae/.test(t)) return 'Floresta'; // faisões
  if (/struthionidae/.test(t)) return 'Savana'; // avestruzes: savana
  if (/murídeos|cricetídeos/.test(t)) return 'Floresta';
  if (/troglodytidae|trogloditídeos/.test(t)) return 'Floresta';
  if (/coraciidae/.test(t)) return 'Savana'; // roleiros: savana/campos abertos
  if (/bucerotídeos|bucerotidae/.test(t)) return 'Floresta';
  if (/psittaculidae/.test(t)) return 'Floresta';
  if (/sturnidae|esturnídeos/.test(t)) return 'Savana';
  if (/tityridae/.test(t)) return 'Floresta';
  if (/canidae|canídeos/.test(t)) return 'Floresta';
  if (/tundra\b/.test(t)) return 'Polar';
  // familias faltantes → habitat
  if (/accipitridae|accipitrideos|acipitrídeos/.test(t)) return 'Floresta';
  if (/corvidae/.test(t)) return 'Floresta';
  if (/turdidae/.test(t)) return 'Floresta';
  if (/falconidae|falconídeos/.test(t)) return 'Montanha';
  if (/strigidae|tytonidae/.test(t)) return 'Floresta';
  if (/tyrannidae|tiranídeos/.test(t)) return 'Floresta';
  if (/fringilídeos/.test(t)) return 'Floresta';
  if (/alaudídeos/.test(t)) return 'Savana';
  if (/emberizidae/.test(t)) return 'Savana';
  if (/phylloscopidae/.test(t)) return 'Floresta';
  if (/cuniculidae/.test(t)) return 'Floresta'; // paca
  if (/latidae/.test(t)) return 'AguaDoce'; // barramundi
  if (/apodídeos/.test(t)) return 'Floresta'; // andorinhões PT form
  if (/pelecaniformes/.test(t)) return 'AguaDoce'; // garças, pelicanos
  if (/\bgarça\b|\bgarças\b/.test(t)) return 'AguaDoce';
  if (/passeriforme|passeriformes/.test(t)) return 'Floresta'; // passeriformes genéricos → Floresta
  if (/apodidae|hemiprocnidae/.test(t)) return 'Floresta'; // andorinhões: florestas tropicais
  if (/caprimulgidae/.test(t)) return 'Floresta'; // noitibós
  if (/pipridae/.test(t)) return 'Floresta'; // tangarás/manakins
  if (/trochilidae/.test(t)) return 'Floresta'; // beija-flores
  if (/picidae/.test(t)) return 'Floresta'; // pica-paus

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
  'nao e animal':          0,
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

  // Regra 2b: não é animal (é planta, fungo, etc.)
  if (ePlanta(descricao)) {
    rejeitados.push({ ...item, motivoRejeicao: 'nao e animal' });
    contMotivos['nao e animal']++;
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
