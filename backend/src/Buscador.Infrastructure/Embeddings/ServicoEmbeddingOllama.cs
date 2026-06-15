using Buscador.Application.Compartilhado;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace Buscador.Infrastructure.Embeddings;

public class ServicoEmbeddingOllama : IServicoEmbedding
{
    private readonly string _baseUrl;
    private const string Modelo = "nomic-embed-text";

    public ServicoEmbeddingOllama(IConfiguration configuracao)
    {
        _baseUrl = configuracao["Ollama:BaseUrl"] ?? "http://localhost:11434";
    }

    public async Task<float[]> GerarAsync(string texto, CancellationToken cancellationToken = default)
    {
        var gerador = new OllamaEmbeddingGenerator(new Uri(_baseUrl), Modelo);
        var resultado = await gerador.GenerateAsync([texto], cancellationToken: cancellationToken);
        return resultado[0].Vector.ToArray();
    }
}
