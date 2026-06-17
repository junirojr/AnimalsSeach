import type { KeyboardEvent } from "react";
import type { Animal } from "@/types/animal";
import { rotulosDieta, rotulosHabitat } from "@/types/animal";

interface CartaoAnimalProps {
  animal: Animal;
  pontuacao?: number;
  aoClicar?: (animal: Animal) => void;
}

const gradientesHabitat: Record<string, [string, string]> = {
  Floresta: ["#0a3020", "#1a5a35"],
  Oceano: ["#041830", "#0a3a5a"],
  Deserto: ["#301a04", "#5a3010"],
  Savana: ["#2e2004", "#4a3508"],
  Montanha: ["#181828", "#2e2e48"],
  AguaDoce: ["#041e2e", "#0a3848"],
  Polar: ["#182038", "#28385a"],
};

function formatarPontuacao(pontuacao: number): string {
  if (pontuacao >= 0 && pontuacao <= 1) {
    return `${Math.round(pontuacao * 100)}%`;
  }

  return pontuacao.toFixed(2);
}

export function CartaoAnimal({ animal, pontuacao, aoClicar }: CartaoAnimalProps) {
  const ehClicavel = aoClicar !== undefined;
  const exibirPontuacao = pontuacao !== undefined;

  const rotuloHabitat = rotulosHabitat[animal.habitat] ?? "—";
  const rotuloDieta = rotulosDieta[animal.dieta] ?? "—";
  const [corA, corB] = gradientesHabitat[animal.habitat] ?? ["#0d1f1e", "#162c2a"];
  const [genero] = animal.nomeCientifico.split(" ");

  function aoPressionarTecla(evento: KeyboardEvent<HTMLDivElement>) {
    if (!ehClicavel || (evento.key !== "Enter" && evento.key !== " ")) return;
    evento.preventDefault();
    aoClicar?.(animal);
  }

  return (
    <div
      className={`overflow-hidden rounded-2xl transition-transform ${ehClicavel ? "cursor-pointer hover:-translate-y-0.5 focus:outline-none focus:ring-2 focus:ring-[#00e5cc]" : ""}`}
      style={{ background: "#0d1f1e", border: "1px solid #1a3330" }}
      role={ehClicavel ? "button" : undefined}
      tabIndex={ehClicavel ? 0 : undefined}
      onClick={() => aoClicar?.(animal)}
      onKeyDown={aoPressionarTecla}
    >
      {/* Área visual com gradiente por habitat */}
      <div
        className="relative flex h-44 items-end overflow-hidden"
        style={{ background: `linear-gradient(135deg, ${corA} 0%, ${corB} 100%)` }}
      >
        <span
          className="pointer-events-none absolute inset-0 flex select-none items-center justify-center font-bold italic"
          style={{ color: "rgba(255,255,255,0.04)", fontSize: "90px", lineHeight: 1 }}
          aria-hidden
        >
          {genero}
        </span>

        {exibirPontuacao && (
          <span
            className="absolute right-3 top-3 rounded-full px-2.5 py-1 text-xs font-bold"
            style={{ background: "#00e5cc", color: "#091716" }}
          >
            {formatarPontuacao(pontuacao)} Match
          </span>
        )}
      </div>

      {/* Conteúdo */}
      <div className="p-4">
        <div>
          <h3 className="truncate text-base font-bold text-white">
            {animal.nomeComum}
          </h3>
          <p className="truncate text-xs italic" style={{ color: "#6b9690" }}>
            {animal.nomeCientifico}
          </p>
        </div>

        <div className="mt-3 flex flex-wrap gap-2">
          <span
            className="rounded-full px-2.5 py-0.5 font-mono text-[10px] font-semibold uppercase tracking-wider"
            style={{
              background: "rgba(0,229,204,0.08)",
              border: "1px solid rgba(0,229,204,0.2)",
              color: "#00e5cc",
            }}
          >
            Habitat: {rotuloHabitat}
          </span>
          <span
            className="rounded-full px-2.5 py-0.5 font-mono text-[10px] font-semibold uppercase tracking-wider"
            style={{
              background: "rgba(251,191,36,0.08)",
              border: "1px solid rgba(251,191,36,0.2)",
              color: "#fbbf24",
            }}
          >
            Dieta: {rotuloDieta}
          </span>
        </div>
      </div>
    </div>
  );
}
