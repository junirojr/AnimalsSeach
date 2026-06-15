using FluentValidation;

namespace Buscador.Application.Funcionalidades.BuscarAnimais;

public sealed class BuscarAnimaisConsultaValidador : AbstractValidator<BuscarAnimaisConsulta>
{
    public BuscarAnimaisConsultaValidador()
    {
        RuleFor(x => x.Q)
            .NotEmpty()
            .WithMessage("O termo de busca nao pode ser vazio.");

        RuleFor(x => x.Limite)
            .InclusiveBetween(1, 100)
            .WithMessage("Limite deve estar entre 1 e 100.");
    }
}
