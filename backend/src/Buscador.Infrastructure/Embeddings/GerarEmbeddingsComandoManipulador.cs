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
        var idsSemFragmentos = await _contexto.Database
            .SqlQuery<IdSemFragmentos>(
                $"""
                SELECT a.id AS "Id" FROM animais a
                WHERE NOT EXISTS (
                    SELECT 1 FROM fragmentos_animal f WHERE f.animal_id = a.id
                )
                """)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var total = 0;

        for (var i = 0; i < idsSemFragmentos.Count; i += TamanhoLote)
        {
            var lote = idsSemFragmentos.Skip(i).Take(TamanhoLote).ToList();

            foreach (var id in lote)
            {
                var animal = await _repositorio.ObterPorIdAsync(
                    AnimalId.De(id), cancellationToken);

                if (animal is null)
                    continue;

                var fragmentos = FragmentadorAnimal.Fragmentar(animal);
                if (fragmentos.Count == 0)
                    continue;

                // Lote: todos os fragmentos do animal numa unica chamada ao Ollama.
                var vetores = await _servicoEmbedding.GerarVariosAsync(
                    fragmentos, TipoTextoEmbedding.Documento, cancellationToken);

                for (var j = 0; j < fragmentos.Count; j++)
                {
                    var vetorString = "[" + string.Join(",",
                        vetores[j].Select(f => f.ToString("G", CultureInfo.InvariantCulture))) + "]";

                    await _contexto.Database.ExecuteSqlRawAsync(
                        "INSERT INTO fragmentos_animal (id, animal_id, texto, embedding) VALUES ({0}, {1}, {2}, {3}::vector)",
                        Guid.NewGuid(), id, fragmentos[j], vetorString);
                }

                total++;
            }
        }

        return total;
    }

    private record IdSemFragmentos(Guid Id);
}
