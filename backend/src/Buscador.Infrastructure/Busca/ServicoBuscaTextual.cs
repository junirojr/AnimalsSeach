using System.Text.RegularExpressions;
using Buscador.Application.Compartilhado;
using Buscador.Domain.Animais;
using Buscador.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Buscador.Infrastructure.Busca;

public class ServicoBuscaTextual : IServicoBuscaTextual
{
    private readonly ContextoBanco _contexto;

    public ServicoBuscaTextual(ContextoBanco contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<ResultadoBuscaDto>> BuscarAsync(
        string consulta,
        int limite,
        CancellationToken cancellationToken = default)
    {
        // Sanitiza (remove pontuacao que quebraria o to_tsquery) e une os termos com OR (|),
        // para maximizar a recall: o ts_rank ja premia quem casa mais termos e o hibrido/RRF
        // reordena. unaccent (no SQL) torna a busca insensivel a acento.
        var termos = Regex
            .Replace(consulta, @"[^\p{L}\p{N}\s]", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (termos.Length == 0)
            return [];

        var consultaPreparada = string.Join(" | ", termos);

        // Passo 1: IDs e scores via SQL — evita tipo custom com enums
        var scores = await _contexto.Database
            .SqlQuery<IdComPontuacao>(
                $"""
                SELECT a.id AS "Id", ts_rank(a.search_vector, q) AS "Pontuacao"
                FROM animais a,
                     to_tsquery('portuguese', unaccent({consultaPreparada})) q
                WHERE a.search_vector @@ q
                ORDER BY ts_rank(a.search_vector, q) DESC
                LIMIT {limite}
                """)
            .ToListAsync(cancellationToken);

        if (scores.Count == 0)
            return [];

        // Passo 2: carregar entidades pelo ID
        var animalIds = scores.Select(s => AnimalId.De(s.Id)).ToList();
        var animais = await _contexto.Animais
            .Where(a => animalIds.Contains(a.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Passo 3: combinar mantendo a ordem do score
        return scores
            .Join(animais, s => s.Id, a => a.Id.Valor,
                (s, a) => new ResultadoBuscaDto(a.ParaDto(), s.Pontuacao))
            .ToList();
    }

    private record IdComPontuacao(Guid Id, double Pontuacao);
}
