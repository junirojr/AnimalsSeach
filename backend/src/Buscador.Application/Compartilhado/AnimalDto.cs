namespace Buscador.Application.Compartilhado;

public record AnimalDto(
    Guid Id,
    string NomeComum,
    string NomeCientifico,
    string Descricao,
    string Habitat,
    string Dieta,
    string StatusConservacao
);
