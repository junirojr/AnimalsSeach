namespace Buscador.Domain.Animais;

public sealed class Animal : RaizAgregada<AnimalId>
{
    public string NomeComum { get; private set; }
    public string NomeCientifico { get; private set; }
    public string Descricao { get; private set; }
    public Habitat Habitat { get; private set; }
    public Dieta Dieta { get; private set; }
    public StatusConservacao StatusConservacao { get; private set; }

    private Animal(
        AnimalId id,
        string nomeComum,
        string nomeCientifico,
        string descricao,
        Habitat habitat,
        Dieta dieta,
        StatusConservacao statusConservacao) : base(id)
    {
        NomeComum = nomeComum;
        NomeCientifico = nomeCientifico;
        Descricao = descricao;
        Habitat = habitat;
        Dieta = dieta;
        StatusConservacao = statusConservacao;
    }

    public static Animal Criar(
        string nomeComum,
        string nomeCientifico,
        string descricao,
        Habitat habitat,
        Dieta dieta,
        StatusConservacao statusConservacao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nomeComum);
        ArgumentException.ThrowIfNullOrWhiteSpace(nomeCientifico);
        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);

        return new Animal(AnimalId.Novo(), nomeComum, nomeCientifico, descricao, habitat, dieta, statusConservacao);
    }
}
