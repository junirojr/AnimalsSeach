import { http, HttpResponse } from "msw";
import type { ResultadoBusca } from "@/types/animal";

const baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5024";

const resultadosFake: ResultadoBusca[] = [
  {
    animal: {
      id: "11111111-1111-1111-1111-111111111111",
      nomeComum: "Onca-pintada",
      nomeCientifico: "Panthera onca",
      descricao: "Maior felino das Americas.",
      caracteristicas: "Pelagem amarelada com rosetas.",
      dieta: "Carnivoro",
      habitat: "Floresta",
      distribuicaoGeografica: "America do Sul e Central",
      statusConservacao: "Vulneravel",
      tags: ["felino", "predador"],
      curiosidades: "Tem a mordida mais forte entre os felinos.",
    },
    pontuacao: 0.92,
  },
  {
    animal: {
      id: "22222222-2222-2222-2222-222222222222",
      nomeComum: "Arara-azul",
      nomeCientifico: "Anodorhynchus hyacinthinus",
      descricao: "Maior especie de arara.",
      caracteristicas: "Plumagem azul-cobalto.",
      dieta: "Herbivoro",
      habitat: "Savana",
      distribuicaoGeografica: "Brasil central",
      statusConservacao: "Vulneravel",
      tags: ["ave", "psitacideo"],
      curiosidades: "Usa o bico para quebrar cocos.",
    },
    pontuacao: 0.78,
  },
];

export const handlers = [
  http.get(`${baseUrl}/api/animais/buscar`, () => {
    return HttpResponse.json(resultadosFake);
  }),
];
