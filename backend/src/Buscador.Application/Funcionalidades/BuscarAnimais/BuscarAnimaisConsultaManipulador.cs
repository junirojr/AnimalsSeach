using Buscador.Application.Compartilhado;
using MediatR;

namespace Buscador.Application.Funcionalidades.BuscarAnimais;

public sealed class BuscarAnimaisConsultaManipulador
    : IRequestHandler<BuscarAnimaisConsulta, IReadOnlyList<ResultadoBuscaDto>>
{
    private readonly IServicoBuscaTextual _servicoBuscaTextual;
    private readonly IServicoBuscaSemantica _servicoBuscaSemantica;
    private readonly IServicoBuscaHibrida _servicoBuscaHibrida;

    public BuscarAnimaisConsultaManipulador(
        IServicoBuscaTextual servicoBuscaTextual,
        IServicoBuscaSemantica servicoBuscaSemantica,
        IServicoBuscaHibrida servicoBuscaHibrida)
    {
        _servicoBuscaTextual = servicoBuscaTextual;
        _servicoBuscaSemantica = servicoBuscaSemantica;
        _servicoBuscaHibrida = servicoBuscaHibrida;
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
                await _servicoBuscaHibrida.BuscarAsync(request.Q, request.Limite, cancellationToken),

            _ => throw new NotSupportedException($"Modo de busca nao suportado: {request.Modo}")
        };
    }
}
