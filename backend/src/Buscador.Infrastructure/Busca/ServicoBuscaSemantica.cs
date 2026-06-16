using Buscador.Application.Compartilhado;
using Buscador.Domain.Animais;
using Buscador.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Buscador.Infrastructure.Busca;

public class ServicoBuscaSemantica : IServicoBuscaSemantica
{
    private readonly ContextoBanco _contexto;
    private readonly IServicoEmbedding _servicoEmbedding;

    public ServicoBuscaSemantica(ContextoBanco contexto, IServicoEmbedding servicoEmbedding)
    {
        _contexto = contexto;
        _servicoEmbedding = servicoEmbedding;
    }

    public async Task<IReadOnlyList<ResultadoBuscaDto>> BuscarAsync(
        string consulta,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var vetorConsulta = await _servicoEmbedding.GerarAsync(consulta.Trim(), TipoTextoEmbedding.Consulta, cancellationToken);
        var vetorString = "[" + string.Join(",",
            vetorConsulta.Select(f => f.ToString("G", CultureInfo.InvariantCulture))) + "]";

        // Passo 1: IDs e distancias cosine via SQL
        var scores = await _contexto.Database
            .SqlQuery<IdComPontuacao>(
                $"""
                SELECT a.id AS "Id", (embedding <=> {vetorString}::vector) AS "Pontuacao"
                FROM animais a
                WHERE a.embedding IS NOT NULL
                ORDER BY embedding <=> {vetorString}::vector ASC
                LIMIT {limite}
                """)
            .ToListAsync(cancellationToken);

        if (scores.Count == 0)
            return [];

        // Passo 2: carregar entidades pelo ID — EF cuida da conversao de enums
        var animalIds = scores.Select(s => AnimalId.De(s.Id)).ToList();
        var animais = await _contexto.Animais
            .Where(a => animalIds.Contains(a.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Passo 3: combinar mantendo a ordem do score (1 - distancia = similaridade)
        return scores
            .Join(animais, s => s.Id, a => a.Id.Valor,
                (s, a) => new ResultadoBuscaDto(a.ParaDto(), 1 - s.Pontuacao))
            .ToList();
    }

    private record IdComPontuacao(Guid Id, double Pontuacao);
}
