# Referência — Endpoints da API

Base URL (desenvolvimento): `http://localhost:5000`

---

## POST /api/animais/popular

Popula o banco com os animais da semente (`DadosSementeAnimal`). Use para inicializar os dados em um ambiente limpo.

**Parâmetros:** nenhum.

**Resposta 200 OK:**
```json
{
  "inseridos": 10
}
```

---

## POST /api/animais/embeddings/gerar

Gera e persiste os embeddings vetoriais (bge-m3, 1024 dim) para todos os animais que ainda não possuem embeddings. Necessário para a busca semântica e híbrida.

**Parâmetros:** nenhum.

**Resposta 200 OK:**
```json
{
  "processados": 10
}
```

---

## GET /api/animais

Lista os animais com paginação.

**Parâmetros query:**

| Parâmetro | Tipo | Padrão | Descrição |
|-----------|------|--------|-----------|
| `pagina`  | int  | 1      | Número da página (começa em 1) |
| `tamanho` | int  | 20     | Itens por página |

**Resposta 200 OK:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nomeComum": "Leão",
    "nomeCientifico": "Panthera leo",
    "descricao": "O leão é o felino mais social...",
    "dieta": "Carnivoro",
    "habitat": "Savana",
    "statusConservacao": "Vulneravel"
  }
]
```

---

## GET /api/animais/{id}

Retorna os dados de um animal pelo seu identificador único.

**Parâmetros rota:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id`      | guid | Identificador do animal |

**Resposta 200 OK:** objeto animal (mesmo formato de GET /api/animais).

**Resposta 404 Not Found:** quando o ID não existe no banco.

---

## GET /api/animais/buscar

Realiza busca full-text, semântica ou híbrida sobre o catálogo de animais.

**Parâmetros query:**

| Parâmetro | Tipo       | Padrão    | Descrição |
|-----------|------------|-----------|-----------|
| `q`       | string     | —         | Termo de busca (obrigatório, não pode ser vazio) |
| `modo`    | ModoBusca  | `Textual` | Modo de busca: `Textual`, `Semantica` ou `Hibrida` |
| `limite`  | int        | 10        | Número máximo de resultados |

**Resposta 200 OK:**
```json
[
  {
    "animal": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nomeComum": "Leão",
      "nomeCientifico": "Panthera leo"
    },
    "pontuacao": 0.87
  }
]
```

**Resposta 400 Bad Request:** quando `q` está vazio ou em branco.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Q": ["'Q' must not be empty."]
  }
}
```
