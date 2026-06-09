# Digital Signature Platform — Apresentação do Projeto

Documento de referência para entrevistas. Cobre plano, execução, decisões e como rodar.

---

## O que é o projeto

Plataforma web de assinatura digital de documentos. Objetivo declarado: demonstrar domínio de arquitetura, criptografia e boas práticas de engenharia para vaga em empresa de identidade digital.

**Stack:**
- Backend: .NET 10 + ASP.NET Core Minimal APIs + EF Core 9 + PostgreSQL
- Frontend: Angular 20 (standalone components, signals)
- Infra: Docker + GitHub Actions CI/CD + SonarCloud

**Deploy público:**
- API: `https://digital-signature-platform-1.onrender.com`
- Frontend: Static Site no Render (mesmo repo)
- Banco: PostgreSQL gerenciado no Render (free tier)

---

## Cronologia de implementação

### Fase 0 — Fundação (commits iniciais)

**O que foi feito:**
- Criação do plano de implementação (`docs: add Digital Signature Platform implementation plan`)
- Scaffold da solução com 9 projetos (`chore: scaffold complete solution`)
- Configurações base: `.editorconfig`, `.gitignore`, `Directory.Build.props` com `TreatWarningsAsErrors=true`
- CI no GitHub Actions desde o primeiro commit com código real

**Decisão importante:** `TreatWarningsAsErrors = true` desde o início. Warnings viram erros de build — evita acúmulo de dívida técnica.

---

### Fase 1 — Domain + Application (TDD)

**Commits:**
```
feat(users):    implement RegisterUser with TDD (Domain + Application)
feat(auth):     implement LoginUser + JWT authentication (TDD)
test(arch):     add dependency rule tests enforcing Clean Architecture
```

**O que foi construído:**
- Entidades `User`, `Document`, `Signature` com invariantes de domínio
- Value objects: `Email` (com regex NonBacktracking para prevenir ReDoS), `DocumentHash`, `PasswordPolicy`
- Comandos MediatR: `RegisterUserCommand`, `LoginUserCommand`
- Validators FluentValidation acoplados aos comandos
- **Testes de arquitetura** (NetArchTest) que quebram o CI se alguém violar a regra de dependência

**Por que testes de arquitetura:**
Domain não pode referenciar Infrastructure. Application não pode referenciar Api. Isso é convenção — sem teste, alguém viola sem perceber. O teste torna a regra executável.

---

### Fase 2 — Infrastructure (banco + criptografia)

**Commits:**
```
feat(infra):      add PostgreSQL + EF Core, User entity, initial migration
feat(documents):  implement document upload with SHA-256 + AES-GCM (TDD)
feat(signatures): implement RSA-PSS + X509 + PFX digital signing (TDD)
```

**Pipeline de criptografia (5 camadas):**

| # | Camada | Algoritmo | Onde |
|---|--------|-----------|------|
| 1 | Hash de integridade | SHA-256 | Upload |
| 2 | Criptografia em repouso | AES-256-GCM | Storage |
| 3 | Assinatura digital | RSA-PSS + SHA-256 | Signing |
| 4 | Validação de certificado | X.509 | Validation |
| 5 | Chave protegida por senha | PKCS#12 (PFX) | Key store |

**Detalhe sobre AES-GCM:** modo autenticado — um byte alterado no arquivo armazenado lança `AuthenticationTagMismatchException`. Adulteração é detectada na decriptação, sem MAC separado.

**Detalhe sobre RSA-PSS vs PKCS#1 v1.5:** PSS é probabilístico (salt aleatório por operação), PKCS#1 é determinístico. RFC 8017 recomenda PSS para novas implementações. Cita isso na entrevista.

---

### Fase 3 — API + qualidade

**Commits:**
```
feat:    add pipeline behaviors, global error middleware, admin endpoint, API integration tests
ci:      add coverage collection, ReportGenerator, and PR template
docs:    complete Week 4 — Docker, ADRs, docs/ai, updated README
```

