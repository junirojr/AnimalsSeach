using Buscador.Domain.Animais;
using FluentAssertions;

namespace Buscador.Domain.Tests.Animais;

public class AnimalIdTests
{
    [Fact]
    public void Novo_GeraValoresUnicos()
    {
        var id1 = AnimalId.Novo();
        var id2 = AnimalId.Novo();

        id1.Valor.Should().NotBe(id2.Valor);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void AnimalId_ComMesmoValor_SaoIguais()
    {
        var guid = Guid.NewGuid();
        var id1 = AnimalId.De(guid);
        var id2 = AnimalId.De(guid);

        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }
}
