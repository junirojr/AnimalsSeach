export type ModoBusca = "Textual" | "Semantica" | "Hibrida";

export interface Animal {
  id: string;
  nomeComum: string;
  nomeCientifico: string;
  descricao: string;
  caracteristicas: string;
  dieta: string;
  habitat: string;
  distribuicaoGeografica: string;
  statusConservacao: string;
  tags: string[];
  curiosidades: string;
}

export interface ResultadoBusca {
  animal: Animal;
  pontuacao: number;
}

// Mapas nome-enum -> rotulo PT exibivel. Chaves = valores exatos que a API serializa (JsonStringEnumConverter).
export const rotulosDieta: Record<string, string> = {
  Carnivoro: "Carnívoro",
  Herbivoro: "Herbívoro",
  Onivoro: "Onívoro",
};

export const rotulosHabitat: Record<string, string> = {
  Floresta: "Floresta",
  Oceano: "Oceano",
  Deserto: "Deserto",
  Savana: "Savana",
  Montanha: "Montanha",
  AguaDoce: "Água doce",
  Polar: "Polar",
};

export const rotulosStatusConservacao: Record<string, string> = {
  PoucoPreocupante: "Pouco preocupante",
  QuaseAmeacado: "Quase ameaçado",
  Vulneravel: "Vulnerável",
  EmPerigo: "Em perigo",
  CriticamenteEmPerigo: "Criticamente em perigo",
  ExtintoNaNatureza: "Extinto na natureza",
  Extinto: "Extinto",
};
