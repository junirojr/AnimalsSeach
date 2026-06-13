namespace Buscador.Application.Funcionalidades.BuscarAnimais;

public record BuscarAnimaisConsulta(
    string Q,
    ModoBusca Modo = ModoBusca.Hibrida,
    int Limite = 10
) : IRequest<IReadOnlyList<ResultadoBuscaDto>>;
