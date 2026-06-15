using Buscador.Domain.Animais;

namespace Buscador.Application.Compartilhado;

public record AnimalDto(
    Guid Id,
    string NomeComum,
    string NomeCientifico,
    string Descricao,
    string Caracteristicas,
    Dieta Dieta,
    Habitat Habitat,
    string DistribuicaoGeografica,
    StatusConservacao StatusConservacao,
    string[] Tags,
    string Curiosidades);

public static class AnimalExtensoes
{
    public static AnimalDto ParaDto(this Animal animal)
    {
        return new AnimalDto(
            animal.Id.Valor,
            animal.NomeComum,
            animal.NomeCientifico,
            animal.Descricao,
            animal.Caracteristicas,
            animal.Dieta,
            animal.Habitat,
            animal.DistribuicaoGeografica,
            animal.StatusConservacao,
            animal.Tags,
            animal.Curiosidades);
    }
}
