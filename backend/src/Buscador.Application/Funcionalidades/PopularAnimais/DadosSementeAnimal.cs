using Buscador.Domain.Animais;

namespace Buscador.Application.Funcionalidades.PopularAnimais;

public static class DadosSementeAnimal
{
    public static readonly IReadOnlyList<Animal> Animais = new[]
    {
        Animal.Criar(
            nomeComum: "Leão",
            nomeCientifico: "Panthera leo",
            descricao: "O leão é o felino mais social e vive em grupos chamados alcateias. Mamífero carnívoro de grande porte, domina a paisagem africana com seu porte majestoso. Famoso pelo rugido poderoso que pode ser ouvido a quilômetros de distância.",
            caracteristicas: "Pelagem dourada ou marrom-clara, os machos possuem uma juba volumosa ao redor da cabeça e pescoço. Corpo musculoso e compacto, cabeça grande com mandíbulas poderosas. Olhos amarelos brilhantes e dentes caninos até 7 cm.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Savana,
            distribuicaoGeografica: "Encontrado principalmente na África subsaariana, com pequena população na Índia. Concentra-se em savanas, pradarias e florestas abertas.",
            statusConservacao: StatusConservacao.Vulneravel,
            tags: new[] { "felino", "predador", "mamífero", "social", "majestoso" },
            curiosidades: "Os leões dormem até 20 horas por dia. Apenas o leão possui uma juba, que serve para atrair fêmeas e intimidar rivais. As leonas fazem a maior parte da caça enquanto os machos defendem o território."
        ),
        Animal.Criar(
            nomeComum: "Lobo",
            nomeCientifico: "Canis lupus",
            descricao: "O lobo é um canídeo selvagem inteligente e altamente social que caça em matilhas coordenadas. Ancestral direto do cão doméstico, o lobo é um predador de topo em seus habitats. Símbolo de liberdade e selvageria nas culturas humanas.",
            caracteristicas: "Pelagem espessa variando de cinza, marrom e preto, adaptada ao clima frio. Corpo robusto e patas longas para corridas de longa distância, podem atingir velocidades de 40 km/h. Focinho proeminente e orelhas eretas.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Floresta,
            distribuicaoGeografica: "Distribuído pelo hemisfério norte, principalmente na América do Norte, Europa e Ásia. Habita florestas boreais, tundras e montanhas.",
            statusConservacao: StatusConservacao.PoucoPreocupante,
            tags: new[] { "canídeo", "predador", "social", "inteligente", "selvagem" },
            curiosidades: "Os lobos caçam em matilhas lideradas por um casal alfa. Possuem um sistema de comunicação sofisticado baseado em uivos, rosnados e linguagem corporal. Podem viajar até 40 km em um dia caçando."
        ),
        Animal.Criar(
            nomeComum: "Golfinho",
            nomeCientifico: "Tursiops truncatus",
            descricao: "O golfinho-nariz-de-garrafa é um cetáceo altamente inteligente conhecido pela sua brincadeira e curiosidade. Mamífero aquático que vive em grupos sociais complexos chamados pods. Reconhecido por sua capacidade de eciolocalização sofisticada.",
            caracteristicas: "Corpo aerodinâmico cinza carvão dorsalmente e mais claro ventralmente. Cabeça arredondada com um bico curto e robusto distintivo. Barbatanas dorsal e laterais bem desenvolvidas, cauda bifurcada.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Oceano,
            distribuicaoGeografica: "Encontrado em águas temperadas e tropicais dos oceanos Atlântico, Pacífico e Índico. Habita principalmente águas costeiras e baías.",
            statusConservacao: StatusConservacao.PoucoPreocupante,
            tags: new[] { "cetáceo", "inteligente", "aquático", "social", "brincalhão" },
            curiosidades: "Golfinhos conseguem reconhecer a si mesmos em espelhos, demonstrando autoconsciência. Usam eciolocalização emitindo cliques que ricocheteiam nos objetos para navegar e caçar. Frequentemente resgatam humanos e ajudam-se mutuamente."
        ),
        Animal.Criar(
            nomeComum: "Águia-real",
            nomeCientifico: "Aquila chrysaetos",
            descricao: "A águia-real é uma ave de rapina magnífica com visão extremamente aguçada e garras poderosas. Predador de topo aéreo capaz de detectar presas a quilômetros de distância. Símbolo de poder e liberdade em muitas culturas.",
            caracteristicas: "Plumagem marrom-escura com manchas douradas na cabeça e nuca, daí o nome 'chrysaetos'. Asas longas e largas, corpo musculoso e garras letais de até 5 cm. Bico curvado e poderoso, olhos amarelos penetrantes.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Montanha,
            distribuicaoGeografica: "Distribuída pelo hemisfério norte através de montanhas e regiões áridas da América do Norte, Europa e Ásia. Habita picos de montanhas e escarpas rochosas.",
            statusConservacao: StatusConservacao.PoucoPreocupante,
            tags: new[] { "ave", "predador", "voo", "visão-aguçada", "majestosa" },
            curiosidades: "Águias-reais podem voar a velocidades de até 200 km/h em mergulho. Seus olhos são 4 a 8 vezes mais aguçados que os humanos. Podem carregar presas pesando até 4 kg com suas garras poderosas."
        ),
        Animal.Criar(
            nomeComum: "Cobra-real",
            nomeCientifico: "Python regius",
            descricao: "A cobra-real é uma serpente constritora dócil originária da África Ocidental. Popular em cativeiro, ganhou seu nome pela crença de que seria a cobra do rei. Não é venenosa, mata suas presas por constrição.",
            caracteristicas: "Corpo robusto variando em cores de marrom, dourado, amarelo e preto. Cabeça triangular pequena com boca extensível, corpo cilíndrico com padrões pretos e brancos. Atinge até 1,5 metros de comprimento.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Floresta,
            distribuicaoGeografica: "Originária da África Ocidental, principalmente em regiões de savana e floresta. Encontrada em países como Gana, Benin, Togo e Nigéria.",
            statusConservacao: StatusConservacao.PoucoPreocupante,
            tags: new[] { "réptil", "constrictora", "cobra", "dócil", "exótica" },
            curiosidades: "Cobras-reais podem permanecer semanas sem comer. Matam suas presas envolvendo-as e sufocando-as lentamente. Enrolam-se em uma bola defensiva quando ameaçadas, daí o nome 'regius'."
        ),
        Animal.Criar(
            nomeComum: "Sapo-comum",
            nomeCientifico: "Rana temporaria",
            descricao: "O sapo-comum é um anfíbio europeu que passa parte de sua vida em água e parte em terra seca. Fundamental para o controle de insetos e importante indicador da saúde ambiental. Metamorfose impressionante de girino para sapo adulto.",
            caracteristicas: "Pele áspera castanho-avermelhada a cinzenta com padrão reticulado. Corpo robusto, olhos salientes com pupila horizontal, tímpano visível atrás dos olhos. Patas traseiras longas adaptadas para saltos, patas dianteiras curtas.",
            dieta: Dieta.Onivoro,
            habitat: Habitat.AguaDoce,
            distribuicaoGeografica: "Nativo da Europa, encontrado da Grã-Bretanha até o Cáucaso. Habita lagoas, brejos, zonas húmidas e florestas úmidas.",
            statusConservacao: StatusConservacao.PoucoPreocupante,
            tags: new[] { "anfíbio", "insectívoro", "aquático", "metamorfose", "verde" },
            curiosidades: "Girinos respiram através de brânquias enquanto sapos adultos respiram pulmões e pele. Durante amplexo, múltiplos machos podem agrupando-se em volta de uma fêmea. Suas peles secretam compostos antimicrobianos."
        ),
        Animal.Criar(
            nomeComum: "Tubarão-branco",
            nomeCientifico: "Carcharodon carcharias",
            descricao: "O tubarão-branco é o maior peixe predador do mundo e um dos mais antigos predadores de topo marinhos. Máquina de caça perfeita com sentidos refinados e velocidade impressionante. Símbolo da força selvagem e do mistério oceânico.",
            caracteristicas: "Corpo hidrodinâmico cinza-azulado dorsalmente e branco ventralmente, com mancha preta sob a nadadeira peitoral. Boca enorme com até 3000 dentes triangulares serrilhados dispostos em fileiras. Nadadeira dorsal triangular proeminente.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Oceano,
            distribuicaoGeografica: "Encontrado em todos os oceanos, principalmente em águas temperadas. Habita águas costeiras e profundas onde há foca e leão-marinho.",
            statusConservacao: StatusConservacao.Vulneravel,
            tags: new[] { "tubarão", "predador-topo", "peixe", "marinho", "formidável" },
            curiosidades: "Tubarões-brancos podem atingir velocidades de 56 km/h em investidas. Seus dentes são constantemente regenerados ao longo da vida. Podem detectar uma gota de sangue em uma piscina olímpica."
        ),
        Animal.Criar(
            nomeComum: "Urso-polar",
            nomeCientifico: "Ursus maritimus",
            descricao: "O urso-polar é o mamífero carnívoro terrestre maior e está perfeitamente adaptado ao ambiente ártico extremo. Excelente nadador com metabolismo único para sobreviver em temperaturas abaixo de -40°C. Enfrentando ameaça de aquecimento global.",
            caracteristicas: "Pelagem branca espessa com subpelo denso para isolamento térmico. Corpo robusto e alongado com patas largas e garras curvas para aderência no gelo. Nariz e lábios pretos, olhos pequenos, orelhas arredondadas.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Polar,
            distribuicaoGeografica: "Circumpolar, encontrado em redor do Ártico, principalmente em regiões com gelo marinho. Distribui-se pelo Alasca, Canadá, Rússia, Groenlândia e Noruega.",
            statusConservacao: StatusConservacao.Vulneravel,
            tags: new[] { "urso", "ártico", "natação", "térmico", "majestoso" },
            curiosidades: "Ursos-polares podem nadar até 100 km em busca de alimento. Seu pelo branco não é realmente branco mas transparente e reflete luz. Caçam focas através de buracos no gelo, esperando pacientemente por horas."
        ),
        Animal.Criar(
            nomeComum: "Elefante-africano",
            nomeCientifico: "Loxodonta africana",
            descricao: "O elefante africano é o maior mamífero terrestre, conhecido pela inteligência excepcional e estrutura social complexa. Herbívoro gentil apesar de seu tamanho impressionante, essencial para manter ecossistemas de savana. Memória proverbial e capacidade de relacionamentos profundos.",
            caracteristicas: "Corpo imenso pesando até 6 toneladas, coberto por pele cinzenta espessa. Orelhas largas como leques para dissipar calor, tromba flexível com 40000 músculos. Presas marfim alongadas, patas colunares que suportam o peso.",
            dieta: Dieta.Herbivoro,
            habitat: Habitat.Savana,
            distribuicaoGeografica: "Nativo da África subsaariana, encontrado em savanas, florestas abertas e semi-áridas. Presente em países como Botsuana, Quênia, Tanzânia e Zimbábue.",
            statusConservacao: StatusConservacao.Vulneravel,
            tags: new[] { "mamífero", "herbívoro", "inteligente", "social", "gigante" },
            curiosidades: "Elefantes vivem em grupos matriarcais liderados pela fêmea mais velha. Podem processar luto e mostram comportamento de empatia com congêneres mortos. Sua tromba contém receptores sensoriais sofisticados e força excepcional."
        ),
        Animal.Criar(
            nomeComum: "Papagaio-militar",
            nomeCientifico: "Ara militaris",
            descricao: "O papagaio-militar é uma arara verde de médio porte encontrada nos Andes. Intelecto excepcional para uma ave, com capacidade de resolver problemas e aprender vocabulário. Ligação social forte com seus parceiros ao longo da vida.",
            caracteristicas: "Plumagem predominantemente verde com manchas vermelhas na testa e asas. Cabeça grande com olho branco penetrante, bico poderoso preto curvado. Cauda longa e rígida, patas cinzentas zigodáctilas para agarrar.",
            dieta: Dieta.Onivoro,
            habitat: Habitat.Floresta,
            distribuicaoGeografica: "Encontrado nos Andes da América do Sul, principalmente na Bolívia, Peru, Equador e Colômbia. Habita florestas de montanha entre 1500 e 3000 metros de altitude.",
            statusConservacao: StatusConservacao.PoucoPreocupante,
            tags: new[] { "ave", "papagaio", "inteligente", "colorido", "social" },
            curiosidades: "Papagaios-militares podem viver mais de 50 anos. Possuem uma das maiores proporções de cérebro para corpo entre aves. Podem aprender a imitar sons e comunicar-se através de vocalizações complexas."
        )
    };
}