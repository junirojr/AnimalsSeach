using System.Net;
using System.Net.Http.Json;
using Buscador.Api.Tests.Fixtures;
using FluentAssertions;

namespace Buscador.Api.Tests.Animais;

public class CadastrarAnimalEndpointTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public CadastrarAnimalEndpointTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CadastrarAnimal_ComPayloadValido_Retorna201ComLocation()
    {
        await _fixture.ApplyMigrationsAsync();
        var cliente = _fixture.CreateClient();

        var payload = new
        {
            nomeComum = "Tigre",
            nomeCientifico = "Panthera tigris",
            descricao = "Maior felino do mundo.",
            caracteristicas = "Listras pretas e laranja.",
            dieta = "Carnivoro",
            habitat = "Floresta",
            distribuicaoGeografica = "Asia",
            statusConservacao = "EmPerigo",
            tags = new[] { "felino", "predador" },
            curiosidades = "Excelente nadador."
        };

        var resposta = await cliente.PostAsJsonAsync("/api/animais", payload);

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        resposta.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CadastrarAnimal_ComNomeComumVazio_Retorna400()
    {
        await _fixture.ApplyMigrationsAsync();
        var cliente = _fixture.CreateClient();

        var payload = new
        {
            nomeComum = "",
            nomeCientifico = "Panthera tigris",
            descricao = "Maior felino do mundo.",
            caracteristicas = "Listras pretas e laranja.",
            dieta = "Carnivoro",
            habitat = "Floresta",
            distribuicaoGeografica = "Asia",
            statusConservacao = "EmPerigo",
            tags = new[] { "felino" },
            curiosidades = "Excelente nadador."
        };

        var resposta = await cliente.PostAsJsonAsync("/api/animais", payload);

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
