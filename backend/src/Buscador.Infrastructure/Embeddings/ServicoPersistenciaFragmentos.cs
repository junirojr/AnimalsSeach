using Buscador.Application.Compartilhado;
using Buscador.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Buscador.Infrastructure.Embeddings;

public sealed class ServicoPersistenciaFragmentos : IServicoPersistenciaFragmentos
{
    private readonly ContextoBanco _contexto;

    public ServicoPersistenciaFragmentos(ContextoBanco contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Guid>> ObterIdsSemFragmentosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _contexto.Database
            .SqlQuery<IdSemFragmentos>(
                $"""
                SELECT a.id AS "Id" FROM animais a
                WHERE NOT EXISTS (
                    SELECT 1 FROM fragmentos_animal f WHERE f.animal_id = a.id
                )
                """)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task InserirFragmentoAsync(
        Guid animalId,
        string texto,
        float[] vetor,
        CancellationToken cancellationToken = default)
    {
        var vetorString = "[" + string.Join(",",
            vetor.Select(f => f.ToString("G", CultureInfo.InvariantCulture))) + "]";

        await _contexto.Database.ExecuteSqlRawAsync(
            "INSERT INTO fragmentos_animal (id, animal_id, texto, embedding) VALUES ({0}, {1}, {2}, {3}::vector)",
            Guid.NewGuid(), animalId, texto, vetorString);
    }

    private record IdSemFragmentos(Guid Id);
}
