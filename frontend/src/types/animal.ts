export type ModoBusca = "Textual" | "Semantica" | "Hibrida";

export interface Animal {
  id: string;
  nomeComum: string;
  nomeCientifico: string;
  descricao: string;
  caracteristicas: string;
  dieta: number;
  habitat: number;
  distribuicaoGeografica: string;
  statusConservacao: number;
  tags: string[];
  curiosidades: string;
}

export interface ResultadoBusca {
  animal: Animal;
  pontuacao: number;
}

// Mapas int -> rotulo PT, derivados dos enums do Domain (.NET). A ordem dos indices
// segue EXATAMENTE a ordem de declaracao dos enums em backend/src/Buscador.Domain/Animais/.
export const rotulosDieta: Record<number, string> = {
  0: "Carnivoro",
  1: "Herbivoro",
  2: "Onivoro",
};

export const rotulosHabitat: Record<number, string> = {
  0: "Floresta",
  1: "Oceano",
  2: "Deserto",
  3: "Savana",
  4: "Montanha",
  5: "Agua doce",
  6: "Polar",
};

export const rotulosStatusConservacao: Record<number, string> = {
  0: "Pouco preocupante",
  1: "Quase ameacado",
  2: "Vulneravel",
  3: "Em perigo",
  4: "Criticamente em perigo",
  5: "Extinto na natureza",
  6: "Extinto",
};
