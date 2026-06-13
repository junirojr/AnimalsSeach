namespace Buscador.Application.Funcionalidades.ObterAnimais;

public sealed class ObterAnimaisConsultaManipulador : IRequestHandler<ObterAnimaisConsulta, IReadOnlyList<AnimalDto>>
{
    private readonly IRepositorioAnimal _repositorio;

    public ObterAnimaisConsultaManipulador(IRepositorioAnimal repositorio) => _repositorio = repositorio;

    public async Task<IReadOnlyList<AnimalDto>> Handle(ObterAnimaisConsulta request, CancellationToken cancellationToken)
    {
        var animais = await _repositorio.ObterPaginadoAsync(request.Pagina, request.Tamanho, cancellationToken);

        return animais.Select(a => new AnimalDto(
            a.Id.Valor,
            a.NomeComum,
            a.NomeCientifico,
            a.Descricao,
            a.Habitat.ToString(),
            a.Dieta.ToString(),
            a.StatusConservacao.ToString()
        )).ToList();
    }
}
