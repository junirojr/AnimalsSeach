import type { KeyboardEvent } from "react";
import type { Animal } from "@/types/animal";
import { rotulosDieta, rotulosHabitat } from "@/types/animal";

interface CartaoAnimalProps {
  animal: Animal;
  pontuacao?: number;
  aoClicar?: (animal: Animal) => void;
}

function formatarPontuacao(pontuacao: number): string {
  if (pontuacao >= 0 && pontuacao <= 1) {
    return `${Math.round(pontuacao * 100)}%`;
  }

  return pontuacao.toFixed(2);
}

export function CartaoAnimal({ animal, pontuacao, aoClicar }: CartaoAnimalProps) {
  const ehClicavel = aoClicar !== undefined;
  const temTags = animal.tags.length > 0;
  const exibirPontuacao = pontuacao !== undefined;

  const rotuloHabitat = rotulosHabitat[animal.habitat] ?? "—";
  const rotuloDieta = rotulosDieta[animal.dieta] ?? "—";

  function aoPressionarTecla(evento: KeyboardEvent<HTMLDivElement>) {
    if (!ehClicavel) {
      return;
    }

    if (evento.key !== "Enter" && evento.key !== " ") {
      return;
    }

    evento.preventDefault();
    aoClicar?.(animal);
  }

  const classesBase =
    "relative rounded-xl border border-gray-200 bg-white p-4 shadow-sm transition";
  const classesClicavel = ehClicavel
    ? "cursor-pointer hover:-translate-y-0.5 hover:border-gray-300 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-emerald-400"
    : "";

  return (
    <div
      className={`${classesBase} ${classesClicavel}`}
      role={ehClicavel ? "button" : undefined}
      tabIndex={ehClicavel ? 0 : undefined}
      onClick={() => aoClicar?.(animal)}
      onKeyDown={aoPressionarTecla}
    >
      {exibirPontuacao && (
        <span className="absolute right-3 top-3 text-xs text-gray-500">
          {formatarPontuacao(pontuacao)}
        </span>
      )}

      <h3 className="pr-12 text-lg font-semibold text-gray-900">
        {animal.nomeComum}
      </h3>
      <p className="text-sm italic text-gray-500">{animal.nomeCientifico}</p>

      <div className="mt-3 flex flex-wrap gap-2">
        <span className="rounded-full bg-emerald-100 px-2.5 py-0.5 text-xs font-medium text-emerald-800">
          {rotuloHabitat}
        </span>
        <span className="rounded-full bg-amber-100 px-2.5 py-0.5 text-xs font-medium text-amber-800">
          {rotuloDieta}
        </span>
      </div>

      {temTags && (
        <div className="mt-3 flex flex-wrap gap-1.5">
          {animal.tags.map((tag) => (
            <span
              key={tag}
              className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600"
            >
              {tag}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
