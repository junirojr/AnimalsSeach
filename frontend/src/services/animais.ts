import type { Animal, ModoBusca, ResultadoBusca } from "@/types/animal";

const baseUrl = process.env.NEXT_PUBLIC_API_URL;

export async function buscarAnimais(
  q: string,
  modo: ModoBusca,
  limite = 10,
): Promise<ResultadoBusca[]> {
  const url = `${baseUrl}/api/animais/buscar?q=${encodeURIComponent(q)}&modo=${modo}&limite=${limite}`;
  const resp = await fetch(url);

  if (!resp.ok) {
    throw new Error(`Falha na busca (HTTP ${resp.status})`);
  }

  return resp.json();
}

export async function obterAnimal(id: string): Promise<Animal | null> {
  const resp = await fetch(`${baseUrl}/api/animais/${id}`);

  if (resp.status === 404) {
    return null;
  }

  if (!resp.ok) {
    throw new Error(`Falha ao obter animal (HTTP ${resp.status})`);
  }

  return resp.json();
}
