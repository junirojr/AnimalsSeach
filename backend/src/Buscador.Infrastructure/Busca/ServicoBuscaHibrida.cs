using Buscador.Application.Compartilhado;

namespace Buscador.Infrastructure.Busca;

public class ServicoBuscaHibrida : IServicoBuscaHibrida
{
    private readonly IServicoBuscaTextual _textual;
    private readonly IServicoBuscaSemantica _semantica;

    public ServicoBuscaHibrida(IServicoBuscaTextual textual, IServicoBuscaSemantica semantica)
    {
        _textual = textual;
        _semantica = semantica;
    }

    public async Task<IReadOnlyList<ResultadoBuscaDto>> BuscarAsync(
        string consulta, int limite, CancellationToken cancellationToken = default)
    {
        // Pool de candidatos maior que o limite final, para a fusao ter material dos dois lados.
        var tamanhoPool = Math.Max(limite, 20);

        var textual = await _textual.BuscarAsync(consulta, tamanhoPool, cancellationToken);
        var semantica = await _semantica.BuscarAsync(consulta, tamanhoPool, cancellationToken);

        return FusaoRrf.Fundir([textual, semantica], limite);
    }
}
