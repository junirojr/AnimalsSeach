using Buscador.Domain.Animais;
using FluentAssertions;

namespace Buscador.Domain.Tests.Animais;

public class AnimalTests
{
    [Fact]
    public void Criar_ComDadosValidos_RetornaAnimal()
    {
        var animal = Animal.Criar(
            nomeComum: "Leao",
            nomeCientifico: "Panthera leo",
            descricao: "Grande felino da savana africana.",
            caracteristicas: "Juba, garras retrateis, visao noturna.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Savana,
            distribuicaoGeografica: "Africa subsaariana.",
            statusConservacao: StatusConservacao.Vulneravel,
            tags: ["felino", "predador"],
            curiosidades: "O rugido pode ser ouvido a 8 km de distancia.");

        animal.Should().NotBeNull();
        animal.NomeComum.Should().Be("Leao");
        animal.NomeCientifico.Should().Be("Panthera leo");
        animal.Dieta.Should().Be(Dieta.Carnivoro);
        animal.Habitat.Should().Be(Habitat.Savana);
        animal.StatusConservacao.Should().Be(StatusConservacao.Vulneravel);
        animal.Id.Should().NotBeNull();
    }

    [Fact]
    public void Criar_ComNomeComumVazio_LancaArgumentException()
    {
        var acao = () => Animal.Criar(
            nomeComum: "",
            nomeCientifico: "Panthera leo",
            descricao: "Grande felino da savana africana.",
            caracteristicas: "Juba, garras retrateis.",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Savana,
            distribuicaoGeografica: "Africa subsaariana.",
            statusConservacao: StatusConservacao.Vulneravel,
            tags: [],
            curiosidades: "Curiosidade qualquer.");

        acao.Should().Throw<ArgumentException>()
            .WithParameterName("nomeComum");
    }
}
