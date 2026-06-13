namespace Buscador.Application.Funcionalidades.GerarEmbeddings;

public sealed class GerarEmbeddingsComandoManipulador : IRequestHandler<GerarEmbeddingsComando, int>
{
    private readonly IServicoPersistenciaEmbedding _servicoPersistencia;

    public GerarEmbeddingsComandoManipulador(IServicoPersistenciaEmbedding servicoPersistencia) =>
        _servicoPersistencia = servicoPersistencia;

    public Task<int> Handle(GerarEmbeddingsComando request, CancellationToken cancellationToken) =>
        _servicoPersistencia.GerarESalvarParaTodosAnimaisAsync(cancellationToken);
}
