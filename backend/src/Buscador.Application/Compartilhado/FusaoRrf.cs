namespace Buscador.Application.Compartilhado;

// Reciprocal Rank Fusion: funde varias listas ja ordenadas por relevancia.
// score(animal) = soma de 1/(K + posicao) em cada lista onde ele aparece (posicao comeca em 1).
public static class FusaoRrf
{
    public const int K = 60; // valor classico do paper de RRF

    public static IReadOnlyList<ResultadoBuscaDto> Fundir(
        IEnumerable<IReadOnlyList<ResultadoBuscaDto>> listasRanqueadas,
        int limite)
    {
        var pontuacoes = new Dictionary<Guid, double>();
        var animais = new Dictionary<Guid, AnimalDto>();

        foreach (var lista in listasRanqueadas)
        {
            for (var i = 0; i < lista.Count; i++)
            {
                var id = lista[i].Animal.Id;
                pontuacoes[id] = pontuacoes.GetValueOrDefault(id) + 1.0 / (K + i + 1);
                animais[id] = lista[i].Animal;
            }
        }

        return pontuacoes
            .OrderByDescending(p => p.Value)
            .Take(limite)
            .Select(p => new ResultadoBuscaDto(animais[p.Key], p.Value))
            .ToList();
    }
}
