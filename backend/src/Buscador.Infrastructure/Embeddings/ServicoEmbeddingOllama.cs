using Buscador.Application.Compartilhado;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace Buscador.Infrastructure.Embeddings;

public class ServicoEmbeddingOllama : IServicoEmbedding
{
    private readonly string _baseUrl;
    // bge-m3: modelo multilingue (1024 dimensoes). NAO usa prefixos de tarefa.
    private const string Modelo = "bge-m3";

    public ServicoEmbeddingOllama(IConfiguration configuracao)
    {
        _baseUrl = configuracao["Ollama:BaseUrl"] ?? "http://localhost:11434";
    }

    public async Task<float[]> GerarAsync(string texto, TipoTextoEmbedding tipo, CancellationToken cancellationToken = default)
    {
        var resultado = await GerarVariosAsync([texto], tipo, cancellationToken);
        return resultado[0];
    }

    public async Task<IReadOnlyList<float[]>> GerarVariosAsync(IReadOnlyList<string> textos, TipoTextoEmbedding tipo, CancellationToken cancellationToken = default)
    {
        if (textos.Count == 0)
            return [];

        var gerador = new OllamaEmbeddingGenerator(new Uri(_baseUrl), Modelo);
        var resultado = await gerador.GenerateAsync(textos, cancellationToken: cancellationToken);
        return resultado.Select(e => e.Vector.ToArray()).ToList();
    }
}
