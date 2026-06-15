using Buscador.Application.Compartilhado;
using Buscador.Application.Funcionalidades.GerarEmbeddings;
using Buscador.Domain.Animais;
using Buscador.Infrastructure.Persistencia;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Buscador.Infrastructure.Embeddings;

public sealed class GerarEmbeddingsComandoManipulador
    : IRequestHandler<GerarEmbeddingsComando, int>
{
    private readonly ContextoBanco _contexto;
    private readonly IRepositorioAnimal _repositorio;
    private readonly IServicoEmbedding _servicoEmbedding;
    private const int TamanhoLote = 5;

    public GerarEmbeddingsComandoManipulador(
        ContextoBanco contexto,
        IRepositorioAnimal repositorio,
        IServicoEmbedding servicoEmbedding)
    {
        _contexto = contexto;
        _repositorio = repositorio;
        _servicoEmbedding = servicoEmbedding;
    }

    public async Task<int> Handle(
        GerarEmbeddingsComando request,
        CancellationToken cancellationToken)
    {
        var idsSemEmbedding = await _contexto.Database
            .SqlQuery<IdSemEmbedding>(
                $"""
                SELECT a.id AS "Id" FROM animais a WHERE a.embedding IS NULL
                """)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var total = 0;

        for (var i = 0; i < idsSemEmbedding.Count; i += TamanhoLote)
        {
            var lote = idsSemEmbedding.Skip(i).Take(TamanhoLote).ToList();

            foreach (var id in lote)
            {
                var animal = await _repositorio.ObterPorIdAsync(
                    AnimalId.De(id), cancellationToken);

                if (animal is null)
                    continue;

                var texto = $"{animal.Descricao} {animal.Caracteristicas} {animal.Curiosidades}";
                var vetor = await _servicoEmbedding.GerarAsync(texto, cancellationToken);

                var vetorString = "[" + string.Join(",",
                    vetor.Select(f => f.ToString("G", CultureInfo.InvariantCulture))) + "]";

                await _contexto.Database.ExecuteSqlRawAsync(
                    "UPDATE animais SET embedding = {0}::vector WHERE id = {1}",
                    vetorString, id);

                total++;
            }
        }

        return total;
    }

    private record IdSemEmbedding(Guid Id);
}
