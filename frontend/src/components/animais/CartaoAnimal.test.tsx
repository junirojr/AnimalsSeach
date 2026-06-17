import { render, screen } from "@testing-library/react";
import { CartaoAnimal } from "@/components/animais/CartaoAnimal";
import type { Animal } from "@/types/animal";

const animalFake: Animal = {
  id: "1",
  nomeComum: "Onca-pintada",
  nomeCientifico: "Panthera onca",
  descricao: "Maior felino das Americas.",
  caracteristicas: "Pelagem amarelada com rosetas.",
  dieta: 0,
  habitat: 0,
  distribuicaoGeografica: "America do Sul",
  statusConservacao: 2,
  tags: ["felino"],
  curiosidades: "Mordida muito forte.",
};

describe("CartaoAnimal", () => {
  it("renderiza nome comum e rotulos de habitat e dieta", () => {
    render(<CartaoAnimal animal={animalFake} />);

    expect(screen.getByText("Onca-pintada")).toBeInTheDocument();
    expect(screen.getByText("Floresta")).toBeInTheDocument();
    expect(screen.getByText("Carnivoro")).toBeInTheDocument();
  });
});
