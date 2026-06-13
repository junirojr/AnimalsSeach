namespace Buscador.Application.Funcionalidades.PopularAnimais;

internal static class DadosSementeAnimal
{
    internal static IReadOnlyList<(string Nome, string NomeCientifico, string Descricao, Habitat Habitat, Dieta Dieta, StatusConservacao Status)> Animais =>
    [
        ("Onca-pintada", "Panthera onca", "O maior felino das Americas, encontrado principalmente na Amazonia e no Pantanal.", Habitat.Floresta, Dieta.Carnivoro, StatusConservacao.Vulneravel),
        ("Arara-azul", "Anodorhynchus hyacinthinus", "A maior arara do mundo, com plumagem azul-cobalto vibrante.", Habitat.Floresta, Dieta.Herbivoro, StatusConservacao.Vulneravel),
        ("Boto-cor-de-rosa", "Inia geoffrensis", "O golfinho de agua doce mais comum da Amazonia, famoso pela coloracao rosada.", Habitat.AguaDoce, Dieta.Carnivoro, StatusConservacao.EmPerigo),
        ("Tamandua-bandeira", "Myrmecophaga tridactyla", "Mamifero de focinho longo especializado em se alimentar de formigas e cupins.", Habitat.Savana, Dieta.Carnivoro, StatusConservacao.Vulneravel),
        ("Mico-leao-dourado", "Leontopithecus rosalia", "Pequeno primata de pelagem dourada endemico da Mata Atlantica.", Habitat.Floresta, Dieta.Onivoro, StatusConservacao.EmPerigo),
        ("Peixe-boi-da-amazonia", "Trichechus inunguis", "O unico sirenio exclusivamente de agua doce, herbivoro pacifico.", Habitat.AguaDoce, Dieta.Herbivoro, StatusConservacao.Vulneravel),
        ("Lobo-guara", "Chrysocyon brachyurus", "O maior canideo da America do Sul, com pernas longas adaptadas ao Cerrado.", Habitat.Savana, Dieta.Onivoro, StatusConservacao.QuaseAmeacado),
        ("Capivara", "Hydrochoerus hydrochaeris", "O maior roedor do mundo, semi-aquatico e altamente social.", Habitat.AguaDoce, Dieta.Herbivoro, StatusConservacao.PoucoPreocupante),
        ("Tucano-toco", "Ramphastos toco", "Ave iconica com bico laranja-avermelhado imenso, simbolo da fauna brasileira.", Habitat.Floresta, Dieta.Onivoro, StatusConservacao.PoucoPreocupante),
        ("Anta", "Tapirus terrestris", "O maior mamifero terrestre da America do Sul, importante dispersor de sementes.", Habitat.Floresta, Dieta.Herbivoro, StatusConservacao.Vulneravel),
    ];
}
