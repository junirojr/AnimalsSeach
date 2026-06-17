using Buscador.Application.Compartilhado;
using Buscador.Domain.Animais;
using MediatR;

namespace Buscador.Application.Funcionalidades.GerarEmbeddings;

public sealed class GerarEmbeddingsComandoManipulador
    : IRequestHandler<GerarEmbeddingsComando, int>
{
    private readonly IRepositorioAnimal _repositorio;
    private readonly IServicoEmbedding _servicoEmbedding;
    private readonly IServicoPersistenciaFragmentos _persistencia;

    public GerarEmbeddingsComandoManipulador(
        IRepositorioAnimal repositorio,
        IServicoEmbedding servicoEmbedding,
        IServicoPersistenciaFragmentos persistencia)
    {
        _repositorio = repositorio;
        _servicoEmbedding = servicoEmbedding;
        _persistencia = persistencia;
    }

    public async Task<int> Handle(
        GerarEmbeddingsComando request,
        CancellationToken cancellationToken)
    {
        var ids = await _persistencia.ObterIdsSemFragmentosAsync(cancellationToken);

        var total = 0;

        foreach (var id in ids)
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
                fragmentos, cancellationToken);

            for (var j = 0; j < fragmentos.Count; j++)
            {
                await _persistencia.InserirFragmentoAsync(
                    id, fragmentos[j], vetores[j], cancellationToken);
            }

            total++;
        }

        return total;
    }
}
