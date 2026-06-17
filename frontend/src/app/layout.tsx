import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { Provedores } from "./provedores";
import NavegacaoTopo from "@/components/layout/NavegacaoTopo";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Animalsearch — Buscador de Animais",
  description: "Busca hibrida (textual, semantica e hibrida) sobre um catalogo de animais",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="pt-BR"
      className={`${geistSans.variable} ${geistMono.variable} h-full`}
    >
      <body className="min-h-full" style={{ background: "#091716" }}>
        <Provedores>
          <NavegacaoTopo />
          <main className="pt-14 pb-6">
            {children}
          </main>
        </Provedores>
      </body>
    </html>
  );
}
