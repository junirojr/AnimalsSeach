using Buscador.Domain.Animais;
using Microsoft.EntityFrameworkCore;

namespace Buscador.Infrastructure.Persistencia;

public class RepositorioAnimal : IRepositorioAnimal
{
    private readonly ContextoBanco _contexto;

    public RepositorioAnimal(ContextoBanco contexto)
    {
        _contexto = contexto;
    }

    public async Task<Animal?> ObterPorIdAsync(AnimalId id, CancellationToken cancellationToken = default)
    {
        return await _contexto.Animais
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Animal animal, CancellationToken cancellationToken = default)
    {
        _contexto.Animais.Add(animal);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task AdicionarVariosAsync(IEnumerable<Animal> animais, CancellationToken cancellationToken = default)
    {
        _contexto.Animais.AddRange(animais);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Animal>> ObterPaginadoAsync(int pagina, int tamanho, CancellationToken cancellationToken = default)
    {
        return await _contexto.Animais
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(cancellationToken);
    }
}
