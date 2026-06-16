using Buscador.Api.Tests.Fixtures;
using Buscador.Application.Compartilhado;
using Buscador.Application.Funcionalidades.GerarEmbeddings;
using Buscador.Application.Funcionalidades.PopularAnimais;
using Buscador.Domain.Animais;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Buscador.Api.Tests.Busca;

public class ServicoBuscaHibridaTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public ServicoBuscaHibridaTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task BuscarAsync_ComConsultaMista_RetornaAnimaisRelevantes()
    {
        // Arrange
        await _fixture.ApplyMigrationsAsync();

        using var scope = _fixture.Services.CreateScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IRepositorioAnimal>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var servicoBusca = scope.ServiceProvider.GetRequiredService<IServicoBuscaHibrida>();

        await repositorio.AdicionarVariosAsync(DadosSementeAnimal.Animais, CancellationToken.None);
        await sender.Send(new GerarEmbeddingsComando(), CancellationToken.None);

        // Act — consulta com palavra literal + conceito semantico
        var resultados = await servicoBusca.BuscarAsync(
            "predador dos oceanos", 5, CancellationToken.None);

        // Assert
        resultados.Should().NotBeEmpty(
            "a busca hibrida deve combinar FTS e semantica e retornar resultados");

        resultados.Should().BeInDescendingOrder(r => r.Pontuacao,
            "resultados devem estar ordenados por pontuacao RRF decrescente");
    }
}
