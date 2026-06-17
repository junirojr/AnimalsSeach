namespace Buscador.Application.Compartilhado;

public interface IServicoPersistenciaFragmentos
{
    Task<IReadOnlyList<Guid>> ObterIdsSemFragmentosAsync(CancellationToken cancellationToken = default);
    Task InserirFragmentoAsync(Guid animalId, string texto, float[] vetor, CancellationToken cancellationToken = default);
}
