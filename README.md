# Digital Signature Platform

Plataforma web para gerenciamento e assinatura digital de documentos. Projeto de portfólio demonstrando competências Full Stack (.NET 10 + Angular 20) com foco em segurança da informação, criptografia e certificação digital.

![Build](https://github.com/joaogsoares1993/digital-signature-platform/actions/workflows/ci.yml/badge.svg)

---

## Visão Geral

O sistema cobre o ciclo de vida completo de um documento assinável:

```
Upload → Hash SHA-256 → Criptografia AES-GCM → Assinatura RSA/PFX → Validação X509 → Auditoria
```

### Pilares técnicos demonstrados

| Competência | Implementação |
|---|---|
| Clean Architecture | Domain / Application / Infrastructure / API com regra verificada por teste |
| TDD real | Red-Green-Refactor desde o commit inicial; histórico prova a disciplina |
| Segurança progressiva | SHA-256 → AES-GCM → RSA-PSS → X509 → PFX (5 níveis) |
| CQRS leve | MediatR: Command + Handler por caso de uso |
| DevOps | GitHub Actions CI, SonarQube, Quality Gate, Docker |
| IA como ferramenta | `/docs/ai` com prompts versionados e análise crítica |

---

## Stack

### Backend
- **.NET 10** + ASP.NET Core Minimal APIs
- **EF Core 9** + PostgreSQL (Npgsql)
- **MediatR 11** (CQRS leve)
- **FluentValidation 11**

### Frontend
- **Angular 20** (standalone components, signals)
- **Angular Material**
- **RxJS**

### Testes
- **xUnit** + **FluentAssertions 7** + **NSubstitute 5**
- **NetArchTest.Rules** (regra de dependência)
- **Testcontainers** (integração com PostgreSQL real)

### Infraestrutura
- **Docker** + **Docker Compose**
- **GitHub Actions** CI/CD
- **SonarQube** Quality Gate

---

## Arquitetura

```
         ┌─────────────────────────────────────────┐
         │                  API                    │  (composition root, HTTP)
         │   ┌─────────────────────────────────┐   │
         │   │         Infrastructure          │   │  (EF Core, crypto, JWT, cert store)
         │   │   ┌─────────────────────────┐   │   │
         │   │   │      Application        │   │   │  (use cases, ports, validators)
         │   │   │   ┌─────────────────┐   │   │   │
         │   │   │   │     Domain      │   │   │   │  (entidades, VOs, regras puras)
         │   │   │   └─────────────────┘   │   │   │
         │   │   └─────────────────────────┘   │   │
         │   └─────────────────────────────────┘   │
         └─────────────────────────────────────────┘
```

**Regra de dependência:** setas apontam para dentro. Domain não conhece ninguém. Regra executável: `tests/DigitalSignature.ArchitectureTests/DependencyRuleTests.cs`.

---

## Como Executar Localmente

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js 22+](https://nodejs.org) (para frontend)

### 1. Clonar o repositório

```bash
git clone https://github.com/SEU_USUARIO/digital-signature-platform.git
cd digital-signature-platform
```

### 2. Subir o banco de dados

```bash
docker compose -f deploy/docker-compose.yml up -d
```

### 3. Aplicar migrations

```bash
dotnet ef database update \
  --project src/DigitalSignature.Infrastructure \
  --startup-project src/DigitalSignature.Api
```

### 4. Configurar variáveis (desenvolvimento)

O arquivo `src/DigitalSignature.Api/appsettings.Development.json` já contém configurações de desenvolvimento. Para produção, use variáveis de ambiente (ver `.env.example`).

### 5. Rodar a API

```bash
dotnet run --project src/DigitalSignature.Api
```

API disponível em `https://localhost:7xxx` | OpenAPI UI em `/openapi/v1.json`.

### 6. Rodar o Frontend (em breve)

```bash
cd src/frontend/digital-signature-web
npm install
ng serve
```

---

## Como Testar

### Rodar todos os testes

```bash
dotnet test DigitalSignature.slnx
```

### Testes por camada

```bash
# Domain (unit - sem dependências externas)
dotnet test tests/DigitalSignature.Domain.UnitTests

# Application (unit - com NSubstitute mocks)
dotnet test tests/DigitalSignature.Application.UnitTests

# Arquitetura (regra de dependência)
dotnet test tests/DigitalSignature.ArchitectureTests

# Integração (requer Docker para Testcontainers)
dotnet test tests/DigitalSignature.Infrastructure.IntegrationTests
dotnet test tests/DigitalSignature.Api.IntegrationTests
```

### Estado atual dos testes

| Projeto | Testes | Status |
|---|---|---|
| Domain.UnitTests | 31 | ✅ |
| Application.UnitTests | 25 | ✅ |
| Infrastructure.IntegrationTests | 9 | ✅ |
| ArchitectureTests | 5 | ✅ |
| **Total** | **70** | **✅** |

---

## Fluxo de Autenticação

```
POST /api/users/register
  Body: { email, password }
  → Valida Email VO + PasswordPolicy
  → Hash PBKDF2-SHA512 (350k iterations)
  → Persiste no PostgreSQL
  ← 201 Created { id }

POST /api/users/login
  Body: { email, password }
  → Busca usuário por email
  → Verifica hash (timing-safe)
  → Gera JWT (HMAC-SHA256, 60min)
  ← 200 OK { token }
```

**Anti-enumeração:** mesmo erro para "usuário não existe" e "senha incorreta".

---

## Fluxo de Assinatura Digital

> Em implementação (Semana 3)

```
POST /api/documents/upload
  → SHA-256 do conteúdo (integridade)
  → AES-256-GCM (confidencialidade em repouso)
  → Persiste metadados + conteúdo cifrado

POST /api/documents/{id}/sign
  → Recomputa hash
  → Assina hash com RSA-PSS (chave privada / PFX)
  → Persiste Signature + AuditEvent

POST /api/documents/{id}/validate
  → Recomputa hash do documento
  → Verifica assinatura RSA-PSS
  → Valida certificado X509 (validade + cadeia + key usage)
  ← { isValid, certificate, signedAt }
```

---

## Níveis de Criptografia

| Nível | Algoritmo | Propósito | Status |
|---|---|---|---|
| 1 | SHA-256 | Integridade do documento | ✅ Implementado |
| 2 | AES-256-GCM | Confidencialidade em repouso | ✅ Implementado |
| 3 | RSA-PSS | Assinatura digital | ✅ Implementado |
| 4 | X509 | Validação de certificado | ✅ Implementado |
| 5 | PFX/PKCS#12 | Simulação de certificado A1 | ✅ Implementado |

---

## CI/CD

Pipeline no GitHub Actions (`.github/workflows/ci.yml`):

```
Push/PR → Restore → Build (Release) → Test → [SonarQube] → [Deploy]
```

**Regras:**
- Build falha se `TreatWarningsAsErrors` ativo.
- Teste de arquitetura bloqueia violações de dependência.
- Quality Gate SonarQube: cobertura ≥ 70% global (≥ 80% Domain/Application).

---

## Decisões Arquiteturais (ADRs)

| ADR | Decisão |
|---|---|
| [0001](docs/adr/0001-clean-architecture.md) | Clean Architecture + CQRS leve com MediatR |
| [0002](docs/adr/0002-postgres-efcore.md) | PostgreSQL + EF Core 9 |
| [0003](docs/adr/0003-jwt-authentication.md) | JWT stateless, anti-enumeração |
| [0004](docs/adr/0004-aes-gcm-encryption.md) | AES-256-GCM para criptografia em repouso |
| [0005](docs/adr/0005-rsa-pss-signature.md) | RSA-PSS sobre hash, PFX para material de chave |
| [0006](docs/adr/0006-file-storage-abstraction.md) | Abstração IFileStorage (filesystem → cloud-ready) |
| [0008](docs/adr/0008-commercial-licenses.md) | Versões OSS de MediatR e FluentAssertions |

---

## Estrutura do Projeto

```
digital-signature-platform/
├─ .github/workflows/     # CI/CD
├─ deploy/                # docker-compose.yml
├─ docs/
│  ├─ adr/                # Architecture Decision Records
│  └─ ai/                 # Prompts de IA utilizados
├─ src/
│  ├─ DigitalSignature.Domain/
│  ├─ DigitalSignature.Application/
│  ├─ DigitalSignature.Infrastructure/
│  └─ DigitalSignature.Api/
└─ tests/
   ├─ DigitalSignature.Domain.UnitTests/
   ├─ DigitalSignature.Application.UnitTests/
   ├─ DigitalSignature.Infrastructure.IntegrationTests/
   ├─ DigitalSignature.Api.IntegrationTests/
   └─ DigitalSignature.ArchitectureTests/
```

---

## Uso de IA (Claude Code)

Este projeto foi desenvolvido com Claude Code como principal ferramenta de apoio. Os prompts utilizados estão versionados em [`/docs/ai`](docs/ai/) com análise crítica do que foi aceito/rejeitado — demonstrando engenharia assistida por IA sob controle do desenvolvedor.

---

## Roadmap

- [X] **Semana 1:** Fundação, esqueleto, CI, RegisterUser + LoginUser/JWT (36 testes)
- [X] **Semana 2:** Upload + SHA-256 + AES-256-GCM (57 testes)
- [X] **Semana 3:** Assinatura RSA-PSS + X509 + PFX + Auditoria (70 testes)
- [X] **Semana 4:** Docker, ADRs, `/docs/ai`, README (backend completo)
- [ ] **Pendente:** Frontend Angular, SonarQube, Deploy público

---

## Variáveis de Ambiente

Copie `.env.example` para `.env` (nunca versionar):

```bash
ConnectionStrings__Default=Host=...;Database=...
Jwt__Secret=<256-bit-random-key>
Jwt__Issuer=digital-signature-platform
Jwt__Audience=digital-signature-platform
Jwt__ExpiryMinutes=60
```

---

*Desenvolvido por João Gabriel — portfólio para vaga Full Stack Angular + .NET em empresa de identidade/certificação digital.*
