using Buscador.Application.Funcionalidades.ObterAnimais;
using Buscador.Domain.Animais;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;

namespace Buscador.Application.Tests.Funcionalidades;

public class ObterAnimaisTests
{
    [Fact]
    public async Task ObterAnimais_ComPaginaInvalida_FalhaNaValidacao()
    {
        var validador = new ObterAnimaisConsultaValidador();
        var consulta = new ObterAnimaisConsulta(Pagina: 0, Tamanho: 10);

        var resultado = await validador.ValidateAsync(consulta);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().HaveCount(1);
        resultado.Errors.First().PropertyName.Should().Be("Pagina");
    }

    [Fact]
    public async Task ObterAnimais_ComTamanhoPequeno_FalhaNaValidacao()
    {
        var validador = new ObterAnimaisConsultaValidador();
        var consulta = new ObterAnimaisConsulta(Pagina: 1, Tamanho: 0);

        var resultado = await validador.ValidateAsync(consulta);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Tamanho");
    }

    [Fact]
    public async Task ObterAnimais_ComTamanhoGrande_FalhaNaValidacao()
    {
        var validador = new ObterAnimaisConsultaValidador();
        var consulta = new ObterAnimaisConsulta(Pagina: 1, Tamanho: 101);

        var resultado = await validador.ValidateAsync(consulta);

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Tamanho");
    }

    [Fact]
    public async Task ObterAnimais_ComParametrosValidos_Sucesso()
    {
        var animal = Animal.Criar(
            "Lobo",
            "Canis lupus",
            "Canídeo selvagem",
            "Pelagem cinzenta",
            Dieta.Carnivoro,
            Habitat.Floresta,
            "Hemisfério Norte",
            StatusConservacao.PoucoPreocupante,
            new[] { "canídeo" },
            "Caça em matilhas"
        );

        var repositorioMock = new Mock<IRepositorioAnimal>();
        repositorioMock
            .Setup(r => r.ObterPaginadoAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Animal> { animal });

        var manipulador = new ObterAnimaisConsultaManipulador(repositorioMock.Object);
        var consulta = new ObterAnimaisConsulta(Pagina: 1, Tamanho: 10);

        var resultado = await manipulador.Handle(consulta, CancellationToken.None);

        resultado.Should().HaveCount(1);
        resultado.First().NomeComum.Should().Be("Lobo");
    }
}
