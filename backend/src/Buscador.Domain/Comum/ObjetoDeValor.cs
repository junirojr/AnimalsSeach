namespace Buscador.Domain.Comum;

public abstract class ObjetoDeValor
{
    protected abstract IEnumerable<object> ObterComponentesDeIgualdade();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        var outro = (ObjetoDeValor)obj;
        return ObterComponentesDeIgualdade().SequenceEqual(outro.ObterComponentesDeIgualdade());
    }

    public override int GetHashCode() =>
        ObterComponentesDeIgualdade()
            .Aggregate(0, (hash, obj) => HashCode.Combine(hash, obj));

    public static bool operator ==(ObjetoDeValor? esquerda, ObjetoDeValor? direita) =>
        esquerda is null ? direita is null : esquerda.Equals(direita);

    public static bool operator !=(ObjetoDeValor? esquerda, ObjetoDeValor? direita) =>
        !(esquerda == direita);
}