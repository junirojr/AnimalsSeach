using FluentValidation;

namespace Buscador.Application.Funcionalidades.ObterAnimais;

public sealed class ObterAnimaisConsultaValidador : AbstractValidator<ObterAnimaisConsulta>
{
    public ObterAnimaisConsultaValidador()
    {
        RuleFor(x => x.Pagina)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Página deve ser maior ou igual a 1.");

        RuleFor(x => x.Tamanho)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(100)
            .WithMessage("Tamanho deve estar entre 1 e 100.");
    }
}
