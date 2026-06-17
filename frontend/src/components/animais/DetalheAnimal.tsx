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
    <section className="mt-5">
      <h3 className="text-xs font-bold uppercase tracking-widest" style={{ color: "#6b9690" }}>
        {titulo}
      </h3>
      <p className="mt-1.5 whitespace-pre-line text-sm leading-relaxed" style={{ color: "#b8d4d0" }}>
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
      className="fixed inset-0 z-50 flex items-end justify-center p-4 sm:items-center"
      style={{ background: "rgba(0,0,0,0.75)" }}
      onClick={aoFechar}
    >
      <div
        className="relative max-h-[85vh] w-full max-w-2xl overflow-y-auto rounded-2xl p-6"
        style={{
          background: "#0d1f1e",
          border: "1px solid #1a3330",
          boxShadow: "0 25px 50px rgba(0,0,0,0.6)",
        }}
        role="dialog"
        aria-modal="true"
        onClick={(evento) => evento.stopPropagation()}
      >
        <button
          type="button"
          aria-label="Fechar"
          onClick={aoFechar}
          className="absolute right-4 top-4 flex h-8 w-8 items-center justify-center rounded-full text-lg transition-colors focus:outline-none focus:ring-2 focus:ring-[#00e5cc]"
          style={{ color: "#6b9690", border: "1px solid #1a3330" }}
        >
          <span aria-hidden="true" className="leading-none">×</span>
        </button>

        <header className="pr-10">
          <h2 className="text-2xl font-bold text-white">{animal.nomeComum}</h2>
          <p className="text-sm italic" style={{ color: "#6b9690" }}>
            {animal.nomeCientifico}
          </p>
        </header>

        <div className="mt-4 flex flex-wrap gap-2">
          <span
            className="rounded-full px-3 py-1 font-mono text-xs font-semibold uppercase tracking-wider"
            style={{ background: "rgba(0,229,204,0.08)", border: "1px solid rgba(0,229,204,0.2)", color: "#00e5cc" }}
          >
            {rotuloHabitat}
          </span>
          <span
            className="rounded-full px-3 py-1 font-mono text-xs font-semibold uppercase tracking-wider"
            style={{ background: "rgba(251,191,36,0.08)", border: "1px solid rgba(251,191,36,0.2)", color: "#fbbf24" }}
          >
            {rotuloDieta}
          </span>
          <span
            className="rounded-full px-3 py-1 font-mono text-xs font-semibold uppercase tracking-wider"
            style={{ background: "rgba(248,113,113,0.08)", border: "1px solid rgba(248,113,113,0.2)", color: "#f87171" }}
          >
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
          <div className="mt-6 flex flex-wrap gap-1.5 pt-4" style={{ borderTop: "1px solid #1a3330" }}>
            {animal.tags.map((tag) => (
              <span
                key={tag}
                className="rounded-full px-2.5 py-0.5 text-xs"
                style={{ background: "#122624", border: "1px solid #1a3330", color: "#6b9690" }}
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
