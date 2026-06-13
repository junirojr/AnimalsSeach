namespace Buscador.Application.Compartilhado;

public interface IServicoBuscaTextual
{
    Task<IReadOnlyList<ResultadoBuscaDto>> BuscarAsync(string consulta, int limite, CancellationToken ct = default);
}
