using Buscador.Application.Compartilhado;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace Buscador.Infrastructure.Embeddings;

public class ServicoEmbeddingOllama : IServicoEmbedding
{
    private readonly string _baseUrl;
    // bge-m3: modelo multilingue (1024 dimensoes). NAO usa prefixos de tarefa
    // (search_query/search_document) — query e documento sao tratados de forma simetrica.
    private const string Modelo = "bge-m3";

    public ServicoEmbeddingOllama(IConfiguration configuracao)
    {
        _baseUrl = configuracao["Ollama:BaseUrl"] ?? "http://localhost:11434";
    }

    public async Task<float[]> GerarAsync(string texto, TipoTextoEmbedding tipo, CancellationToken cancellationToken = default)
    {
        // bge-m3 nao requer prefixo por tipo; 'tipo' fica na interface para permitir
        // voltar a um modelo que precise (ex.: nomic-embed-text).
        var gerador = new OllamaEmbeddingGenerator(new Uri(_baseUrl), Modelo);
        var resultado = await gerador.GenerateAsync([texto], cancellationToken: cancellationToken);
        return resultado[0].Vector.ToArray();
    }
}