**O que foi adicionado:**
- Pipeline behaviors MediatR: `ValidationBehavior` (FluentValidation automático antes de todo handler)
- `ExceptionMiddleware` global — stack trace nunca vaza para o cliente
- Endpoint `/health` para healthcheck
- Testes de integração da API com Testcontainers (PostgreSQL real no CI, não mock)
- 7 ADRs documentando cada decisão arquitetural
- Cobertura: Domain 100%, Application 100%, global ≥70%

---

### Fase 4 — Frontend Angular 20

**Commit:**
```
feat(frontend): add Angular 20 frontend with auth, document management
```

**Funcionalidades:**
- Login/Registro com interceptor JWT
- Lista de documentos com paginação
- Upload com feedback de progresso
- Assinar documento
- Validar assinatura com status visual
- Trilha de auditoria

**Técnicas Angular:** standalone components, signals para estado reativo, Angular Material.

---

### Fase 5 — Docker + CI/CD

**Commits:**
```
chore: finalize Docker, nginx config, and update README
chore: finalize project — SonarQube CI, seed, CD workflow, checklist
ci:    add SonarQube analysis workflow (build.yml)
```

**Detalhes:**
- `api.Dockerfile`: multi-stage (SDK → ASP.NET runtime), roda como user `app` (não-root)
- `web.Dockerfile`: multi-stage (Node → `nginxinc/nginx-unprivileged`), roda como uid 101
- `DatabaseSeeder`: cria usuários demo/admin na primeira inicialização (idempotente)
- CD workflow: build + push Docker images para GHCR

---

### Fase 6 — SonarCloud (segurança e qualidade)

**Commits:**
```
fix(sonar): suppress false positive JWT hotspot in JwtTokenGenerator
fix(sonar): remove hardcoded credentials from AppDbContextFactory and Dockerfile
fix(sonar): track package-lock.json for predictable dependency versions
fix(sonar): add NonBacktracking to Email regex to prevent ReDoS
fix(sonar): pass SONAR_TOKEN via env var instead of shell interpolation
fix(sonar): pin workflow actions to SHA and remove secret interpolation
fix(sonar): run nginx as non-root using nginx-unprivileged image
fix(sonar): await RunAsync instead of blocking Run in Program.cs
fix(sonar): remove hardcoded credentials from appsettings.Testing.json
```

**Issues corrigidos e por quê:**

| Issue | Fix | Por quê importa |
|-------|-----|-----------------|
| ReDoS em regex de Email | `RegexOptions.NonBacktracking` | Complexidade O(n), não exponencial |
| Secret interpolado em `run:` | Ler de `$SONAR_TOKEN` (env) | Secret exposto em logs do CI |
| Actions com `@v3` / `@v4` | Pinado para SHA completo | Supply chain attack — tag mutável |
| nginx rodando como root | `nginx-unprivileged` image | Princípio de menor privilégio |
| Credenciais em JSON de teste | Config in-memory em `ApiFactory` | Nenhum arquivo com senha no repo |

**Resultado:** Quality Gate Passed, zero hotspots de segurança.

---

### Fase 7 — Deploy Render

**Commits:**
```
fix(docker): use built-in app user instead of adduser in api image
fix(web):    point production apiUrl to Render backend service
docs:        mark SonarQube, deploy, and security review as complete
```

**Problema encontrado:** `nginx:alpine` tem `adduser` mas `mcr.microsoft.com/dotnet/aspnet:10.0` é imagem mínima sem ele. Fix: usar `USER app` (user pré-existente nas imagens .NET 8+).

**Problema de CORS:** `environment.prod.ts` usava `/api` (relativo) — funciona no Docker local com nginx proxy, mas Static Site no Render não tem proxy. Fix: URL absoluta da API.

---

## Decisões arquiteturais (ADRs)

| ADR | Decisão | Alternativa descartada |
|-----|---------|----------------------|
| 0001 | Clean Architecture + CQRS leve (MediatR) | Vertical Slice, MVC simples |
| 0002 | PostgreSQL + EF Core 9 | SQL Server (custo), SQLite (não-prod) |
| 0003 | JWT stateless + anti-enumeration | Cookies de sessão, OIDC (sobre-engenharia) |
| 0004 | AES-256-GCM (modo autenticado) | AES-CBC+HMAC (padding oracle risk) |
| 0005 | RSA-PSS sobre hash SHA-256 | PKCS#1 v1.5 (determinístico, menos seguro) |
| 0006 | `IFileStorage` abstração | Filesystem direto no domain |

