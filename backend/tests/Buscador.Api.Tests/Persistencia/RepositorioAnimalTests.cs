using Buscador.Api.Tests.Fixtures;
using Buscador.Domain.Animais;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Buscador.Api.Tests.Persistencia;

public class RepositorioAnimalTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public RepositorioAnimalTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AdicionarEObterPorId_Persiste()
    {
        // Arrange
        await _fixture.ApplyMigrationsAsync();

        using var scope = _fixture.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IRepositorioAnimal>();

        var animal = Animal.Criar(
            nomeComum: "Leão",
            nomeCientifico: "Panthera leo",
            descricao: "Grande felino carnívoro",
            caracteristicas: "Mane prominent, golden coat",
            dieta: Dieta.Carnivoro,
            habitat: Habitat.Savana,
            distribuicaoGeografica: "Africa",
            statusConservacao: StatusConservacao.Vulneravel,
            tags: new[] { "feline", "predator" },
            curiosidades: "King of the jungle");

        // Act
        await repositorio.AdicionarAsync(animal);
        var resultado = await repositorio.ObterPorIdAsync(animal.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(animal.Id);
        resultado.NomeComum.Should().Be("Leão");
        resultado.Dieta.Should().Be(Dieta.Carnivoro);
    }
}
