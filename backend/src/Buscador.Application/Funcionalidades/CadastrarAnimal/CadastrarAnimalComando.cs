using Buscador.Domain.Animais;
using MediatR;

namespace Buscador.Application.Funcionalidades.CadastrarAnimal;

public record CadastrarAnimalComando(
    string NomeComum,
    string NomeCientifico,
    string Descricao,
    string Caracteristicas,
    Dieta Dieta,
    Habitat Habitat,
    string DistribuicaoGeografica,
    StatusConservacao StatusConservacao,
    string[] Tags,
    string Curiosidades
) : IRequest<Guid>;
