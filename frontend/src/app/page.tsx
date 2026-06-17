"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";

import BarraBusca from "@/components/busca/BarraBusca";
import AlternadorModo from "@/components/busca/AlternadorModo";
import { CartaoAnimal } from "@/components/animais/CartaoAnimal";
import { DetalheAnimal } from "@/components/animais/DetalheAnimal";
import { buscarAnimais } from "@/services/animais";
import type { Animal, ModoBusca, ResultadoBusca } from "@/types/animal";

export default function Home() {
  const [termo, setTermo] = useState("");
  const [modo, setModo] = useState<ModoBusca>("Textual");
  const [animalSelecionado, setAnimalSelecionado] = useState<Animal | null>(null);

  const termoLimpo = termo.trim();
  const habilitado = termoLimpo.length > 0;

  const { data, isLoading, isError, error } = useQuery<ResultadoBusca[]>({
    queryKey: ["busca", termo, modo],
    queryFn: () => buscarAnimais(termo, modo),
    enabled: habilitado,
  });

  function renderizarResultados() {
    if (!habilitado) {
      return (
        <p className="text-center text-zinc-500 dark:text-zinc-400">
          Digite algo para comecar a buscar.
        </p>
      );
    }

    if (isLoading) {
      return (
        <p className="text-center text-zinc-500 dark:text-zinc-400">
          Buscando...
        </p>
      );
    }

    if (isError) {
      return (
        <p className="text-center text-red-600 dark:text-red-400">
          {(error as Error).message}
        </p>
      );
    }

    const resultados = data ?? [];

    if (resultados.length === 0) {
      return (
        <p className="text-center text-zinc-500 dark:text-zinc-400">
          Nenhum animal encontrado para &quot;{termo}&quot;.
        </p>
      );
    }

    return (
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {resultados.map((r) => (
          <CartaoAnimal
            key={r.animal.id}
            animal={r.animal}
            pontuacao={r.pontuacao}
            aoClicar={setAnimalSelecionado}
          />
        ))}
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-black">
      <main className="mx-auto flex w-full max-w-5xl flex-col gap-8 px-4 py-12 sm:px-6 lg:px-8">
        <header className="flex flex-col items-center gap-2 text-center">
          <h1 className="text-4xl font-bold tracking-tight text-zinc-900 dark:text-zinc-50">
            Deep Sparrow
          </h1>
          <p className="text-lg text-zinc-600 dark:text-zinc-400">
            Busca híbrida de animais
          </p>
        </header>

        <section className="flex flex-col items-center gap-4">
          <BarraBusca aoBuscar={setTermo} />
          <AlternadorModo modo={modo} aoTrocarModo={setModo} />
        </section>

        <section>{renderizarResultados()}</section>
      </main>

      <DetalheAnimal
        animal={animalSelecionado}
        aoFechar={() => setAnimalSelecionado(null)}
      />
    </div>
  );
}
