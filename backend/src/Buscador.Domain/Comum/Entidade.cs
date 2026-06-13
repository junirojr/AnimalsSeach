namespace Buscador.Domain.Comum;

public abstract class Entidade<TId>
{
    public TId Id { get; protected set; }

    protected Entidade(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entidade<TId> outro) return false;
        if (ReferenceEquals(this, outro)) return true;
        if (GetType() != outro.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, outro.Id);
    }

    public override int GetHashCode() =>
        EqualityComparer<TId>.Default.GetHashCode(Id!);

    public static bool operator ==(Entidade<TId>? esquerda, Entidade<TId>? direita) =>
        esquerda is null ? direita is null : esquerda.Equals(direita);

    public static bool operator !=(Entidade<TId>? esquerda, Entidade<TId>? direita) =>
        !(esquerda == direita);
}