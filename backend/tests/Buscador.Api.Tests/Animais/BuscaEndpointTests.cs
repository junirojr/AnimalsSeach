using System.Net;
using Buscador.Api.Tests.Fixtures;
using FluentAssertions;

namespace Buscador.Api.Tests.Animais;

public class BuscaEndpointTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public BuscaEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Buscar_ComQueryVazia_Retorna400()
    {
        // Arrange
        await _fixture.ApplyMigrationsAsync();
        var cliente = _fixture.CreateClient();

        // Act
        var resposta = await cliente.GetAsync("/api/animais/buscar?q=&modo=Textual");

        // Assert
        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
