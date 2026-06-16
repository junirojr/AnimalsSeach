using Buscador.Domain.Animais;
using System.Text.RegularExpressions;

namespace Buscador.Application.Funcionalidades.GerarEmbeddings;

// Divide um animal em fragmentos (chunks): nome, cada frase dos campos longos e cada tag isolada.
// Tags isoladas dao a atributos como "voo" um vetor proprio, sem diluir na descricao inteira.
public static partial class FragmentadorAnimal
{
    public static IReadOnlyList<string> Fragmentar(Animal animal)
    {
        var fragmentos = new List<string>();

        if (!string.IsNullOrWhiteSpace(animal.NomeComum))
            fragmentos.Add(animal.NomeComum.Trim());

        fragmentos.AddRange(DividirEmFrases(animal.Descricao));
        fragmentos.AddRange(DividirEmFrases(animal.Caracteristicas));
        fragmentos.AddRange(DividirEmFrases(animal.Curiosidades));

        foreach (var tag in animal.Tags)
            if (!string.IsNullOrWhiteSpace(tag))
                fragmentos.Add(tag.Trim());

        return fragmentos
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .ToList();
    }

    private static IEnumerable<string> DividirEmFrases(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return [];

        return SeparadorFrases()
            .Split(texto)
            .Select(f => f.Trim())
            .Where(f => f.Length > 0);
    }

    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex SeparadorFrases();
}
