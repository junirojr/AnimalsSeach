using Buscador.Application.Compartilhado;
using Buscador.Application.Funcionalidades.ObterAnimalPorId;
using Buscador.Domain.Animais;
using FluentAssertions;
using Moq;
using Xunit;

namespace Buscador.Application.Tests.Funcionalidades;

public class ObterAnimalPorIdTests
{
    [Fact]
    public async Task ObterAnimalPorId_QuandoExiste_RetornaDto()
    {
        var animalId = AnimalId.De(Guid.NewGuid());
        var animal = Animal.Criar(
            "Leão",
            "Panthera leo",
            "Felino majestoso",
            "Pelagem dourada",
            Dieta.Carnivoro,
            Habitat.Savana,
            "África",
            StatusConservacao.Vulneravel,
            new[] { "felino" },
            "Dorme 20 horas"
        );

        var repositorioMock = new Mock<IRepositorioAnimal>();
        repositorioMock
            .Setup(r => r.ObterPorIdAsync(animalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var manipulador = new ObterAnimalPorIdConsultaManipulador(repositorioMock.Object);
        var consulta = new ObterAnimalPorIdConsulta(animalId.Valor);

        var resultado = await manipulador.Handle(consulta, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.NomeComum.Should().Be("Leão");
        resultado.NomeCientifico.Should().Be("Panthera leo");
    }

    [Fact]
    public async Task ObterAnimalPorId_QuandoNaoExiste_RetornaNull()
    {
        var animalId = AnimalId.De(Guid.NewGuid());

        var repositorioMock = new Mock<IRepositorioAnimal>();
        repositorioMock
            .Setup(r => r.ObterPorIdAsync(animalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal?)null);

        var manipulador = new ObterAnimalPorIdConsultaManipulador(repositorioMock.Object);
        var consulta = new ObterAnimalPorIdConsulta(animalId.Valor);

        var resultado = await manipulador.Handle(consulta, CancellationToken.None);

        resultado.Should().BeNull();
    }
}
