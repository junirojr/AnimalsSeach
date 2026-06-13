namespace Buscador.Application.Funcionalidades.ObterAnimalPorId;

public sealed class ObterAnimalPorIdConsultaManipulador : IRequestHandler<ObterAnimalPorIdConsulta, AnimalDto?>
{
    private readonly IRepositorioAnimal _repositorio;

    public ObterAnimalPorIdConsultaManipulador(IRepositorioAnimal repositorio) => _repositorio = repositorio;

    public async Task<AnimalDto?> Handle(ObterAnimalPorIdConsulta request, CancellationToken cancellationToken)
    {
        var animal = await _repositorio.ObterPorIdAsync(AnimalId.De(request.Id), cancellationToken);
        return animal is null ? null : ParaDto(animal);
    }

    private static AnimalDto ParaDto(Animal animal) => new(
        animal.Id.Valor,
        animal.NomeComum,
        animal.NomeCientifico,
        animal.Descricao,
        animal.Habitat.ToString(),
        animal.Dieta.ToString(),
        animal.StatusConservacao.ToString()
    );
}
