using Buscador.Api.Tests.Fixtures;
using Buscador.Application.Compartilhado;
using Buscador.Application.Funcionalidades.PopularAnimais;
using Buscador.Domain.Animais;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Buscador.Api.Tests.Busca;

public class ServicoBuscaTextualTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public ServicoBuscaTextualTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task BuscarAsync_ComPalavraChaveUnica_RetornaAnimalRelevante()
    {
        // Arrange
        await _fixture.ApplyMigrationsAsync();

        using var scope = _fixture.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IRepositorioAnimal>();
        var servicoBusca = scope.ServiceProvider.GetRequiredService<IServicoBuscaTextual>();

        await repositorio.AdicionarVariosAsync(DadosSementeAnimal.Animais, CancellationToken.None);

        // Act
        // "juba" aparece exclusivamente na descricao/caracteristicas/curiosidades do Leao
        var resultados = await servicoBusca.BuscarAsync("juba", 10, CancellationToken.None);

        // Assert
        resultados.Should().NotBeEmpty("a busca por 'juba' deve retornar ao menos um resultado");
        resultados[0].Animal.NomeComum.Should().Be("Leão",
            "o Leao deve ser o resultado mais relevante para a palavra-chave 'juba'");
    }
}
