using Buscador.Application.Compartilhado;
using Buscador.Domain.Animais;
using MediatR;

namespace Buscador.Application.Funcionalidades.ObterAnimalPorId;

public sealed class ObterAnimalPorIdConsultaManipulador : IRequestHandler<ObterAnimalPorIdConsulta, AnimalDto?>
{
    private readonly IRepositorioAnimal _repositorio;

    public ObterAnimalPorIdConsultaManipulador(IRepositorioAnimal repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<AnimalDto?> Handle(ObterAnimalPorIdConsulta request, CancellationToken cancellationToken)
    {
        var animal = await _repositorio.ObterPorIdAsync(AnimalId.De(request.Id), cancellationToken);
        return animal?.ParaDto();
    }
}
