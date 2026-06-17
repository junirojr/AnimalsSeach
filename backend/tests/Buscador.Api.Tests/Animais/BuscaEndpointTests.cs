using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    [Fact]
    public async Task Buscar_Textual_ComResultado_Retorna200ComItens()
    {
        // Arrange
        await _fixture.ApplyMigrationsAsync();
        var cliente = _fixture.CreateClient();
        await cliente.PostAsync("/api/animais/popular", null);

        // Act
        var resposta = await cliente.GetAsync("/api/animais/buscar?q=leao&modo=Textual");

        // Assert
        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        json.GetArrayLength().Should().BeGreaterThan(0, "busca por 'leao' deve retornar resultados com unaccent");
    }
}
