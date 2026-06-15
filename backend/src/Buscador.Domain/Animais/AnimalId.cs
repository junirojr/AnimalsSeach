using Buscador.Domain.Comum;

namespace Buscador.Domain.Animais;

public sealed class AnimalId : ObjetoDeValor
{
    public Guid Valor { get; }

    private AnimalId(Guid valor)
    {
        Valor = valor;
    }

    public static AnimalId Novo() => new(Guid.NewGuid());

    public static AnimalId De(Guid valor) => new(valor);

    protected override IEnumerable<object> ObterComponentesDeIgualdade()
    {
        yield return Valor;
    }

    public override string ToString() => Valor.ToString();
}