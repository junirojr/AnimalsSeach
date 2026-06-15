using Buscador.Application.Compartilhado;
using MediatR;

namespace Buscador.Application.Funcionalidades.ObterAnimais;

public record ObterAnimaisConsulta(int Pagina, int Tamanho) : IRequest<IReadOnlyList<AnimalDto>>;
