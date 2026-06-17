namespace Buscador.Application.Compartilhado;

public interface IServicoEmbedding
{
    Task<float[]> GerarAsync(string texto, CancellationToken cancellationToken = default);

    // Gera os embeddings de varios textos numa unica chamada (lote) — bem mais rapido que 1 a 1.
    Task<IReadOnlyList<float[]>> GerarVariosAsync(IReadOnlyList<string> textos, CancellationToken cancellationToken = default);
}
