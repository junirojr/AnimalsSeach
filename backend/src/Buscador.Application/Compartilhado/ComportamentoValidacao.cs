namespace Buscador.Application.Compartilhado;

public sealed class ComportamentoValidacao<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validadores;

    public ComportamentoValidacao(IEnumerable<IValidator<TRequest>> validadores) => _validadores = validadores;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validadores.Any())
            return await next();

        var contexto = new ValidationContext<TRequest>(request);

        var falhas = _validadores
            .Select(v => v.Validate(contexto))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (falhas.Count > 0)
            throw new ValidationException(falhas);

        return await next();
    }
}
