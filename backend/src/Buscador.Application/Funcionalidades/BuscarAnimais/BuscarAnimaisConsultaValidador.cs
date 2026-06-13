namespace Buscador.Application.Funcionalidades.BuscarAnimais;

public sealed class BuscarAnimaisConsultaValidador : AbstractValidator<BuscarAnimaisConsulta>
{
    public BuscarAnimaisConsultaValidador()
    {
        RuleFor(x => x.Q).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Limite).InclusiveBetween(1, 50);
    }
}
