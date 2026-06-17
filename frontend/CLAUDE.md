# Frontend — Buscador (Animalsearch)

## Stack
- **Framework**: Next.js 16 (App Router)
- **Linguagem**: TypeScript
- **Estilos**: Tailwind CSS v4
- **Fetch / Cache**: TanStack Query (React Query v5)
- **Testes de componente**: Jest + React Testing Library + MSW
- **Testes E2E**: Playwright

## Estrutura
```
src/
  app/
    page.tsx           → página principal de busca (integra tudo, useQuery)
    provedores.tsx     → 'use client', QueryClientProvider (Provedores)
    layout.tsx         → envolve children com <Provedores>
  components/
    busca/
      BarraBusca.tsx          → input com debounce (~400ms), chama aoBuscar
      BarraBusca.test.tsx
      AlternadorModo.tsx      → segmented Textual | Semantica | Hibrida
    animais/
      CartaoAnimal.tsx        → card resumido (nome, habitat, dieta, tags, pontuacao)
      CartaoAnimal.test.tsx
      DetalheAnimal.tsx       → modal com descricao, curiosidades, status etc.
  services/
    animais.ts         → funções de fetch: buscarAnimais(), obterAnimal()
  types/
    animal.ts          → Animal, ResultadoBusca, ModoBusca + mapas dos enums
                         (rotulosDieta, rotulosHabitat, rotulosStatusConservacao)
tests/
  mocks/
    handlers.ts        → MSW: mock de GET /api/animais/buscar
    server.ts          → setupServer(...handlers)
e2e/
  busca.spec.ts        → Playwright: digitar, ver resultados, trocar modo
jest.config.ts         → Jest via next/jest (jsdom) + override de transformIgnorePatterns (ESM do MSW)
jest.polyfills.ts      → polyfills de Web APIs (undici) p/ MSW v2 em jsdom
jest.setup.ts          → jest-dom + ciclo de vida do MSW server
playwright.config.ts   → testDir ./e2e, baseURL :3000, webServer (npm run dev)
```

## Convenções
- **Código, funções, variáveis e nomes de arquivo em PORTUGUÊS SEM ACENTO**
  (ex.: `BarraBusca.tsx`, `CartaoAnimal.tsx`, `buscarAnimais()`, `AlternadorModo`).
  Arquivos-convenção do Next mantêm o nome padrão (`page.tsx`, `layout.tsx`).
- Componentes em PascalCase, co-localizados com seus testes (`*.test.tsx`)
- Enums do backend chegam como **números**; converter via os mapas em `types/animal.ts`
  (ordem lida dos enums em `backend/src/Buscador.Domain/Animais/`)
- `NEXT_PUBLIC_API_URL` em `.env.local` aponta para o backend local (`http://localhost:5024`)
- Estado de busca (termo + modo) é **elevado** para `page.tsx`; os componentes recebem props/callbacks
- Chamadas HTTP via TanStack Query na página (`useQuery`) → serviço `animais.ts` (não `fetch` solto nos componentes)
- MSW intercepta chamadas nos testes de componente (não depende do backend)

## Comandos
```bash
npm run dev          # dev server em localhost:3000
npm run build        # build de produção
npm test             # Jest + RTL + MSW (não precisa do backend)
npx playwright test  # E2E (requer backend semeado + frontend rodando)
```
