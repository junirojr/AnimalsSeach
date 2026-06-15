namespace Buscador.Domain.Comum;

public abstract class Entidade<TId>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entidade(TId id)
    {
        Id = id;
    }

    protected Entidade() { }

    public override bool Equals(object? obj)
    {
        if (obj is not Entidade<TId> outra)
            return false;

        if (ReferenceEquals(this, outra))
            return true;

        if (GetType() != outra.GetType())
            return false;

        return Id.Equals(outra.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entidade<TId>? a, Entidade<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entidade<TId>? a, Entidade<TId>? b) => !(a == b);
}
