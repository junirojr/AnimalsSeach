using Buscador.Application.Funcionalidades.GerarEmbeddings;
using Buscador.Application.Funcionalidades.PopularAnimais;
using FluentAssertions;

namespace Buscador.Application.Tests.Funcionalidades.GerarEmbeddings;

public class FragmentadorAnimalTests
{
    [Fact]
    public void Fragmentar_ComAnimalSemente_IncluiNomeETodasAsTagsSeparadas()
    {
        var animal = DadosSementeAnimal.Animais.First();

        var fragmentos = FragmentadorAnimal.Fragmentar(animal);

        fragmentos.Should().Contain(animal.NomeComum.Trim());
        foreach (var tag in animal.Tags)
            fragmentos.Should().Contain(f => f.Contains(tag.Trim()));
        fragmentos.Count.Should().BeGreaterThan(animal.Tags.Length);
    }
}
