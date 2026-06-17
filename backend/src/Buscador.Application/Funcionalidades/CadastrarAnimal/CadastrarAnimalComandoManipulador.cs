using Buscador.Domain.Animais;
using MediatR;

namespace Buscador.Application.Funcionalidades.CadastrarAnimal;

public sealed class CadastrarAnimalComandoManipulador : IRequestHandler<CadastrarAnimalComando, Guid>
{
    private readonly IRepositorioAnimal _repositorio;

    public CadastrarAnimalComandoManipulador(IRepositorioAnimal repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Guid> Handle(CadastrarAnimalComando request, CancellationToken cancellationToken)
    {
        var animal = Animal.Criar(
            request.NomeComum,
            request.NomeCientifico,
            request.Descricao,
            request.Caracteristicas,
            request.Dieta,
            request.Habitat,
            request.DistribuicaoGeografica,
            request.StatusConservacao,
            request.Tags,
            request.Curiosidades
        );

        await _repositorio.AdicionarAsync(animal, cancellationToken);

        return animal.Id.Valor;
    }
}
