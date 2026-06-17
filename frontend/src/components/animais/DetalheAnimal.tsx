"use client";

import { useEffect } from "react";
import type { Animal } from "@/types/animal";
import {
  rotulosDieta,
  rotulosHabitat,
  rotulosStatusConservacao,
} from "@/types/animal";

interface DetalheAnimalProps {
  animal: Animal | null;
  aoFechar: () => void;
}

interface SecaoProps {
  titulo: string;
  texto: string;
}

function Secao({ titulo, texto }: SecaoProps) {
  return (
    <section className="mt-4">
      <h3 className="text-sm font-semibold uppercase tracking-wide text-gray-500">
        {titulo}
      </h3>
      <p className="mt-1 whitespace-pre-line text-sm leading-relaxed text-gray-700">
        {texto}
      </p>
    </section>
  );
}

export function DetalheAnimal({ animal, aoFechar }: DetalheAnimalProps) {
  useEffect(() => {
    if (animal === null) {
      return;
    }

    function aoPressionarTecla(evento: KeyboardEvent) {
      if (evento.key !== "Escape") {
        return;
      }

      aoFechar();
    }

    window.addEventListener("keydown", aoPressionarTecla);

    return () => {
      window.removeEventListener("keydown", aoPressionarTecla);
    };
  }, [animal, aoFechar]);

  if (animal === null) {
    return null;
  }

  const temTags = animal.tags.length > 0;

  const rotuloHabitat = rotulosHabitat[animal.habitat] ?? "—";
  const rotuloDieta = rotulosDieta[animal.dieta] ?? "—";
  const rotuloStatus = rotulosStatusConservacao[animal.statusConservacao] ?? "—";

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onClick={aoFechar}
    >
      <div
        className="relative max-h-[85vh] w-full max-w-2xl overflow-y-auto rounded-2xl bg-white p-6 shadow-xl"
        role="dialog"
        aria-modal="true"
        onClick={(evento) => evento.stopPropagation()}
      >
        <button
          type="button"
          aria-label="Fechar"
          onClick={aoFechar}
          className="absolute right-4 top-4 flex h-8 w-8 items-center justify-center rounded-full text-gray-400 transition hover:bg-gray-100 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-emerald-400"
        >
          <span aria-hidden="true" className="text-lg leading-none">
            ×
          </span>
        </button>

        <header className="pr-10">
          <h2 className="text-2xl font-bold text-gray-900">
            {animal.nomeComum}
          </h2>
          <p className="text-base italic text-gray-500">
            {animal.nomeCientifico}
          </p>
        </header>

        <div className="mt-4 flex flex-wrap gap-2">
          <span className="rounded-full bg-emerald-100 px-3 py-1 text-xs font-medium text-emerald-800">
            {rotuloHabitat}
          </span>
          <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-medium text-amber-800">
            {rotuloDieta}
          </span>
          <span className="rounded-full bg-rose-100 px-3 py-1 text-xs font-medium text-rose-800">
            {rotuloStatus}
          </span>
        </div>

        <Secao titulo="Descricao" texto={animal.descricao} />
        <Secao titulo="Caracteristicas" texto={animal.caracteristicas} />
        <Secao titulo="Curiosidades" texto={animal.curiosidades} />
        <Secao
          titulo="Distribuicao geografica"
          texto={animal.distribuicaoGeografica}
        />

        {temTags && (
          <div className="mt-6 flex flex-wrap gap-1.5 border-t border-gray-100 pt-4">
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
    </div>
  );
}
