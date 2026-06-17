# Por que RRF (Reciprocal Rank Fusion) para Combinar Rankings?

O documento `architecture-decisions.md` (secao 5) descreve *o que* o RRF faz e mostra sua
implementacao no codigo. Este documento aprofunda o raciocinio por tras da escolha: por que RRF
e nao uma alternativa mais simples, quais propriedades matematicas o tornam robusto, e o que
cada parametro significa na pratica.

---

## O problema que motivou o RRF

O buscador combina dois sistemas que falam linguas numericas diferentes:

- A busca textual retorna `ts_rank`, que e um score sem unidade definida. Ele cresce conforme o
  termo aparece mais vezes no documento. Um animal cujo nome repete o termo buscado em varios
  campos pode ter `ts_rank = 0.9`; outro com o termo apenas na descricao pode ter `ts_rank = 0.06`.
  Nao ha teto natural.

- A busca semantica retorna `1 - distancia_coseno`, que e matematicamente delimitada entre `0`
  e `1`. Um valor de `0.92` indica alta similaridade; abaixo de `0.5` ja e considerado fraco.

Se voce calcular a media aritmetica de `ts_rank = 0.9` com `similaridade = 0.3`, obtem `0.6`.
Isso e maior do que a media de `ts_rank = 0.3` com `similaridade = 0.95`, que da `0.625`. O
segundo animal e claramente mais relevante para uma busca semantica, mas a media simples quase
empata os dois — e poderia inverter a ordem dependendo dos valores exatos.

A raiz do problema e que os dois scores habitam escalas incompativeis. Qualquer operacao que os
combine diretamente (media, soma ponderada) esta comparando grandezas diferentes.

---

## A ideia central: usar posicao, nao pontuacao

O RRF resolve o problema descartando completamente os scores brutos. Em vez de perguntar
"qual e o score deste animal?", ele pergunta: "em que posicao este animal aparece em cada lista?"

Isso transforma o problema: posicoes sao sempre comparaveis. O primeiro colocado na lista
textual e o primeiro colocado na lista semantica estao na mesma "escala" — ambos sao o melhor
resultado do seu respectivo sistema, independentemente de quanto vale o score por tras.

A formula e:

```
score_rrf(animal) = soma de  1 / (k + posicao_na_lista_i)
```

Onde:
- `posicao_na_lista_i` e a posicao do animal na lista `i` (comecando em 1, nao em 0)
- `k` e uma constante de suavizacao (valor classico: 60, do paper de Cormack et al., 2009)
- A soma percorre todas as listas onde o animal aparece

---

## Exemplo numerico

Considere dois animais, A e B, retornados pelas buscas textual e semantica:

| Animal | Posicao na lista textual | Posicao na lista semantica |
|--------|--------------------------|---------------------------|
| A      | 1                        | 1                         |
| B      | 1                        | ausente                   |

Aplicando a formula com `k = 60`:

- **Animal A**: `1/(60+1) + 1/(60+1) = 0.01639 + 0.01639 ≈ 0.0328`
- **Animal B**: `1/(60+1) = 0.01639 ≈ 0.0164`

O animal A, que foi primeiro lugar em ambas as listas, recebe exatamente o dobro do score do
animal B, que foi primeiro lugar em apenas uma. Isso e intuitivo: consenso entre os dois sistemas
e um sinal mais forte de relevancia do que um bom resultado isolado.

Agora imagine um terceiro animal C que aparece em 5o lugar em ambas as listas:

- **Animal C**: `1/(60+5) + 1/(60+5) = 0.01538 + 0.01538 ≈ 0.0308`

Mesmo aparecendo em 5o lugar em ambas, C tem score parecido com A (0.0308 vs 0.0328), mas
inferior. O consenso entre os sistemas ainda e valorizado, mas posicoes melhores pesam mais.

---

## Por que k = 60

O valor `k = 60` vem do paper original de Cormack, Clarke e Buettcher (2009), que o estabeleceu
empiricamente como o valor que maximiza a qualidade do ranking fundido em colecoes de documentos
de busca academica.

O papel matematico de `k` e suavizar a diferenca entre posicoes proximas do topo. Veja o efeito:

| Posicao | Score com k=60 | Score com k=10 |
|---------|---------------|---------------|
| 1       | 1/61 ≈ 0.0164 | 1/11 ≈ 0.0909 |
| 2       | 1/62 ≈ 0.0161 | 1/12 ≈ 0.0833 |
| 10      | 1/70 ≈ 0.0143 | 1/20 ≈ 0.0500 |
| 20      | 1/80 ≈ 0.0125 | 1/30 ≈ 0.0333 |

Com `k = 10`, a diferenca entre o 1o e o 2o lugar e grande (0.0909 vs 0.0833 — quase 9% de
diferenca relativa). Com `k = 60`, essa diferenca cai para menos de 2%. Isso significa que um
animal em 2o lugar perde pouco para o 1o — posicoes proximas sao tratadas como quase equivalentes.

Para o projeto Buscador, onde a relevancia entre resultados proximos do topo e genuinamente
parecida, essa suavizacao e desejavel. Um `k` menor tornaria o sistema mais sensivel a pequenas
diferencas de posicao, o que poderia amplificar ruido em vez de sinal.

---

## Robustez a outliers

Esta e, talvez, a propriedade mais importante do RRF em contextos hibridos.

Imagine que um animal tem o termo buscado repetido 40 vezes em suas tags (um caso extremo de
keyword stuffing). A busca textual pode retornar um `ts_rank` muito alto para ele — digamos,
`ts_rank = 3.5`, muito acima dos outros resultados. Em uma media ponderada, esse valor inflaria
o score hibrido de forma desproporcional.

No RRF, esse animal simplesmente "ocupa o 1o lugar" na lista textual. Nao importa se o ts_rank
era 3.5 ou 0.5 — o RRF ve apenas a posicao. Se a busca semantica nao o considera relevante, ele
nao aparece na lista semantica, e seu score RRF final e apenas `1/61 ≈ 0.0164` — o mesmo que
qualquer outro 1o lugar exclusivo. O outlier nao contamina o resultado hibrido.

---

## Normalizacao final

Apos calcular os scores RRF, `FusaoRrf` normaliza o resultado dividindo todos os scores pelo
maior valor encontrado. O animal de maior score recebe `1.0`; os demais recebem valores
proporcionais entre `0` e `1`.

Essa normalizacao e puramente de apresentacao — nao altera a ordem dos resultados, apenas torna
o campo `pontuacao` intuitivo para quem consome a API. Um score de `0.5` significa "metade do
consenso do melhor resultado encontrado".

---

## Onde esta implementado

- `FusaoRrf.cs` em `Buscador.Application/Compartilhado/`: funcao pura estatica, sem dependencias
  externas. Recebe as listas ranqueadas, aplica a formula e retorna os resultados normalizados.

- `ServicoBuscaHibrida.cs` em `Buscador.Infrastructure/Busca/`: orquestra as duas buscas e
  chama `FusaoRrf.Fundir`. Usa um pool minimo de 20 candidatos por lado para garantir que o
  algoritmo tenha material suficiente de ambos os sistemas antes de cortar no limite final.

O fato de `FusaoRrf` ser uma classe de funcao pura em `Application` (sem acesso a banco ou
infraestrutura) torna a logica de fusao testavel de forma isolada, sem containers ou banco de dados.
