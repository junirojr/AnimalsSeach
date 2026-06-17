"use client";

import type { ModoBusca } from "@/types/animal";

interface AlternadorModoProps {
  modo: ModoBusca;
  aoTrocarModo: (modo: ModoBusca) => void;
}

const modos: { valor: ModoBusca; abrev: string }[] = [
  { valor: "Hibrida", abrev: "HÍBRIDA" },
  { valor: "Textual", abrev: "FTS" },
  { valor: "Semantica", abrev: "SEMÂNTICA" },
];

export default function AlternadorModo({ modo, aoTrocarModo }: AlternadorModoProps) {
  return (
    <div className="flex items-center gap-2">
      {modos.map((item) => {
        const ativo = item.valor === modo;

        if (ativo) {
          return (
            <button
              key={item.valor}
              type="button"
              aria-pressed
              onClick={() => aoTrocarModo(item.valor)}
              className="flex items-center gap-1.5 rounded-full px-4 py-1.5 text-xs font-bold tracking-wider"
              style={{
                background: "rgba(0,229,204,0.12)",
                border: "1px solid #00e5cc",
                color: "#00e5cc",
              }}
            >
              <span className="h-1.5 w-1.5 rounded-full" style={{ background: "#00e5cc" }} />
              MODO: {item.abrev}
            </button>
          );
        }

        return (
          <button
            key={item.valor}
            type="button"
            aria-pressed={false}
            onClick={() => aoTrocarModo(item.valor)}
            className="rounded-full px-4 py-1.5 text-xs font-semibold tracking-wide transition-colors"
            style={{
              background: "#0d1f1e",
              border: "1px solid #1a3330",
              color: "#6b9690",
            }}
          >
            {item.abrev}
          </button>
        );
      })}
    </div>
  );
}
