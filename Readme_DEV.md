# Tutorial de Desenvolvimento (DEV)

Este guia descreve como configurar e executar o projeto **UBS Watchdog** em ambientes **Windows, Linux e macOS**, cobrindo back-end, banco de dados e front-end.

---

## 1. Pré-requisitos

### Obrigatórios

* **Git**
* **Docker Desktop** (Windows, macOS ou Linux)
* **Node.js (LTS)** – necessário para rodar o front-end

### Opcionais

* **.NET SDK 8** – apenas se desejar executar a API fora do Docker

---

## 2. Clonando o repositório

```bash
git clone https://github.com/PedroPauloMorenoCamargo/UBS_Watchdog.git
cd UBS_Watchdog
```

---

## 3. Instalando o Docker

### Windows

1. Baixe o **Docker Desktop**:
   [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)
2. Durante a instalação, habilite o **WSL 2**, se solicitado.
3. Reinicie o computador, caso necessário.

---

### macOS

1. Baixe o **Docker Desktop para macOS**:
   [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)
2. Arraste o Docker para a pasta **Applications**.
3. Abra o Docker Desktop e aguarde a inicialização completa.

---

### Linux (Ubuntu / Debian)

```bash
sudo apt update
sudo apt install -y docker.io docker-compose-plugin
sudo usermod -aG docker $USER
newgrp docker
```

---

### Verificação:

```bash
docker --version
docker compose version
```

---

## 4. Subindo o Back-end + Banco de Dados (PostgreSQL)

### 4.1 Criar arquivo de ambiente (.env)

Na **raiz do projeto**, crie o arquivo `.env`.

**Linux / macOS:**

```bash
touch .env
```

**Windows (PowerShell):**

```powershell
New-Item .env
```

Adicione o conteúdo abaixo:

```env
POSTGRES_DB=ubs_monitoring_db
POSTGRES_USER=ubs_user
POSTGRES_PASSWORD=ubs_user@1234
```

---

### 4.2 Subir os containers

Na raiz do projeto, execute:

```bash
docker compose up --build
```

> O comando é o mesmo para **Windows, Linux e macOS**.

Isso irá:

* Subir um container **PostgreSQL**
* Subir a **API .NET 8**
* Aplicar automaticamente as variáveis de ambiente

---

## 5. Acessando a API

Após os containers subirem com sucesso:

* **Swagger UI**
  [http://localhost:8080/swagger](http://localhost:8080/swagger)

* **Health Check**
  [http://localhost:8080/health](http://localhost:8080/health)

---

## 6. Instalando Node.js e npm

### Windows / macOS

Baixe o instalador **LTS** em:

* [https://nodejs.org/](https://nodejs.org/)

Durante a instalação no Windows, mantenha a opção **“Add to PATH”** habilitada.

---

### Linux (Ubuntu / Debian)

```bash
sudo apt install -y nodejs npm
```

---

### Verificação

```bash
node -v
npm -v
```

---

## 7. Rodando o Front-end (React + TypeScript)

### 7.1 Acessar a pasta do front-end

```bash
cd frontend
```

---

### 7.2 Criar arquivo de ambiente do front-end

Crie o arquivo `.env` dentro da pasta `frontend`.

**Linux / macOS:**

```bash
touch .env
```

**Windows (PowerShell):**

```powershell
New-Item .env
```

Adicione o conteúdo:

```env
VITE_API_BASE_URL=http://localhost:8080
```

---

### 7.3 Instalar dependências

```bash
npm install
```

---

### 7.4 Rodar em modo desenvolvimento

```bash
npm run dev
```

---

## 8. Acessando o Front-end

Após iniciar o front-end:

* **Aplicação Web**
  [http://localhost:5173](http://localhost:5173)

---

## 9. Encerrando os serviços

### 9.1 Back-end e Banco de Dados (Docker)

Para parar e remover os containers do back-end e do banco de dados, execute na raiz do projeto:

```bash
docker compose down
```

Esse comando:

* Interrompe todos os containers em execução
* Remove os containers, mantendo volumes e imagens intactos

---

### 9.2 Front-end (React)

Caso o front-end esteja rodando em modo desenvolvimento, finalize a execução pressionando:

```text
Ctrl + C
```

no terminal onde o comando `npm run dev` foi executado.

---

## 10. Problemas Comuns

### 10.1 Porta já em uso (8080 ou 5432)

* Verifique se não há outro serviço usando a porta.
* Ajuste as portas no arquivo `docker-compose.yml`, se necessário.

---

### 10.2 Docker sem permissão (Linux)

* Confirme se o usuário pertence ao grupo `docker`:

```bash
groups
```

* Caso não pertença:

```bash
sudo usermod -aG docker $USER
newgrp docker
```

---
