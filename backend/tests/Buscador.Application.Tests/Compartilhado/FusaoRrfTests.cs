using Buscador.Application.Compartilhado;
using Buscador.Domain.Animais;
using FluentAssertions;

namespace Buscador.Application.Tests.Compartilhado;

public class FusaoRrfTests
{
    private static ResultadoBuscaDto Resultado(Guid id, double pontuacao)
    {
        var dto = new AnimalDto(id, "Animal", "Nome cientifico", "desc", "carac",
            Dieta.Carnivoro, Habitat.Floresta, "dist", StatusConservacao.PoucoPreocupante, [], "curiosidades");
        return new ResultadoBuscaDto(dto, pontuacao);
    }

    [Fact]
    public void Fundir_AnimalBemRanqueadoNasDuasListas_FicaEmPrimeiro()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();

        // a: 1o no textual e 2o no semantico  -> melhor RRF
        // b: 2o no textual e 1o no semantico
        // c: so aparece no semantico (3o)
        var textual = new List<ResultadoBuscaDto> { Resultado(a, 0.9), Resultado(b, 0.8) };
        var semantica = new List<ResultadoBuscaDto> { Resultado(b, 0.7), Resultado(a, 0.6), Resultado(c, 0.5) };

        var fundido = FusaoRrf.Fundir([textual, semantica], 10);

        fundido.Should().HaveCount(3);
        fundido.Select(r => r.Animal.Id).Should().StartWith(new[] { a, b }); // a e b na frente de c
        fundido.Should().BeInDescendingOrder(r => r.Pontuacao);
        fundido.Last().Animal.Id.Should().Be(c); // c (so 1 lista) por ultimo
    }

    [Fact]
    public void Fundir_RespeitaOLimite()
    {
        var lista = new List<ResultadoBuscaDto>
        {
            Resultado(Guid.NewGuid(), 0.9),
            Resultado(Guid.NewGuid(), 0.8),
            Resultado(Guid.NewGuid(), 0.7)
        };

        var fundido = FusaoRrf.Fundir([lista], 2);

        fundido.Should().HaveCount(2);
    }
}
