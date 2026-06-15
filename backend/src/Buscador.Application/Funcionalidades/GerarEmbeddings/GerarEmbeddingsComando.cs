using MediatR;

namespace Buscador.Application.Funcionalidades.GerarEmbeddings;

public record GerarEmbeddingsComando() : IRequest<int>;
