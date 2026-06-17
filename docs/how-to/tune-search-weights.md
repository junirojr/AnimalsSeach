# Como Ajustar o Comportamento da Busca Hibrida

Este guia cobre os parametros que controlam o algoritmo RRF usado pela busca hibrida, onde
cada um esta no codigo, o que cada um afeta, e como testar o efeito de uma mudanca.

Para entender *por que* o RRF funciona dessa forma, consulte
`docs/explanation/rrf-algorithm.md`.

---

## O que e possivel ajustar

Ha dois parametros que controlam o comportamento da busca hibrida. Ambos sao valores hardcoded
no codigo — nao existem variaveis de ambiente, arquivos de configuracao ou feature flags para
eles. Para alterar, e necessario editar o codigo e recompilar.

| Parametro      | Valor atual | Arquivo                                                                 | O que controla                                      |
|----------------|-------------|-------------------------------------------------------------------------|-----------------------------------------------------|
| `K` (constante RRF) | `60`   | `Buscador.Application/Compartilhado/FusaoRrf.cs`                       | Suavizacao da diferenca entre posicoes              |
| Pool minimo    | `20`        | `Buscador.Infrastructure/Busca/ServicoBuscaHibrida.cs`                 | Candidatos por lado antes de aplicar o RRF          |

---

## Como ajustar a constante K

### Localizacao

Arquivo: `backend/src/Buscador.Application/Compartilhado/FusaoRrf.cs`

```csharp
public const int K = 60; // valor classico do paper de RRF
```

### O que K controla

`K` suaviza a diferenca de score entre posicoes proximas do topo. O score de cada posicao e
`1 / (K + posicao)`:

| Posicao | K = 30         | K = 60         | K = 100        |
|---------|----------------|----------------|----------------|
| 1       | 1/31 ≈ 0.0323  | 1/61 ≈ 0.0164  | 1/101 ≈ 0.0099 |
| 2       | 1/32 ≈ 0.0313  | 1/62 ≈ 0.0161  | 1/102 ≈ 0.0098 |
| 5       | 1/35 ≈ 0.0286  | 1/65 ≈ 0.0154  | 1/105 ≈ 0.0095 |
| 10      | 1/40 ≈ 0.0250  | 1/70 ≈ 0.0143  | 1/110 ≈ 0.0091 |

**K menor (ex: 30)**: a diferenca entre 1o e 2o lugar e maior. O algoritmo privilegia mais
fortemente quem ficou no topo de uma das listas, mesmo sem aparecer na outra.

**K maior (ex: 100)**: posicoes proximas do topo ficam quase equivalentes. O algoritmo so
diferencia resultados quando ha uma distancia grande de posicao entre eles.

### Valores tipicos

- `30`: mais agressivo — topo de lista vale muito mais que as posicoes seguintes
- `60`: valor classico do paper original (Cormack et al., 2009) — comportamento balanceado
- `100`: mais conservador — posicoes proximas sao tratadas como quase equivalentes

### Como editar

```csharp
// FusaoRrf.cs — linha atual:
public const int K = 60;

// Para testar com K menor (topo pesa mais):
public const int K = 30;

// Para testar com K maior (posicoes proximas se equivalem mais):
public const int K = 100;
```

---

## Como ajustar o pool minimo de candidatos

### Localizacao

Arquivo: `backend/src/Buscador.Infrastructure/Busca/ServicoBuscaHibrida.cs`

```csharp
var tamanhoPool = Math.Max(limite, 20);
```

### O que o pool controla

Antes de aplicar o RRF, cada sistema de busca (textual e semantico) retorna um conjunto de
candidatos. O pool define quantos candidatos cada lado entrega para o algoritmo de fusao.

O valor atual garante que o pool nunca seja menor que 20, mesmo que o usuario peca apenas
`limite=3`. Isso e necessario porque o RRF funciona melhor quando tem mais candidatos para
comparar — um pool pequeno pode fazer o algoritmo perder resultados relevantes que apareceriam
na lista semantica mas nao na textual (ou vice-versa).

**Pool maior**: mais candidatos por lado entram no RRF. O resultado final tem mais chances de
incluir resultados que aparecem em posicoes intermediarias em ambas as listas. Mais lento.

**Pool menor**: menos candidatos, mais rapido, mas pode perder resultados relevantes que
ficam alem da posicao do pool em uma das listas.

### Como editar

```csharp
// ServicoBuscaHibrida.cs — linha atual:
var tamanhoPool = Math.Max(limite, 20);

// Para aumentar o pool minimo (melhor cobertura, mais lento):
var tamanhoPool = Math.Max(limite, 40);

// Para um pool muito grande (alta qualidade, notavelmente mais lento):
var tamanhoPool = Math.Max(limite, 50);
```

### Quando considerar aumentar

- A busca hibrida esta perdendo animais que aparecem claramente em um dos modos individuais
- O catalogo cresceu muito e os resultados relevantes estao alem das primeiras 20 posicoes
- A latencia nao e uma restricao (ambiente de desenvolvimento ou carga baixa)

---

## Como testar o efeito de uma mudanca

### Passo 1: edite o valor desejado

Escolha qual parametro alterar (`K` ou pool minimo) e edite o arquivo correspondente conforme
as instrucoes acima.

### Passo 2: recompile o backend

```bash
cd backend
dotnet build
```

Corrija eventuais erros de compilacao antes de continuar.

### Passo 3: suba a infraestrutura e o servidor

```bash
# Na raiz do projeto
docker compose up -d

# Em backend/
dotnet run --project src/Buscador.Api
```

### Passo 4: teste com o endpoint

```bash
# Substitua o termo pela consulta que voce quer comparar
curl "http://localhost:5024/api/animais/buscar?q=predador&modo=Hibrida&limite=10"
```

Compare a ordem dos resultados com o valor anterior. Anote as posicoes dos animais que voce
esperava ver no topo.

### Passo 5: compare os modos individuais (opcional)

Para isolar se a mudanca no RRF ajudou, compare com os modos individuais:

```bash
# O que a busca textual retorna sozinha
curl "http://localhost:5024/api/animais/buscar?q=predador&modo=Textual&limite=10"

# O que a busca semantica retorna sozinha
curl "http://localhost:5024/api/animais/buscar?q=predador&modo=Semantica&limite=10"

# O que a fusao produz com os novos valores
curl "http://localhost:5024/api/animais/buscar?q=predador&modo=Hibrida&limite=10"
```

Se um animal relevante aparece em 1o lugar no modo Textual mas some no modo Hibrida, isso pode
indicar que o pool minimo esta pequeno para aquela consulta.

---

## O que nao existe atualmente

Nao ha suporte para:

- Variaveis de ambiente para `K` ou o pool minimo
- Secao no `appsettings.json` para esses valores
- Pesos diferentes por modo (ex: dar mais importancia ao lado semantico)
- Feature flags para alternar o comportamento em producao sem recompilar

Para qualquer ajuste nesses parametros, o fluxo e: **editar o codigo → recompilar → reiniciar
o servidor**.
