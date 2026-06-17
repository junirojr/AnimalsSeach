using Buscador.Application.Funcionalidades.CadastrarAnimal;
using Buscador.Domain.Animais;
using FluentAssertions;
using Moq;

namespace Buscador.Application.Tests.Funcionalidades;

public class CadastrarAnimalTests
{
    [Fact]
    public async Task Handle_ComDadosValidos_ChamaAdicionarAsyncERetornaId()
    {
        var repositorioMock = new Mock<IRepositorioAnimal>();
        repositorioMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Animal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var manipulador = new CadastrarAnimalComandoManipulador(repositorioMock.Object);
        var comando = new CadastrarAnimalComando(
            "Leao",
            "Panthera leo",
            "Felino majestoso",
            "Pelagem dourada",
            Dieta.Carnivoro,
            Habitat.Savana,
            "Africa",
            StatusConservacao.Vulneravel,
            new[] { "felino" },
            "Dorme 20 horas"
        );

        var id = await manipulador.Handle(comando, CancellationToken.None);

        id.Should().NotBeEmpty();
        repositorioMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Animal>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
