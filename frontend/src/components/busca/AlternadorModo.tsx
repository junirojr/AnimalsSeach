"use client";

import type { ModoBusca } from "@/types/animal";

interface AlternadorModoProps {
  modo: ModoBusca;
  aoTrocarModo: (modo: ModoBusca) => void;
}

const modos: { valor: ModoBusca; rotulo: string }[] = [
  { valor: "Textual", rotulo: "Textual" },
  { valor: "Semantica", rotulo: "Semântica" },
  { valor: "Hibrida", rotulo: "Híbrida" },
];

export default function AlternadorModo({ modo, aoTrocarModo }: AlternadorModoProps) {
  return (
    <div className="inline-flex divide-x divide-gray-300 overflow-hidden rounded-lg border border-gray-300">
      {modos.map((item) => {
        const ativo = item.valor === modo;
        const classesAtivo = "bg-blue-600 text-white";
        const classesInativo = "bg-white text-gray-700 hover:bg-gray-100";
        const classes = ativo ? classesAtivo : classesInativo;

        return (
          <button
            key={item.valor}
            type="button"
            aria-pressed={ativo}
            onClick={() => aoTrocarModo(item.valor)}
            className={`px-4 py-2 text-sm font-medium transition ${classes}`}
          >
            {item.rotulo}
          </button>
        );
      })}
    </div>
  );
}
