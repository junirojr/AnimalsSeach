using Buscador.Domain.Comum;

namespace Buscador.Domain.Animais;

public sealed class Animal : RaizAgregada<AnimalId>
{
    public string NomeComum { get; private set; } = string.Empty;
    public string NomeCientifico { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Caracteristicas { get; private set; } = string.Empty;
    public Dieta Dieta { get; private set; }
    public Habitat Habitat { get; private set; }
    public string DistribuicaoGeografica { get; private set; } = string.Empty;
    public StatusConservacao StatusConservacao { get; private set; }
    public string[] Tags { get; private set; } = [];
    public string Curiosidades { get; private set; } = string.Empty;

    private Animal() { }

    private Animal(AnimalId id) : base(id) { }

    public static Animal Criar(
        string nomeComum,
        string nomeCientifico,
        string descricao,
        string caracteristicas,
        Dieta dieta,
        Habitat habitat,
        string distribuicaoGeografica,
        StatusConservacao statusConservacao,
        string[] tags,
        string curiosidades)
    {
        if (string.IsNullOrWhiteSpace(nomeComum))
            throw new ArgumentException("Nome comum não pode ser vazio.", nameof(nomeComum));

        if (string.IsNullOrWhiteSpace(nomeCientifico))
            throw new ArgumentException("Nome científico não pode ser vazio.", nameof(nomeCientifico));

        return new Animal(AnimalId.Novo())
        {
            NomeComum = nomeComum,
            NomeCientifico = nomeCientifico,
            Descricao = descricao,
            Caracteristicas = caracteristicas,
            Dieta = dieta,
            Habitat = habitat,
            DistribuicaoGeografica = distribuicaoGeografica,
            StatusConservacao = statusConservacao,
            Tags = tags,
            Curiosidades = curiosidades
        };
    }
}
