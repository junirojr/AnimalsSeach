using Buscador.Application.Compartilhado;
using MediatR;

namespace Buscador.Application.Funcionalidades.BuscarAnimais;

public sealed class BuscarAnimaisConsultaManipulador
    : IRequestHandler<BuscarAnimaisConsulta, IReadOnlyList<ResultadoBuscaDto>>
{
    private readonly IServicoBuscaTextual _servicoBuscaTextual;
    private readonly IServicoBuscaSemantica _servicoBuscaSemantica;

    public BuscarAnimaisConsultaManipulador(
        IServicoBuscaTextual servicoBuscaTextual,
        IServicoBuscaSemantica servicoBuscaSemantica)
    {
        _servicoBuscaTextual = servicoBuscaTextual;
        _servicoBuscaSemantica = servicoBuscaSemantica;
    }

    public async Task<IReadOnlyList<ResultadoBuscaDto>> Handle(
        BuscarAnimaisConsulta request,
        CancellationToken cancellationToken)
    {
        return request.Modo switch
        {
            ModoBusca.Textual =>
                await _servicoBuscaTextual.BuscarAsync(request.Q, request.Limite, cancellationToken),

            ModoBusca.Semantica =>
                await _servicoBuscaSemantica.BuscarAsync(request.Q, request.Limite, cancellationToken),

            ModoBusca.Hibrida =>
                throw new NotSupportedException("Busca hibrida sera implementada na Fase 6."),

            _ => throw new NotSupportedException($"Modo de busca nao suportado: {request.Modo}")
        };
    }
}
