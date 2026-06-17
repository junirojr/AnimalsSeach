"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";

export function Provedores({ children }: { children: React.ReactNode }) {
  const [clienteQuery] = useState(() => new QueryClient());

  return (
    <QueryClientProvider client={clienteQuery}>{children}</QueryClientProvider>
  );
}
