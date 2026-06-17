"use client";

import { useEffect, useState } from "react";

interface BarraBuscaProps {
  aoBuscar: (termo: string) => void;
  valorInicial?: string;
  placeholder?: string;
}

export default function BarraBusca({
  aoBuscar,
  valorInicial = "",
  placeholder = "Buscar animais...",
}: BarraBuscaProps) {
  const [termo, setTermo] = useState(valorInicial);

  useEffect(() => {
    const temporizador = setTimeout(() => {
      aoBuscar(termo);
    }, 400);

    return () => clearTimeout(temporizador);
    // O debounce dispara a partir das mudancas no texto digitado.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [termo]);

  function aoDigitar(evento: React.ChangeEvent<HTMLInputElement>) {
    setTermo(evento.target.value);
  }

  return (
    <input
      type="text"
      value={termo}
      onChange={aoDigitar}
      placeholder={placeholder}
      aria-label="Buscar animais"
      className="w-full rounded-lg border border-gray-300 px-4 py-3 text-base outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-500"
    />
  );
}
