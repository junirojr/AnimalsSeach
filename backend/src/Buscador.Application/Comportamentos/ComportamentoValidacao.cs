using FluentValidation;
using MediatR;

namespace Buscador.Application.Comportamentos;

public sealed class ComportamentoValidacao<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validadores;

    public ComportamentoValidacao(IEnumerable<IValidator<TRequest>> validadores)
    {
        _validadores = validadores;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validadores.Any())
        {
            return await next();
        }

        var contexto = new ValidationContext<TRequest>(request);

        var resultados = await Task.WhenAll(
            _validadores.Select(validador => validador.ValidateAsync(contexto, cancellationToken)));

        var falhas = resultados
            .SelectMany(resultado => resultado.Errors)
            .Where(falha => falha is not null)
            .ToList();

        if (falhas.Count != 0)
        {
            throw new ValidationException(falhas);
        }

        return await next();
    }
}
