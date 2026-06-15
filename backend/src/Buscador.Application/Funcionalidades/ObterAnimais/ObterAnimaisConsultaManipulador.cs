using Buscador.Application.Compartilhado;
using Buscador.Domain.Animais;
using MediatR;

namespace Buscador.Application.Funcionalidades.ObterAnimais;

public sealed class ObterAnimaisConsultaManipulador : IRequestHandler<ObterAnimaisConsulta, IReadOnlyList<AnimalDto>>
{
    private readonly IRepositorioAnimal _repositorio;

    public ObterAnimaisConsultaManipulador(IRepositorioAnimal repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IReadOnlyList<AnimalDto>> Handle(ObterAnimaisConsulta request, CancellationToken cancellationToken)
    {
        var animais = await _repositorio.ObterPaginadoAsync(request.Pagina, request.Tamanho, cancellationToken);
        return animais.Select(a => a.ParaDto()).ToList();
    }
}
