namespace Buscador.Application.Compartilhado;

// Contrato para a Infrastructure orquestrar: buscar animais, gerar embeddings via Ollama e salvar como shadow property.
public interface IServicoPersistenciaEmbedding
{
    Task<int> GerarESalvarParaTodosAnimaisAsync(CancellationToken ct = default);
}
