"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";

import BarraBusca from "@/components/busca/BarraBusca";
import AlternadorModo from "@/components/busca/AlternadorModo";
import { CartaoAnimal } from "@/components/animais/CartaoAnimal";
import { DetalheAnimal } from "@/components/animais/DetalheAnimal";
import LogoAnimalsearch from "@/components/logo/LogoAnimalsearch";
import { buscarAnimais } from "@/services/animais";
import type { Animal, ModoBusca, ResultadoBusca } from "@/types/animal";

function SecaoHero() {
  return (
    <div className="flex flex-col items-center gap-6 px-4 pb-8 pt-10 text-center">
      <div
        className="flex items-center justify-center"
        style={{ animation: "girar-lento 30s linear infinite" }}
      >
        <LogoAnimalsearch size={112} />
      </div>

      <div>
        <h1 className="text-4xl font-bold tracking-tight text-white">Animalsearch</h1>
        <p className="mt-2 text-sm leading-relaxed" style={{ color: "#6b9690" }}>
          Busca híbrida sobre um catálogo de animais.<br />
          Textual, semântica e por similaridade vetorial.
        </p>
      </div>
    </div>
  );
}

export default function Home() {
  const [termo, setTermo] = useState("");
  const [modo, setModo] = useState<ModoBusca>("Hibrida");
  const [animalSelecionado, setAnimalSelecionado] = useState<Animal | null>(null);

  const termoLimpo = termo.trim();
  const habilitado = termoLimpo.length > 0;

  const { data, isLoading, isError, error } = useQuery<ResultadoBusca[]>({
    queryKey: ["busca", termoLimpo, modo],
    queryFn: () => buscarAnimais(termoLimpo, modo),
    enabled: habilitado,
  });

  const resultados = data ?? [];

  return (
    <>
      <div className="mx-auto w-full max-w-2xl px-4">
        {!habilitado && <SecaoHero />}

        <div className={`flex flex-col gap-3 ${habilitado ? "pt-6" : ""}`}>
          <BarraBusca aoBuscar={setTermo} />
          <AlternadorModo modo={modo} aoTrocarModo={setModo} />
        </div>

        {habilitado && (
          <div className="mt-6">
            {isLoading && (
              <p className="py-12 text-center text-sm" style={{ color: "#6b9690" }}>
                Buscando...
              </p>
            )}

            {isError && (
              <p className="py-12 text-center text-sm text-red-400">
                {(error as Error).message}
              </p>
            )}

            {!isLoading && !isError && (
              <>
                <div className="mb-4 px-1 text-xs" style={{ color: "#6b9690" }}>
                  <span className="font-mono uppercase tracking-widest">
                    {resultados.length} resultado{resultados.length !== 1 ? "s" : ""} encontrado{resultados.length !== 1 ? "s" : ""}
                  </span>
                </div>

                {resultados.length === 0 ? (
                  <p className="py-12 text-center text-sm" style={{ color: "#6b9690" }}>
                    Nenhum animal encontrado para &quot;{termo}&quot;.
                  </p>
                ) : (
                  <div className="flex flex-col gap-4">
                    {resultados.map((r) => (
                      <CartaoAnimal
                        key={r.animal.id}
                        animal={r.animal}
                        pontuacao={r.pontuacao}
                        aoClicar={setAnimalSelecionado}
                      />
                    ))}
                  </div>
                )}
              </>
            )}
          </div>
        )}
      </div>

      <DetalheAnimal
        animal={animalSelecionado}
        aoFechar={() => setAnimalSelecionado(null)}
      />
    </>
  );
}
