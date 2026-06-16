using Buscador.Api.Tests.Fixtures;
using Buscador.Application.Compartilhado;
using Buscador.Application.Funcionalidades.GerarEmbeddings;
using Buscador.Application.Funcionalidades.PopularAnimais;
using Buscador.Domain.Animais;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Buscador.Api.Tests.Busca;

public class ServicoBuscaSemanticaTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public ServicoBuscaSemanticaTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task BuscarAsync_ComConsultaConceitual_RetornaAnimaisRelevantes()
    {
        // Arrange
        await _fixture.ApplyMigrationsAsync();

        using var scope = _fixture.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IRepositorioAnimal>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var servicoBusca = scope.ServiceProvider.GetRequiredService<IServicoBuscaSemantica>();

        await repositorio.AdicionarVariosAsync(DadosSementeAnimal.Animais, CancellationToken.None);
        await sender.Send(new GerarEmbeddingsComando(), CancellationToken.None);

        // Act — consulta conceitual: nenhum animal tem essa frase exata na descricao
        var resultados = await servicoBusca.BuscarAsync(
            "animal que caca em bando", 5, CancellationToken.None);

        // Assert
        resultados.Should().NotBeEmpty(
            "a busca semantica deve retornar resultados para a consulta conceitual");

        resultados.Any(r => r.Animal.NomeComum == "Lobo").Should()
            .BeTrue("o Lobo e descrito como social que caca em matilhas");
    }
}
