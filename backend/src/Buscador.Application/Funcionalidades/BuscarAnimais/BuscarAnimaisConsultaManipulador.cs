namespace Buscador.Application.Funcionalidades.BuscarAnimais;

public sealed class BuscarAnimaisConsultaManipulador : IRequestHandler<BuscarAnimaisConsulta, IReadOnlyList<ResultadoBuscaDto>>
{
    private readonly IServicoBuscaTextual _textual;
    private readonly IServicoBuscaSemantica _semantica;
    private readonly IServicoBuscaHibrida _hibrida;

    public BuscarAnimaisConsultaManipulador(
        IServicoBuscaTextual textual,
        IServicoBuscaSemantica semantica,
        IServicoBuscaHibrida hibrida)
    {
        _textual = textual;
        _semantica = semantica;
        _hibrida = hibrida;
    }

    public Task<IReadOnlyList<ResultadoBuscaDto>> Handle(BuscarAnimaisConsulta request, CancellationToken cancellationToken) =>
        request.Modo switch
        {
            ModoBusca.Textual   => _textual.BuscarAsync(request.Q, request.Limite, cancellationToken),
            ModoBusca.Semantica => _semantica.BuscarAsync(request.Q, request.Limite, cancellationToken),
            ModoBusca.Hibrida   => _hibrida.BuscarAsync(request.Q, request.Limite, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Modo))
        };
}
