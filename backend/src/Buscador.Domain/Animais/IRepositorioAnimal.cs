namespace Buscador.Domain.Animais;

public interface IRepositorioAnimal
{
    Task<Animal?> ObterPorIdAsync(AnimalId id, CancellationToken ct = default);
    Task<IReadOnlyList<Animal>> ObterPaginadoAsync(int pagina, int tamanho, CancellationToken ct = default);
    Task AdicionarAsync(Animal animal, CancellationToken ct = default);
    Task AdicionarVariosAsync(IEnumerable<Animal> animais, CancellationToken ct = default);
}