---

## Como rodar localmente

### Pré-requisito
Docker Desktop instalado e rodando.

### Subir tudo com um comando
```bash
# Copiar variáveis de ambiente
cp .env.example .env
# Editar .env com JWT_SECRET e AES_MASTER_KEY reais
# (gerar com: openssl rand -base64 32)

# Subir stack completa
docker compose -f deploy/docker-compose.yml up -d
```

Abre `http://localhost` no browser.

### Usuários demo (criados automaticamente)
| Usuário | Senha |
|---------|-------|
| `demo@digitalsignature.com` | `Demo@123456` |
| `admin@digitalsignature.com` | `Admin@123456` |

### Rodar só os testes
```bash
# Todos (requer Docker para integration tests)
dotnet test DigitalSignature.slnx

# Só unit tests (sem Docker)
dotnet test tests/DigitalSignature.Domain.UnitTests
dotnet test tests/DigitalSignature.Application.UnitTests
dotnet test tests/DigitalSignature.ArchitectureTests

# Teste específico
dotnet test tests/DigitalSignature.Domain.UnitTests --filter "FullyQualifiedName~EmailTests"
```

### Rodar frontend separado
```bash
cd src/digital-signature-web
npm install
npm start
# Abre http://localhost:4200
```

---

## Números do projeto

| Métrica | Valor |
|---------|-------|
| Total de testes | 74 (todos passando) |
| Cobertura Domain | 100% |
| Cobertura Application | 100% |
| Cobertura global | ≥70% |
| SonarCloud Quality Gate | Passed |
| Security hotspots | 0 |
| ADRs documentados | 7 |
| Camadas de criptografia | 5 |

---

## Para a entrevista — perguntas prováveis e respostas diretas

**"Por que Clean Architecture?"**
> Testabilidade e substituibilidade. Domain tem zero dependências externas — testável sem banco, sem framework. Troca de ORM: só toca Infrastructure. Verificado por teste automatizado, não por convenção.

**"Por que RSA-PSS e não PKCS#1?"**
> PSS usa salt aleatório por operação (probabilístico). PKCS#1 v1.5 é determinístico e vulnerável a fault injection. RFC 8017 recomenda PSS para novas implementações. .NET suporta nativamente.

**"Por que AES-GCM e não AES-CBC?"**
> GCM é modo autenticado — detecta adulteração do ciphertext sem MAC separado. CBC com padding é suscetível a padding oracle attack se mal implementado. Com GCM, adulteração lança exceção na decriptação.

**"Como você garante que a arquitetura não vai ser violada?"**
> Teste de arquitetura com NetArchTest. Quebra o CI se Infrastructure for referenciada por Domain ou Application. Regra executável, não documentação que ninguém lê.

**"O que você faria diferente em produção?"**
> Envelope encryption (chave AES protegida por KMS, não diretamente de env var). HSM ou Key Vault para a chave privada RSA. Refresh token. Rate limiting no API gateway. Carimbo de tempo (TSA) na assinatura. OCSP/CRL para revogação.

**"Por que você usou MediatR gratuito e não pago?"**
> Documentado no ADR-0008. MediatR v11.x é gratuito para uso comercial. Versões posteriores mudaram licença. Decision log existe para qualquer um auditar.

**"Como funciona o anti-enumeration?"**
> Login retorna a mesma mensagem para "email não existe" e "senha errada". Sem isso, atacante sabe quais emails têm conta cadastrada. Verificado em teste específico que garante a mensagem ser idêntica nos dois cenários.

---

## O que falta para produção real

- Refresh token
- Rate limiting (por IP, por endpoint)
- Envelope encryption com KMS
- HSM ou Azure Key Vault para chave RSA
- Carimbo de tempo (TSA) na assinatura
- Revogação (CRL/OCSP)
- Observabilidade (logs estruturados, traces, métricas)
- Múltiplas instâncias (storage compartilhado — S3/Blob ao invés de filesystem)
