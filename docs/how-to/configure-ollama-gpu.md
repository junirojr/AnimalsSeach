# Como Configurar o Ollama com GPU

Por padrão o Ollama roda os modelos na CPU. Habilitar a GPU reduz o tempo de geração de embeddings de ~1 s para ~50 ms por animal.

## Pré-requisitos

| GPU | Driver necessário |
|-----|------------------|
| NVIDIA | CUDA 12+ ([download](https://developer.nvidia.com/cuda-downloads)) |
| AMD (Linux) | ROCm 5.7+ |
| Apple Silicon | Metal — habilitado automaticamente no macOS |

## NVIDIA (Windows / Linux)

### Verificar se a GPU está visível para o Docker

```powershell
docker run --rm --gpus all nvidia/cuda:12.0-base-ubuntu20.04 nvidia-smi
```

Se aparecer a tabela com a GPU, o driver está correto.

### Habilitar GPU no docker-compose.yml

No serviço `ollama` do [docker-compose.yml](../../docker-compose.yml), adicione:

```yaml
services:
  ollama:
    image: ollama/ollama
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
    volumes:
      - ollama_data:/root/.ollama
    ports:
      - "11434:11434"
```

Suba novamente:

```bash
docker compose up -d ollama
```

### Confirmar que o modelo usa GPU

```bash
docker exec ollama ollama run nomic-embed-text "teste"
```

Na saída do `docker stats ollama` você deve ver uso de GPU > 0%.

## AMD (Linux com ROCm)

Substitua o trecho `deploy` por:

```yaml
devices:
  - /dev/kfd
  - /dev/dri
group_add:
  - video
```

E certifique-se de usar a imagem `ollama/ollama:rocm`.

## Apple Silicon (macOS)

Nenhuma configuração extra. O Ollama nativo para macOS usa Metal automaticamente. Não use a imagem Docker no macOS — instale o Ollama via:

```bash
brew install ollama
ollama serve &
ollama pull nomic-embed-text
```

Aponte `Ollama:BaseUrl` em `appsettings.Development.json` para `http://localhost:11434`.

## Verificar configuração atual

```bash
# dentro do container
docker exec ollama ollama ps
```

A saída mostra o modelo ativo e se está usando CPU ou GPU:

```
NAME                    ID              SIZE    PROCESSOR    UNTIL
nomic-embed-text:latest 0a109f422b47    986 MB  100% GPU     ...
```

## Variável de ambiente no projeto

A URL do Ollama é configurada em [backend/src/Buscador.Api/appsettings.Development.json](../../backend/src/Buscador.Api/appsettings.Development.json):

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434"
  }
}
```

Para apontar para um servidor Ollama remoto ou numa porta diferente, altere apenas esse valor — nenhuma mudança de código é necessária.
