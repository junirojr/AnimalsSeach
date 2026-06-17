using FluentValidation;

namespace Buscador.Application.Funcionalidades.CadastrarAnimal;

public sealed class CadastrarAnimalValidador : AbstractValidator<CadastrarAnimalComando>
{
    public CadastrarAnimalValidador()
    {
        RuleFor(x => x.NomeComum)
            .NotEmpty()
            .WithMessage("Nome comum nao pode ser vazio.");

        RuleFor(x => x.NomeCientifico)
            .NotEmpty()
            .WithMessage("Nome cientifico nao pode ser vazio.");

        RuleFor(x => x.Tags)
            .NotNull()
            .WithMessage("Tags nao pode ser nulo.");
    }
}
