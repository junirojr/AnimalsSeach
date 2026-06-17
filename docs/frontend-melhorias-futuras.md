# Frontend — Melhorias Futuras

Itens removidos por YAGNI (sem implementação real, especulativos) e registrados aqui para implementação futura.

---

## Navegação multi-seção (Explorar / Biblioteca / Salvos) + barra inferior

**O que era:** `NavegacaoInferior.tsx` com 4 abas (Buscar, Explorar, Biblioteca, Salvos) fixada no fundo da tela.

**Por que faria sentido:** Quando o app evoluir para múltiplas seções (exploração por categoria, biblioteca pessoal, salvos), a barra inferior é o padrão mobile consolidado.

**Pista técnica:** Recriar com Next.js App Router (`layout.tsx`), estado de aba ativo via `usePathname()`, links com `<Link href="/explorar">`. Avaliar `react-aria` para acessibilidade.

---

## Menu lateral e Perfil de usuário (barra superior)

**O que era:** Botão hambúrguer (Menu) e botão circular de Perfil em `NavegacaoTopo.tsx`, sem ação ao clicar.

**Por que faria sentido:** Um menu lateral pode conter configurações de busca, filtros salvos e preferências. O perfil pode exibir histórico de buscas e animais salvos.

**Pista técnica:** `<Sheet>` / drawer lateral com `radix-ui/react-dialog`. Auth com NextAuth.js ou Clerk.

---

## Busca por voz (microfone na barra de busca)

**O que era:** Botão de microfone em `BarraBusca.tsx`, sem ação ao clicar.

**Por que faria sentido:** Melhora a acessibilidade e UX mobile. Usuário fala o nome do animal e o campo é preenchido automaticamente.

**Pista técnica:** `window.SpeechRecognition` (Web Speech API). Fallback gracioso quando não suportado (ocultar botão). Tratar `onresult` para setar o termo e disparar a busca.

---

## Salvar / favoritar animais (bookmark + persistência)

**O que era:** Botão bookmark (Salvar) no cabeçalho de cada `CartaoAnimal.tsx`, sem ação real.

**Por que faria sentido:** Permite ao usuário montar uma coleção pessoal de animais de interesse.

**Pista técnica:** Persistir em `localStorage` (offline-first simples) ou em tabela `FavoritosUsuario` no backend (requer autenticação). Gerenciar estado global com Zustand ou Context.

---

## Virtualização da lista para grandes volumes

**O que é:** Renderização virtual das linhas de resultados para suportar catálogos grandes sem degradar performance.

**Por que faria sentido:** Com ~1000 animais no catálogo, renderizar todos os cartões simultaneamente causa jank perceptível no scroll.

**Pista técnica:** [`@tanstack/react-virtual`](https://tanstack.com/virtual) com `useVirtualizer`. Medir primeiro com React DevTools Profiler para confirmar que é gargalo.

---

## Tokens de cor (paleta em variáveis CSS)

**O que é:** Extrair as cores hardcoded (`#091716`, `#0d1f1e`, `#1a3330`, `#00e5cc`, `#6b9690`, `#fbbf24`, `#f87171`, `#b8d4d0`, `#122624`) para variáveis CSS no `globals.css` via `@theme inline` do Tailwind v4, substituindo os `style={{ background: "#..." }}` por classes Tailwind.

**Por que faria sentido:** Facilita theming, dark/light mode e manutenção da paleta em um único lugar.

**Pista técnica:** Definir em `globals.css`:
```css
@theme inline {
  --color-surface: #0d1f1e;
  --color-border:  #1a3330;
  --color-accent:  #00e5cc;
  --color-muted:   #6b9690;
  /* … */
}
```
Depois trocar `style={{ background: "#0d1f1e" }}` por `className="bg-surface"` etc.
Commit sugerido: `refactor(front): paleta em tokens de cor`.
