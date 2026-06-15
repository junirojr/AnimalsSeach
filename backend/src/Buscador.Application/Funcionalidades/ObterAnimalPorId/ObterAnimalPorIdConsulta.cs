using Buscador.Application.Compartilhado;
using MediatR;

namespace Buscador.Application.Funcionalidades.ObterAnimalPorId;

public record ObterAnimalPorIdConsulta(Guid Id) : IRequest<AnimalDto?>;
