namespace Buscador.Domain.Comum;

public abstract class ObjetoDeValor
{
    protected abstract IEnumerable<object> ObterComponentesDeIgualdade();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return ((ObjetoDeValor)obj)
            .ObterComponentesDeIgualdade()
            .SequenceEqual(ObterComponentesDeIgualdade());
    }

    public override int GetHashCode()
    {
        return ObterComponentesDeIgualdade()
            .Aggregate(0, HashCode.Combine);
    }

    public static bool operator ==(ObjetoDeValor? a, ObjetoDeValor? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ObjetoDeValor? a, ObjetoDeValor? b) => !(a == b);
}
