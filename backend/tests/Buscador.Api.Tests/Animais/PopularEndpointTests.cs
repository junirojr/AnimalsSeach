using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Buscador.Api.Tests.Fixtures;
using Buscador.Application.Funcionalidades.PopularAnimais;
using FluentAssertions;

namespace Buscador.Api.Tests.Animais;

public class PopularEndpointTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public PopularEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Popular_QuandoChamado_RetornaContagemDaSemente()
    {
        // Arrange
        await _fixture.ApplyMigrationsAsync();
        var cliente = _fixture.CreateClient();

        // Act
        var resposta = await cliente.PostAsync("/api/animais/popular", null);

        // Assert
        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("inseridos").GetInt32().Should().Be(DadosSementeAnimal.Animais.Count);
    }
}
