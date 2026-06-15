namespace Buscador.Application.Compartilhado;

public interface IServicoBuscaSemantica
{
    Task<IReadOnlyList<ResultadoBuscaDto>> BuscarAsync(string consulta, int limite, CancellationToken cancellationToken = default);
}
