namespace Buscador.Application.Funcionalidades.ObterAnimais;

public record ObterAnimaisConsulta(int Pagina = 1, int Tamanho = 20) : IRequest<IReadOnlyList<AnimalDto>>;
