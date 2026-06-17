namespace Buscador.Application.Compartilhado;

public enum TipoTextoEmbedding
{
    Documento,
    Consulta
}

public interface IServicoEmbedding
{
    Task<float[]> GerarAsync(string texto, TipoTextoEmbedding tipo, CancellationToken cancellationToken = default);

    // Gera os embeddings de varios textos numa unica chamada (lote) — bem mais rapido que 1 a 1.
    Task<IReadOnlyList<float[]>> GerarVariosAsync(IReadOnlyList<string> textos, TipoTextoEmbedding tipo, CancellationToken cancellationToken = default);
}
