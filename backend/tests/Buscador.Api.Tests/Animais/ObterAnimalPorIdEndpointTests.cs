using System.Net;
using Buscador.Api.Tests.Fixtures;
using FluentAssertions;

namespace Buscador.Api.Tests.Animais;

public class ObterAnimalPorIdEndpointTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public ObterAnimalPorIdEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ObterPorId_ComIdInexistente_Retorna404()
    {
        // Arrange
        await _fixture.ApplyMigrationsAsync();
        var cliente = _fixture.CreateClient();

        // Act
        var resposta = await cliente.GetAsync($"/api/animais/{Guid.NewGuid()}");

        // Assert
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
