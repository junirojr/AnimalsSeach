namespace Buscador.Application.Funcionalidades.ObterAnimais;

public sealed class ObterAnimaisConsultaValidador : AbstractValidator<ObterAnimaisConsulta>
{
    public ObterAnimaisConsultaValidador()
    {
        RuleFor(x => x.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Tamanho).InclusiveBetween(1, 100);
    }
}
