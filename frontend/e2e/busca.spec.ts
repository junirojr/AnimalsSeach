import { test, expect } from "@playwright/test";

test.describe("Busca de animais", () => {
  test("permite buscar e trocar de modo vendo resultados", async ({ page }) => {
    await page.goto("/");

    await expect(
      page.getByRole("heading", { name: "Deep Sparrow" }),
    ).toBeVisible();

    const campoBusca = page.getByLabel("Buscar animais");
    await campoBusca.fill("felino");

    // Aguarda o primeiro card de resultado aparecer (apos o debounce + fetch).
    const primeiroResultado = page.getByRole("heading", { level: 3 }).first();
    await expect(primeiroResultado).toBeVisible({ timeout: 15_000 });

    // Troca para o modo Semantica e confirma que ainda ha resultados.
    await page.getByRole("button", { name: "Semântica" }).click();
    await expect(
      page.getByRole("heading", { level: 3 }).first(),
    ).toBeVisible({ timeout: 15_000 });

    // Troca para o modo Hibrida e confirma resultados novamente.
    await page.getByRole("button", { name: "Híbrida" }).click();
    await expect(
      page.getByRole("heading", { level: 3 }).first(),
    ).toBeVisible({ timeout: 15_000 });
  });
});
