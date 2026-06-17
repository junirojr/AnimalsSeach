using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Buscador.Api.TratamentoErros;

public sealed class ManipuladorGlobalExcecoes : IExceptionHandler
{
    private readonly ILogger<ManipuladorGlobalExcecoes> _logger;

    public ManipuladorGlobalExcecoes(ILogger<ManipuladorGlobalExcecoes> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception excecao,
        CancellationToken cancellationToken)
    {
        if (excecao is ValidationException ve)
        {
            var detalhes = string.Join(" ", ve.Errors.Select(e => e.ErrorMessage));

            var problemaValidacao = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro de validacao.",
                Detail = detalhes
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(problemaValidacao, cancellationToken);

            return true;
        }

        _logger.LogError(excecao, "Excecao nao tratada durante o processamento da requisicao.");

        var problema = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno do servidor.",
            Detail = "Ocorreu um erro inesperado ao processar a requisicao."
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problema, cancellationToken);

        return true;
    }
}
