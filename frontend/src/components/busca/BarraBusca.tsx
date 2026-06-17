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
    <div className="relative flex w-full items-center">
      <span className="pointer-events-none absolute left-4" style={{ color: "#6b9690" }}>
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <circle cx="11" cy="11" r="8" />
          <path d="m21 21-4.35-4.35" />
        </svg>
      </span>

      <input
        type="text"
        value={termo}
        onChange={aoDigitar}
        placeholder={placeholder}
        aria-label="Buscar animais"
        className="w-full rounded-2xl border border-[#1a3330] py-3.5 pl-11 pr-4 text-sm text-white outline-none transition-all focus:border-[#00e5cc] focus:shadow-[0_0_0_1px_rgba(0,229,204,0.15)]"
        style={{
          background: "#0d1f1e",
          caretColor: "#00e5cc",
        }}
      />
    </div>
  );
}
