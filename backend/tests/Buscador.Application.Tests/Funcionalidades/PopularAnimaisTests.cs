using Buscador.Application.Funcionalidades.PopularAnimais;
using Buscador.Domain.Animais;
using FluentAssertions;
using Moq;
using Xunit;

namespace Buscador.Application.Tests.Funcionalidades;

public class PopularAnimaisTests
{
    [Fact]
    public async Task PopularAnimais_QuandoVazio_InsereTodosOsDaSemente()
    {
        var repositorioMock = new Mock<IRepositorioAnimal>();
        repositorioMock
            .Setup(r => r.ObterPaginadoAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Animal>());

        repositorioMock
            .Setup(r => r.AdicionarVariosAsync(It.IsAny<IEnumerable<Animal>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var manipulador = new PopularAnimaisComandoManipulador(repositorioMock.Object);
        var comando = new PopularAnimaisComando();

        var resultado = await manipulador.Handle(comando, CancellationToken.None);

        resultado.Should().Be(DadosSementeAnimal.Animais.Count);
        repositorioMock.Verify(
            r => r.AdicionarVariosAsync(
                It.Is<IEnumerable<Animal>>(a => a.Count() == DadosSementeAnimal.Animais.Count),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PopularAnimais_QuandoJaExistem_Idempotente()
    {
        var animalExistente = Animal.Criar(
            "Leão",
            "Panthera leo",
            "Felino",
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
            .Setup(r => r.ObterPaginadoAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Animal> { animalExistente });

        var manipulador = new PopularAnimaisComandoManipulador(repositorioMock.Object);
        var comando = new PopularAnimaisComando();

        var resultado = await manipulador.Handle(comando, CancellationToken.None);

        resultado.Should().Be(0);
        repositorioMock.Verify(
            r => r.AdicionarVariosAsync(It.IsAny<IEnumerable<Animal>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
