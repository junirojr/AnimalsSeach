# Frontend — Buscador (Deep Sparrow)

## Stack
- **Framework**: Next.js 15 (App Router)
- **Linguagem**: TypeScript
- **Estilos**: Tailwind CSS
- **Fetch / Cache**: TanStack Query (React Query)
- **Testes de componente**: Jest + React Testing Library + MSW
- **Testes E2E**: Playwright

## Estrutura
```
src/
  app/
    page.tsx          → página principal de busca
    providers.tsx     → TanStack Query provider
    layout.tsx
  components/
    search/
      SearchBar.tsx          → input com debounce (~400ms)
      SearchBar.test.tsx
      SearchModeToggle.tsx   → toggle fulltext | semantic | hybrid
    animals/
      AnimalCard.tsx         → card resumido (nome, habitat, dieta, tags)
      AnimalCard.test.tsx
      AnimalDetail.tsx       → modal/drawer com descrição e curiosidades
  services/
    animals.ts        → funções de fetch: searchAnimals(), getAnimal()
  types/
    animal.ts         → Animal, SearchResult, SearchMode
e2e/
  search.spec.ts      → Playwright: digitar, ver resultados, trocar modo
```

## Convenções
- Componentes em PascalCase, co-localizados com seus testes (`*.test.tsx`)
- `NEXT_PUBLIC_API_URL` em `.env.local` aponta para o backend local
- Chamadas HTTP sempre via TanStack Query (não `fetch` direto nos componentes)
- MSW intercepta chamadas nos testes de componente (não depende do backend)

## Comandos
```bash
npm run dev      # dev server em localhost:3000
npm run build    # build de produção
npm test         # Jest + RTL
npx playwright test  # E2E (requer backend + frontend rodando)
```
