using Buscador.Domain.Animais;
using MediatR;

namespace Buscador.Application.Funcionalidades.PopularAnimais;

public sealed class PopularAnimaisComandoManipulador : IRequestHandler<PopularAnimaisComando, int>
{
    private readonly IRepositorioAnimal _repositorio;

    public PopularAnimaisComandoManipulador(IRepositorioAnimal repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<int> Handle(PopularAnimaisComando request, CancellationToken cancellationToken)
    {
        var animaisExistentes = await _repositorio.ObterPaginadoAsync(1, 1, cancellationToken);

        if (animaisExistentes.Count > 0)
            return 0;

        await _repositorio.AdicionarVariosAsync(DadosSementeAnimal.Animais, cancellationToken);

        return DadosSementeAnimal.Animais.Count;
    }
}
