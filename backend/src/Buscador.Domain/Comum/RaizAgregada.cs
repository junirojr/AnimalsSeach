namespace Buscador.Domain.Comum;

public abstract class RaizAgregada<TId> : Entidade<TId>
    where TId : notnull
{
    protected RaizAgregada(TId id) : base(id) { }

    protected RaizAgregada() { }
}
