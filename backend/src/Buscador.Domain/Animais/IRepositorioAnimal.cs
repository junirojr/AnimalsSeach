namespace Buscador.Domain.Animais;

public interface IRepositorioAnimal
{
    Task<Animal?> ObterPorIdAsync(AnimalId id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Animal animal, CancellationToken cancellationToken = default);
    Task AdicionarVariosAsync(IEnumerable<Animal> animais, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Animal>> ObterPaginadoAsync(int pagina, int tamanho, CancellationToken cancellationToken = default);
}
