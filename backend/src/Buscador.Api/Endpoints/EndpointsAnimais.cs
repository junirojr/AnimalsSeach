using Buscador.Application.Funcionalidades.BuscarAnimais;
using Buscador.Application.Funcionalidades.GerarEmbeddings;
using Buscador.Application.Funcionalidades.ObterAnimais;
using Buscador.Application.Funcionalidades.ObterAnimalPorId;
using Buscador.Application.Funcionalidades.PopularAnimais;
using MediatR;

namespace Buscador.Api.Endpoints;

public static class EndpointsAnimais
{
    public static void MapearEndpointsAnimais(this WebApplication app)
    {
        app.MapPost("/api/animais/popular", async (ISender mediator) =>
        {
            var inseridos = await mediator.Send(new PopularAnimaisComando());
            return Results.Ok(new { inseridos });
        });

        app.MapPost("/api/animais/embeddings/gerar", async (ISender mediator) =>
        {
            var processados = await mediator.Send(new GerarEmbeddingsComando());
            return Results.Ok(new { processados });
        });

        app.MapGet("/api/animais", async (ISender mediator, int pagina = 1, int tamanho = 20) =>
            Results.Ok(await mediator.Send(new ObterAnimaisConsulta(pagina, tamanho))));

        app.MapGet("/api/animais/{id:guid}", async (ISender mediator, Guid id) =>
        {
            var animal = await mediator.Send(new ObterAnimalPorIdConsulta(id));
            return animal is null ? Results.NotFound() : Results.Ok(animal);
        });

        app.MapGet("/api/animais/buscar", async (ISender mediator, string q, ModoBusca modo = ModoBusca.Textual, int limite = 10) =>
            Results.Ok(await mediator.Send(new BuscarAnimaisConsulta(q, modo, limite))));
    }
}
