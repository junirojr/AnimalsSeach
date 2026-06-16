namespace Buscador.Application.Compartilhado;

public interface IServicoBuscaHibrida
{
    Task<IReadOnlyList<ResultadoBuscaDto>> BuscarAsync(string consulta, int limite, CancellationToken cancellationToken = default);
}
