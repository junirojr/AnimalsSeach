namespace Buscador.Application.Compartilhado;

public interface IServicoEmbedding
{
    Task<float[]> GerarAsync(string texto, CancellationToken ct = default);
}
