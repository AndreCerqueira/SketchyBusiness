# Sketchy Business - Backend IA (Instruções de Configuração)

Este projeto utiliza um servidor local em Python para gerir a Inteligência Artificial do jogo (geração de imagens, análise de desenhos e julgamento das rondas). Para que o jogo funcione na totalidade, precisas de ter este servidor a correr em pano de fundo.

Segue os passos abaixo para configurar o teu ambiente.

## Pré-requisitos

### Python 3.8+
Certifica-te de que tens o Python instalado. Podes descarregar em python.org.

> Durante a instalação (no Windows), não te esqueças de marcar a opção **Add Python to PATH**.

### Ollama
A IA de visão utiliza o Ollama para correr o modelo **LLaVA** localmente na tua máquina.

Descarrega e instala a partir de ollama.com.

Após a instalação, abre um terminal (Linha de Comandos/PowerShell) e executa o seguinte comando para descarregar o modelo necessário:

```bash
ollama run llava
```

> **Nota:** O download do modelo pode demorar alguns minutos dependendo da tua ligação à internet (cerca de **4.5 GB**).

---

## Configurar o Servidor (Python VENV)

Abre o terminal e navega até à pasta onde se encontra o código do servidor Python (onde está a diretoria `app/`).

### 1. Criar o Ambiente Virtual

O ambiente virtual isola as dependências deste projeto para não interferir com outros projetos Python no teu computador.

**Windows:**

```bash
python -m venv venv
```

**macOS/Linux:**

```bash
python3 -m venv venv
```

### 2. Ativar o Ambiente Virtual

Sempre que quiseres correr ou instalar pacotes para este servidor, tens de ativar o ambiente primeiro.

**Windows:**

```bash
venv\Scripts\activate
```

**macOS/Linux:**

```bash
source venv/bin/activate
```

> Saberás que está ativado quando vires `(venv)` no início da linha do terminal.

### 3. Instalar Dependências

Com o ambiente ativado, instala as bibliotecas necessárias correndo:

```bash
pip install fastapi uvicorn ollama quickdraw Pillow pydantic
```

---

## Como Iniciar o Servidor

Com o ambiente virtual ativado (`venv`), executa o servidor com o `uvicorn`:

```bash
uvicorn app.main:app --host 127.0.0.1 --port 8000
```

Se tudo estiver correto, verás mensagens a indicar que o LLaVA está a carregar e que o servidor está pronto.

O servidor ficará à escuta no endereço:

```text
http://127.0.0.1:8000
```

---

## Testar se Está a Funcionar

Abre o teu browser e vai a:

```text
http://127.0.0.1:8000/hello-test
```

Deverás ver a mensagem:

```json
{"response": "Hello! O oponente IA está pronto para jogar e julgar!"}
```

---

## Notas Adicionais

- Mantém a janela do terminal aberta enquanto estiveres a jogar. Se a fechares, a IA deixará de responder no Unity.
- Na primeira vez que a IA tentar gerar um desenho, poderá haver um ligeiro atraso enquanto a biblioteca descarrega os dados do QuickDraw para o teu computador.
