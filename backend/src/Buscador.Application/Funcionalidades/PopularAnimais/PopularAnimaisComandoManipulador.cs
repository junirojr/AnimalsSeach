namespace Buscador.Application.Funcionalidades.PopularAnimais;

public sealed class PopularAnimaisComandoManipulador : IRequestHandler<PopularAnimaisComando, int>
{
    private readonly IRepositorioAnimal _repositorio;

    public PopularAnimaisComandoManipulador(IRepositorioAnimal repositorio) => _repositorio = repositorio;

    public async Task<int> Handle(PopularAnimaisComando request, CancellationToken cancellationToken)
    {
        var animais = DadosSementeAnimal.Animais
            .Select(a => Animal.Criar(a.Nome, a.NomeCientifico, a.Descricao, a.Habitat, a.Dieta, a.Status))
            .ToList();

        await _repositorio.AdicionarVariosAsync(animais, cancellationToken);
        return animais.Count;
    }
}
