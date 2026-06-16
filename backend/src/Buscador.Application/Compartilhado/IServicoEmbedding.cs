namespace Buscador.Application.Compartilhado;

public enum TipoTextoEmbedding
{
    Documento,
    Consulta
}

public interface IServicoEmbedding
{
    Task<float[]> GerarAsync(string texto, TipoTextoEmbedding tipo, CancellationToken cancellationToken = default);
}
