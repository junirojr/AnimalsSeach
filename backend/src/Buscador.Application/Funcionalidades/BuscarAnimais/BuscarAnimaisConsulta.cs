using Buscador.Application.Compartilhado;
using MediatR;

namespace Buscador.Application.Funcionalidades.BuscarAnimais;

public record BuscarAnimaisConsulta(
    string Q,
    ModoBusca Modo = ModoBusca.Textual,
    int Limite = 10
) : IRequest<IReadOnlyList<ResultadoBuscaDto>>;
