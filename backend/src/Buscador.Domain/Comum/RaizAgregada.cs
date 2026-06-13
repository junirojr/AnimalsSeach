namespace Buscador.Domain.Comum;

public abstract class RaizAgregada<TId> : Entidade<TId>
{
    protected RaizAgregada(TId id) : base(id)
    {
    }
}