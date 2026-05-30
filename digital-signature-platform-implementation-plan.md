# Implementation Plan — Digital Signature Platform

> Plano de implementação detalhado para projeto de portfólio Full Stack (Angular 20 + .NET 10) com foco em identidade digital, certificação digital, assinatura eletrônica e segurança da informação.
>
> **Perfil do autor:** desenvolvedor júnior usando Claude Code como principal ferramenta de apoio.
> **Janela de execução:** 3 a 4 semanas.
> **Data base do plano:** 2026-05-30.

---

## Sumário

1. [Visão Geral do Projeto](#1-visão-geral-do-projeto)
2. [Objetivos Técnicos e de Negócio](#2-objetivos-técnicos-e-de-negócio)
3. [Arquitetura Completa](#3-arquitetura-completa)
4. [Estrutura de Pastas](#4-estrutura-de-pastas)
5. [Roadmap Cronológico](#5-roadmap-cronológico)
6. [Backlog Detalhado](#6-backlog-detalhado)
7. [Estratégia de TDD](#7-estratégia-de-tdd)
8. [Estratégia de Segurança](#8-estratégia-de-segurança)
9. [Estratégia de Uso de IA](#9-estratégia-de-uso-de-ia)
10. [Estratégia de CI/CD](#10-estratégia-de-cicd)
11. [Estratégia de Deploy](#11-estratégia-de-deploy)
12. [Riscos Técnicos](#12-riscos-técnicos)
13. [Melhorias Futuras](#13-melhorias-futuras)
14. [Checklist Final](#14-checklist-final)

---

## 1. Visão Geral do Projeto

**Digital Signature Platform** é uma aplicação web para gerenciamento e assinatura digital de documentos. O sistema cobre o ciclo de vida completo de um documento assinável: upload, cálculo de integridade, criptografia em repouso, assinatura digital baseada em chave/certificado, validação da assinatura e trilha de auditoria imutável.

O projeto foi desenhado como **peça de portfólio orientada à vaga** (empresa de identidade digital / certificação / assinatura eletrônica). Cada decisão técnica serve a dois propósitos: entregar a funcionalidade **e** demonstrar uma competência específica que o recrutador/sênior procura.

### Pilares de demonstração

| Pilar | Como o projeto demonstra |
|-------|--------------------------|
| Full Stack moderno | Angular 20 standalone + .NET 10 minimal hosting, comunicação REST tipada |
| Clean Architecture | Separação real Domain / Application / Infrastructure / API com regra de dependência |
| Segurança & Criptografia | Progressão SHA-256 → AES → RSA → X509 → PFX, aplicada em features reais |
| TDD real | Red-Green-Refactor desde o commit inicial; testes guiam o design |
| Engenharia assistida por IA | Pasta `/docs/ai` com prompts versionados; Claude Code no fluxo diário |
| DevOps & Qualidade | GitHub Flow, Conventional Commits, GitHub Actions, SonarQube, Quality Gates |
| Deploy real | Sistema acessível por URL pública para recrutadores |

### Princípio condutor

> **Profundidade sobre amplitude.** Melhor um conjunto menor de features impecavelmente testadas, seguras e documentadas do que muitas features frágeis. Em entrevista, o avaliador investiga *como* foi feito, não *quanto*.

---

## 2. Objetivos Técnicos e de Negócio

### 2.1 Objetivos de negócio (narrativa para o recrutador)

- **B1.** Permitir que um usuário autenticado faça upload de um documento e o assine digitalmente.
- **B2.** Permitir que qualquer parte valide se um documento assinado é íntegro e autêntico.
- **B3.** Garantir rastreabilidade: quem fez o quê, quando, sobre qual documento (auditoria).
- **B4.** Simular o uso de certificados digitais reais (PFX / PKCS#12), aproximando o projeto do domínio de certificação digital (ex.: padrão ICP-Brasil A1).
- **B5.** Disponibilizar um painel administrativo com visão de usuários, documentos e eventos de auditoria.

### 2.2 Objetivos técnicos

- **T1.** Aplicar Clean Architecture com regra de dependência apontando para dentro, verificável por testes de arquitetura.
- **T2.** Cobertura de testes significativa nas camadas Domain e Application (meta: ≥ 80% nessas camadas; ≥ 70% global como Quality Gate).
- **T3.** TDD verificável pelo histórico de commits (testes antes da implementação).
- **T4.** Pipeline CI que falha em build quebrado, teste vermelho, cobertura abaixo do gate ou violação de qualidade do SonarQube.
- **T5.** Criptografia correta por construção: algoritmos modernos, modos autenticados, sem segredos em código, comparações seguras.
- **T6.** Documentação de decisões via ADRs e API via OpenAPI.
- **T7.** Deploy reproduzível via Docker / Docker Compose e ambiente público.

### 2.3 Critérios de sucesso (definição de "pronto para entrevista")

- Sistema acessível por URL pública, com usuário de demonstração.
- README permite a um terceiro subir o projeto localmente em < 10 minutos.
- Fluxo completo demonstrável: registrar → logar → upload → assinar → validar → ver auditoria.
- Os 5 níveis de criptografia implementados e explicados na documentação.
- Badges visíveis: build, cobertura, Quality Gate do SonarQube.

### 2.4 Não-objetivos (escopo deliberadamente fora)

- Conformidade legal real com ICP-Brasil / eIDAS (é uma *simulação* educacional).
- Integração com Autoridade Certificadora real ou carimbo de tempo (TSA) qualificado.
- Assinatura de PDF no padrão PAdES embarcado (fica em Melhorias Futuras).
- Multi-tenant, billing, alta disponibilidade.

> Deixar não-objetivos explícitos é sinal de maturidade: comunica que o recorte foi consciente, não acidental.

---

## 3. Arquitetura Completa

### 3.1 Estilo arquitetural

Clean Architecture (Onion / Ports & Adapters) com **CQRS leve** via MediatR no nível de Application. A regra fundamental: **dependências apontam para dentro**. Domain não conhece ninguém; API conhece todos para compor o grafo de injeção de dependência.

```
         ┌─────────────────────────────────────────┐
         │                  API                    │  (composition root, HTTP)
         │   ┌─────────────────────────────────┐   │
         │   │         Infrastructure          │   │  (EF Core, crypto impl, cert store)
         │   │   ┌─────────────────────────┐   │   │
         │   │   │      Application        │   │   │  (use cases, ports, validators)
         │   │   │   ┌─────────────────┐   │   │   │
         │   │   │   │     Domain      │   │   │   │  (entidades, VOs, regras puras)
         │   │   │   └─────────────────┘   │   │   │
         │   │   └─────────────────────────┘   │   │
         │   └─────────────────────────────────┘   │
         └─────────────────────────────────────────┘

   Sentido permitido das dependências:  API → Infrastructure → Application → Domain
   (Application define interfaces; Infrastructure as implementa — Dependency Inversion)
```

### 3.2 Responsabilidades por camada

#### Domain (núcleo, zero dependências externas)
- Entidades: `User`, `Document`, `Signature`, `Certificate`, `AuditEvent`.
- Value Objects: `DocumentHash`, `Email`, `SignatureAlgorithm`, `CertificateThumbprint`.
- Eventos de domínio: `DocumentSignedEvent`, `DocumentUploadedEvent`.
- Exceções de domínio e invariantes (ex.: documento já assinado não pode ser assinado de novo pelo mesmo certificado).
- **Sem** EF Core, sem ASP.NET, sem `System.Security.Cryptography` *de implementação* (o Domain expressa *conceitos* de assinatura/hash via interfaces ou VOs, não a mecânica).

#### Application (orquestração de casos de uso)
- Comandos e queries (MediatR): `RegisterUserCommand`, `UploadDocumentCommand`, `SignDocumentCommand`, `ValidateSignatureQuery`, `GetAuditTrailQuery`.
- Handlers que orquestram regras de domínio e portas.
- **Ports (interfaces)**: `IDocumentRepository`, `IUserRepository`, `IHashService`, `IEncryptionService`, `ISignatureService`, `ICertificateStore`, `IFileStorage`, `IAuditLogger`, `IUnitOfWork`, `IClock`, `ICurrentUser`.
- Validação de entrada com FluentValidation.
- DTOs e mapeamentos. Pipeline behaviors do MediatR (validação, logging, transação).
- Depende **apenas** de Domain.

#### Infrastructure (adapters / detalhes)
- EF Core `DbContext`, configurações, migrations, repositórios concretos.
- Implementações de criptografia: `Sha256HashService`, `AesEncryptionService`, `RsaSignatureService`, `X509Validator`, `PfxCertificateStore`.
- Persistência de arquivos (`FileSystemStorage` local; abstração permite blob storage depois).
- Implementação de auditoria, relógio do sistema, integração com secret/config.
- Implementa as interfaces declaradas em Application.

#### API (composition root + borda HTTP)
- Endpoints (Controllers ou Minimal APIs) finos: recebem request → enviam comando/query via MediatR → retornam resultado.
- Autenticação/autorização (JWT), middleware de tratamento de erros, CORS, rate limiting básico.
- Configuração de DI ligando portas → implementações de Infrastructure.
- OpenAPI/Swagger, health checks.
- **Única** camada que referencia Infrastructure (e somente para registrar DI).

### 3.3 Dependências permitidas (matriz)

| De ↓ \ Pode referenciar → | Domain | Application | Infrastructure | API |
|---------------------------|:------:|:-----------:|:--------------:|:---:|
| **Domain**                | —      | ❌          | ❌             | ❌  |
| **Application**           | ✅     | —           | ❌             | ❌  |
| **Infrastructure**        | ✅     | ✅          | —              | ❌  |
| **API**                   | ✅*    | ✅          | ✅ (só DI)     | —   |

\* API toca Domain apenas para tipos expostos em DTOs quando inevitável; preferir DTOs próprios.

> A matriz será **garantida por teste de arquitetura** (ver Seção 7), tornando a regra executável e não apenas documental.

### 3.4 Decisões arquiteturais relevantes (vão virar ADRs)

- **ADR-0001** — Clean Architecture + CQRS leve com MediatR.
- **ADR-0002** — PostgreSQL como banco relacional + EF Core como ORM.
- **ADR-0003** — Autenticação via JWT stateless; refresh token opcional.
- **ADR-0004** — Criptografia de documentos em repouso com AES-256-GCM (modo autenticado).
- **ADR-0005** — Assinatura digital sobre o *hash* do documento (não sobre o conteúdo inteiro), com RSA-PSS.
- **ADR-0006** — Armazenamento de arquivos via abstração `IFileStorage` (filesystem local agora; nuvem depois).
- **ADR-0007** — Testes de integração com Testcontainers (PostgreSQL real efêmero) em vez de banco em memória.
- **ADR-0008** — Tratamento de licenças de bibliotecas comerciais (ver nota abaixo).

> **Nota de licenciamento (mostra atenção sênior):** algumas libs da stack mudaram para licença comercial em 2025. **MediatR** e **FluentAssertions** passaram a exigir licença paga em versões recentes. Decisão recomendada e a registrar em ADR-0008: (a) usar versões dentro do limite gratuito / fixar versão anterior; ou (b) substituir por alternativas open-source — `Shouldly`/`AwesomeAssertions` no lugar de FluentAssertions, e um mediator simples próprio no lugar de MediatR. O plano mantém a stack pedida, mas o ADR documenta o trade-off. Demonstrar consciência disso é, por si só, um diferencial em entrevista.

### 3.5 Observações de stack específicas do .NET 10 / Angular 20

- **OpenAPI no .NET 10:** o template não inclui mais Swashbuckle por padrão; usa-se `Microsoft.AspNetCore.OpenApi` (geração do documento) combinado com uma UI — **Scalar** (recomendado) ou Swashbuckle/NSwag para a UI Swagger clássica.
- **Angular 20:** componentes **standalone** por padrão, `inject()` em vez de injeção por construtor onde fizer sentido, *control flow* nativo (`@if`/`@for`), e **signals** para estado de UI; RxJS para fluxos assíncronos/HTTP.

---

## 4. Estrutura de Pastas

```
digital-signature-platform/
├─ .github/
│  ├─ workflows/
│  │  ├─ ci.yml                  # build + test + coverage + sonar
│  │  └─ cd.yml                  # deploy (opcional / manual)
│  ├─ pull_request_template.md
│  └─ CODEOWNERS
├─ docs/
│  ├─ adr/
│  │  ├─ 0001-clean-architecture.md
│  │  ├─ 0002-postgres-efcore.md
│  │  └─ ...                     # demais ADRs
│  ├─ ai/                        # OBRIGATÓRIO
│  │  ├─ README.md               # como usamos Claude Code no projeto
│  │  ├─ architecture-prompts.md
│  │  ├─ tdd-prompts.md
│  │  ├─ review-prompts.md
│  │  ├─ security-prompts.md
│  │  └─ refactor-prompts.md
│  ├─ diagrams/                  # C4 / fluxos (auth, assinatura)
│  └─ screenshots/               # para o README
├─ src/
│  ├─ backend/
│  │  ├─ DigitalSignature.Domain/
│  │  │  ├─ Entities/
│  │  │  ├─ ValueObjects/
│  │  │  ├─ Events/
│  │  │  ├─ Exceptions/
│  │  │  └─ Abstractions/        # interfaces conceituais do domínio
│  │  ├─ DigitalSignature.Application/
│  │  │  ├─ Common/
│  │  │  │  ├─ Behaviors/        # pipeline MediatR (validation, logging, tx)
│  │  │  │  ├─ Interfaces/       # PORTS: repos, crypto, storage, clock...
│  │  │  │  └─ Models/           # DTOs, Result<T>
│  │  │  ├─ Users/               # commands/queries/validators por feature
│  │  │  ├─ Documents/
│  │  │  ├─ Signatures/
│  │  │  ├─ Certificates/
│  │  │  └─ Audit/
│  │  ├─ DigitalSignature.Infrastructure/
│  │  │  ├─ Persistence/         # DbContext, configs, migrations, repos
│  │  │  ├─ Cryptography/        # SHA256, AES, RSA, X509, PFX services
│  │  │  ├─ Storage/             # IFileStorage impl
│  │  │  ├─ Auth/                # JWT, password hashing
│  │  │  └─ DependencyInjection.cs
│  │  └─ DigitalSignature.Api/
│  │     ├─ Endpoints/ (ou Controllers/)
│  │     ├─ Middleware/
│  │     ├─ Extensions/
│  │     ├─ appsettings.json
│  │     └─ Program.cs
│  └─ frontend/
│     └─ digital-signature-web/
│        ├─ src/app/
│        │  ├─ core/             # interceptors, guards, services singletons
│        │  ├─ shared/           # componentes/material reutilizáveis
│        │  ├─ features/
│        │  │  ├─ auth/
│        │  │  ├─ documents/
│        │  │  ├─ signatures/
│        │  │  ├─ certificates/
│        │  │  └─ admin-dashboard/
│        │  └─ app.config.ts / app.routes.ts
│        ├─ src/environments/
│        └─ ...
├─ tests/
│  ├─ DigitalSignature.Domain.UnitTests/
│  ├─ DigitalSignature.Application.UnitTests/
│  ├─ DigitalSignature.Infrastructure.IntegrationTests/   # Testcontainers
│  ├─ DigitalSignature.Api.IntegrationTests/              # WebApplicationFactory
│  └─ DigitalSignature.ArchitectureTests/                 # regra de dependência
├─ deploy/
│  ├─ docker/
│  │  ├─ api.Dockerfile
│  │  └─ web.Dockerfile
│  └─ docker-compose.yml
├─ .editorconfig
├─ .gitignore
├─ Directory.Build.props          # versões, nullable, warnings-as-errors
├─ DigitalSignature.sln
└─ README.md
```

### Convenções adotadas

- **Nomenclatura:** uma feature = uma pasta em Application (`Documents/UploadDocument/` com Command + Handler + Validator + Response).
- **`Result<T>`** em vez de exceções para fluxo de negócio esperado; exceções apenas para casos excepcionais.
- **`.editorconfig`** unificado + `nullable enable` + `TreatWarningsAsErrors` em `Directory.Build.props`.
- **Migrations** versionadas no repositório; nunca aplicadas automaticamente em produção sem revisão.
- Frontend organizado por **feature folders** com lazy loading por rota.

---

## 5. Roadmap Cronológico

> Premissa: ~3–4 semanas em ritmo de projeto pessoal. As semanas são incrementos verticais: ao fim de cada uma há algo demonstrável. Se o tempo apertar, a Semana 4 contém os cortes seguros (ver Riscos).

### Semana 1 — Fundação, esqueleto arquitetural e primeiro fluxo testado

**Objetivos**
- Repositório, solução, camadas e pipeline CI mínima no ar **antes** de qualquer feature.
- TDD operante desde o primeiro caso de uso.
- Cadastro + autenticação de usuário ponta a ponta (sem UI ainda ou com UI mínima).

**Entregáveis**
- Solução .NET com 4 projetos + 5 projetos de teste compilando.
- `Directory.Build.props`, `.editorconfig`, `.gitignore`.
- CI (`ci.yml`) rodando build + testes em cada PR.
- Teste de arquitetura validando a regra de dependência.
- Feature `RegisterUser` e `LoginUser` (JWT) com TDD completo.
- PostgreSQL via Docker Compose; migration inicial.
- ADR-0001, 0002, 0003 escritos.

**Critérios de conclusão**
- [ ] `dotnet test` verde local e no CI.
- [ ] Teste de arquitetura falha propositalmente se alguém referenciar Infrastructure no Domain (verificado).
- [ ] Registrar + logar retorna JWT válido (teste de integração na API).
- [ ] Primeiro PR aberto, revisado (auto-revisão guiada por Claude Code) e mergeado via GitHub Flow.

---

### Semana 2 — Documentos, integridade (SHA-256) e criptografia em repouso (AES)

**Objetivos**
- Upload de documentos com persistência de metadados + arquivo.
- **Nível 1 (SHA-256):** hash de integridade calculado e armazenado no upload.
- **Nível 2 (AES):** conteúdo do documento criptografado em repouso.
- Início do frontend Angular real (auth + lista/upload de documentos).

**Entregáveis**
- Feature `UploadDocument` (TDD): valida tipo/tamanho, calcula `DocumentHash`, criptografa, persiste.
- Feature `DownloadDocument` / `GetDocument` com verificação de integridade na leitura.
- `IHashService`/`Sha256HashService` e `IEncryptionService`/`AesEncryptionService` com testes.
- Testes de integração com Testcontainers (PostgreSQL real).
- Frontend: tela de login, guard de rota, interceptor JWT, tela de documentos.
- ADR-0004 (AES-GCM) e ADR-0006 (storage).

**Critérios de conclusão**
- [ ] Upload → registro no banco com hash + conteúdo cifrado (nunca em claro no storage).
- [ ] Alterar 1 byte do conteúdo cifrado/armazenado faz a verificação de integridade falhar (teste prova isso).
- [ ] Frontend autentica e lista documentos do usuário logado.
- [ ] Cobertura das novas features ≥ meta nas camadas Domain/Application.

---

### Semana 3 — Assinatura digital (RSA → X509 → PFX) e validação

**Objetivos**
- **Nível 3 (RSA):** assinar o hash do documento; validar a assinatura.
- **Nível 4 (X509):** validar certificado (validade, cadeia, key usage).
- **Nível 5 (PFX):** carregar PKCS#12, assinar usando a chave do certificado (simula A1).
- Trilha de auditoria registrando cada evento relevante.

**Entregáveis**
- Feature `SignDocument` (TDD): assina hash com RSA/PFX, persiste `Signature` + `AuditEvent`.
- Feature `ValidateSignature` (TDD): recomputa hash, verifica assinatura e certificado.
- Feature `GetAuditTrail`.
- `RsaSignatureService`, `X509Validator`, `PfxCertificateStore` com testes (incl. certificado de teste gerado para o ambiente de testes).
- Frontend: ações "Assinar", "Validar", visual do resultado da validação, tela de auditoria.
- ADR-0005 (assinatura sobre hash, RSA-PSS).

**Critérios de conclusão**
- [ ] Assinar documento gera assinatura verificável e evento de auditoria.
- [ ] Adulterar o documento após assinado faz a validação retornar "inválido" (teste prova).
- [ ] Certificado expirado/ inválido é rejeitado na validação (teste prova).
- [ ] PFX protegido por senha é carregado a partir de configuração/secret, nunca do código.

---

### Semana 4 — Dashboard admin, qualidade, deploy, documentação e polimento

**Objetivos**
- Dashboard administrativo (usuários, documentos, eventos).
- Fechar qualidade: SonarQube + Quality Gate + cobertura no CI.
- Dockerização completa + deploy público.
- README profissional + ADRs + `/docs/ai` finalizados + screenshots.

**Entregáveis**
- Dashboard admin (papel `Admin`) com métricas e listagem de auditoria.
- `ci.yml` integrando SonarQube + cobertura (coverlet + report); badges.
- Dockerfiles (API e Web), `docker-compose.yml` completo (api + web + postgres).
- Deploy público (API + Postgres + frontend). Usuário demo.
- README, diagramas (auth e assinatura), screenshots, `/docs/ai` com exemplos reais usados no projeto.

**Critérios de conclusão**
- [ ] Quality Gate do SonarQube "Passed" no PR.
- [ ] `docker compose up` sobe o sistema inteiro do zero.
- [ ] URL pública acessível com fluxo demo funcional.
- [ ] README permite a um terceiro rodar localmente sem ajuda.
- [ ] Todos os 5 níveis de criptografia documentados em `/docs` e citados no README.

---

## 6. Backlog Detalhado

Formato: **Épico → Histórias** (com aceite resumido). Histórias pensadas para fatiamento vertical e TDD.

### Épico A — Fundação & Arquitetura
- **A1.** Como dev, quero a solução com 4 camadas + projetos de teste para começar com a base correta. *(Aceite: compila; regra de dependência testada.)*
- **A2.** Como dev, quero CI que rode build+testes em todo PR. *(Aceite: PR vermelho bloqueia merge.)*
- **A3.** Como dev, quero `Result<T>` e tratamento global de erros padronizados.
- **A4.** Como dev, quero pipeline behaviors (validação, logging, transação) no MediatR.

### Épico B — Identidade & Autenticação
- **B1.** Registrar usuário (e-mail único, senha forte, hash de senha). *(Aceite: senha nunca em claro; e-mail duplicado rejeitado.)*
- **B2.** Login com emissão de JWT. *(Aceite: credencial inválida → 401; válida → token.)*
- **B3.** Proteção de endpoints por autenticação e por papel (`User`/`Admin`).
- **B4.** (Opcional) Refresh token.

### Épico C — Gestão de Documentos
- **C1.** Upload de documento com validação (tipo, tamanho). *(Aceite: tipo inválido rejeitado.)*
- **C2.** Cálculo e persistência do hash SHA-256 (Nível 1). *(Aceite: hash determinístico e armazenado.)*
- **C3.** Criptografia AES do conteúdo em repouso (Nível 2). *(Aceite: storage nunca contém texto claro.)*
- **C4.** Listar documentos do usuário; obter detalhe; download com verificação de integridade.
- **C5.** Verificação de integridade na leitura. *(Aceite: byte alterado → falha detectada.)*

### Épico D — Assinatura Digital
- **D1.** Assinar hash do documento com RSA (Nível 3). *(Aceite: assinatura verificável.)*
- **D2.** Validar assinatura. *(Aceite: documento adulterado → inválido.)*
- **D3.** Validar certificado X509 (Nível 4): validade, cadeia, uso de chave. *(Aceite: expirado → rejeitado.)*
- **D4.** Assinar usando certificado PFX/PKCS#12 (Nível 5). *(Aceite: PFX carregado de secret; senha nunca em código.)*
- **D5.** Impedir reassinatura inválida (invariante de domínio).

### Épico E — Auditoria
- **E1.** Registrar evento de auditoria em ações sensíveis (upload, assinatura, validação, login).
- **E2.** Consultar trilha de auditoria por documento e por usuário.
- **E3.** Garantir imutabilidade lógica dos eventos (append-only).

### Épico F — Frontend
- **F1.** Tela de login/registro + guard + interceptor JWT.
- **F2.** Lista e upload de documentos (Angular Material).
- **F3.** Ações de assinar/validar com feedback visual de resultado.
- **F4.** Tela de auditoria.
- **F5.** Dashboard administrativo (papel Admin).
- **F6.** Tratamento de erros e estados de carregamento (RxJS/signals).

### Épico G — Qualidade, DevOps & Deploy
- **G1.** SonarQube + Quality Gate no CI.
- **G2.** Cobertura de código publicada (coverlet + relatório) + badge.
- **G3.** Dockerfiles + docker-compose.
- **G4.** Deploy público (API, Postgres, Web).
- **G5.** Health checks e configuração por variáveis de ambiente.

### Épico H — Documentação
- **H1.** README profissional.
- **H2.** ADRs.
- **H3.** `/docs/ai` com prompts versionados.
- **H4.** Diagramas (auth, assinatura) e screenshots.

**Sugestão de priorização (MoSCoW):** Must = A, B, C, D, E, G1–G4, H1–H3. Should = F5, B4, H4. Could = melhorias da Seção 13.

---

## 7. Estratégia de TDD

### 7.1 Disciplina

Ciclo **Red → Green → Refactor** por unidade de comportamento. Commits pequenos: idealmente um commit com o teste vermelho e outro tornando-o verde — o histórico **prova** o TDD ao revisor.

Pirâmide de testes:
- **Base — Unit (Domain):** invariantes e VOs, sem mocks (são puros).
- **Meio — Unit (Application handlers):** com NSubstitute para as portas.
- **Integração:** Infrastructure com **Testcontainers** (PostgreSQL real); API com `WebApplicationFactory`.
- **Topo — Arquitetura:** teste que falha se a regra de dependência for violada.

Ferramentas: **xUnit** (runner), **FluentAssertions** (asserções — ver nota de licença na Seção 3.4), **NSubstitute** (test doubles).

### 7.2 Roteiro TDD por funcionalidade

Para cada feature: (1) testes a escrever primeiro, (2) comportamento a validar, (3) implementação que surge, (4) quando refatorar.

#### Registrar usuário (B1)
1. **Testes primeiro:** e-mail inválido é rejeitado; senha fraca é rejeitada; e-mail duplicado é rejeitado; sucesso retorna id e persiste hash de senha (não a senha).
2. **Validar:** invariantes do VO `Email`, política de senha, unicidade.
3. **Implementação:** VO `Email`, `RegisterUserCommand` + handler + validator + `IUserRepository`, hashing de senha.
4. **Refatorar:** extrair política de senha; remover duplicação entre validator e domínio.

#### Login + JWT (B2)
1. **Testes:** credencial inválida → falha; válida → token com claims corretos e expiração.
2. **Validar:** verificação de senha (comparação resistente a timing via algoritmo de hashing) e emissão do token.
3. **Implementação:** handler de login, `IJwtTokenGenerator`.
4. **Refatorar:** isolar configuração do token; testar expiração com `IClock` fake.

#### Upload + SHA-256 (C1, C2)
1. **Testes:** tipo/tamanho inválido rejeitado; hash de um input conhecido bate com valor esperado; hash é determinístico; documento persiste com hash.
2. **Validar:** integridade calculada na borda de entrada.
3. **Implementação:** `IHashService`/`Sha256HashService`, `UploadDocumentCommand`+handler+validator.
4. **Refatorar:** separar leitura do arquivo do cálculo de hash.

#### AES em repouso (C3, C5)
1. **Testes:** round-trip encrypt→decrypt devolve o original; texto cifrado ≠ claro; conteúdo adulterado no repositório falha na descriptografia autenticada/verificação de integridade.
2. **Validar:** confidencialidade + detecção de adulteração (modo autenticado).
3. **Implementação:** `IEncryptionService`/`AesEncryptionService` (AES-256-GCM), com nonce/IV por documento.
4. **Refatorar:** padronizar formato do envelope (nonce∥tag∥ciphertext).

#### Assinatura RSA (D1, D2)
1. **Testes:** assinatura de um hash conhecido é verificável; documento adulterado → verificação falha; chave pública errada → falha.
2. **Validar:** autenticidade e não-adulteração.
3. **Implementação:** `ISignatureService`/`RsaSignatureService` (RSA-PSS sobre o hash), `SignDocumentCommand`, `ValidateSignatureQuery`.
4. **Refatorar:** separar "assinar bytes" de "assinar documento" (regra de negócio).

#### Validação X509 (D3)
1. **Testes:** certificado válido aceito; expirado rejeitado; uso de chave incompatível rejeitado; cadeia inválida rejeitada.
2. **Validar:** confiança no certificado antes de confiar na assinatura.
3. **Implementação:** `X509Validator` com certificados de teste gerados no setup.
4. **Refatorar:** mapear resultados de validação para um relatório estruturado.

#### Assinatura via PFX (D4)
1. **Testes:** PFX carregado com senha correta expõe chave; senha errada falha; assinatura feita com a chave do PFX é validável pelo certificado contido.
2. **Validar:** uso de material de chave real (simulado A1) sem expor segredo.
3. **Implementação:** `PfxCertificateStore` lendo de configuração/secret; integração com `SignDocument`.
4. **Refatorar:** unificar caminho de assinatura RSA puro vs. via certificado.

#### Auditoria (E1–E3)
1. **Testes:** ação sensível gera exatamente um evento com ator/ação/alvo/timestamp; eventos são append-only.
2. **Validar:** rastreabilidade e imutabilidade.
3. **Implementação:** `IAuditLogger`, behavior/decorator que registra após sucesso.
4. **Refatorar:** centralizar via pipeline behavior do MediatR.

#### Teste de arquitetura (A1)
- Regra executável: Domain não referencia Application/Infrastructure/API; Application não referencia Infrastructure/API. Falha de compilação/relação reprova o build.

### 7.3 Frontend (testes)
- Componentes e serviços testados com o runner do Angular (TestBed); serviços HTTP com `HttpTestingController`. Foco em guard, interceptor e fluxo de assinatura/validação.

---

## 8. Estratégia de Segurança

Progressão didática em 5 níveis. Para cada nível: **conceito → objetivo → onde no projeto → ordem ideal → cuidados de segurança.**

### Nível 1 — SHA-256 (integridade)
- **Conceito:** função de hash criptográfica unidirecional; mesma entrada → mesmo digest; mudança mínima → digest totalmente diferente.
- **Objetivo:** provar integridade do documento.
- **Onde:** calculado no upload e re-verificado na leitura/validação.
- **Ordem:** 1º (mais simples; habilita assinatura depois, que assina o hash).
- **Cuidados:** hash **não** é confidencialidade nem autenticação; para integridade de mensagens autenticadas usar HMAC; nunca usar MD5/SHA-1.

### Nível 2 — AES (confidencialidade em repouso)
- **Conceito:** cifra simétrica de bloco; mesma chave cifra e decifra.
- **Objetivo:** proteger o conteúdo armazenado.
- **Onde:** conteúdo do documento cifrado antes de persistir.
- **Ordem:** 2º.
- **Cuidados:** usar **modo autenticado (AES-GCM)** para detectar adulteração; **nonce/IV único** por operação (nunca reutilizar com a mesma chave); chave vinda de secret/env, nunca hardcoded; considerar envelope encryption (chave de dados protegida por chave mestra).

### Nível 3 — RSA (assinatura digital)
- **Conceito:** criptografia assimétrica; assina-se com a chave privada, verifica-se com a pública.
- **Objetivo:** autenticidade + não-repúdio da assinatura.
- **Onde:** assinar o **hash** do documento (não o conteúdo inteiro), validar na verificação.
- **Ordem:** 3º (depende do hash do Nível 1).
- **Cuidados:** usar **RSA-PSS** (não PKCS#1 v1.5 para novas assinaturas); chave ≥ 2048 bits; **chave privada nunca sai do servidor / nunca é logada**; assinar digest, não bytes arbitrários grandes.

### Nível 4 — Certificados X509 (validação de identidade)
- **Conceito:** certificado liga uma chave pública a uma identidade, assinado por uma Autoridade Certificadora.
- **Objetivo:** validar em quem/no quê confiar antes de aceitar uma assinatura.
- **Onde:** ao validar uma assinatura, validar o certificado associado.
- **Ordem:** 4º.
- **Cuidados:** verificar **validade temporal**, **cadeia de confiança**, **key usage / extended key usage**, e (conceitualmente) **revogação** (CRL/OCSP); não confiar em certificado autoassinado fora de contexto controlado.

### Nível 5 — Certificados PFX / PKCS#12 (uso realista)
- **Conceito:** container PKCS#12 com certificado + chave privada, protegido por senha (formato de certificados A1).
- **Objetivo:** simular o uso de certificado digital real na assinatura.
- **Onde:** carregar PFX e assinar com a chave do certificado.
- **Ordem:** 5º (integra Níveis 3 e 4).
- **Cuidados:** **senha e PFX vêm de secret/variável de ambiente**, nunca do repositório; PFX de teste **não** versionado (ou claramente marcado como dummy de teste); liberar material de chave da memória quando possível; nunca logar a senha.

### Segurança transversal (aplicação inteira)
- Senhas com hashing forte e *salt* (algoritmo dedicado, não SHA cru).
- JWT com expiração curta, segredo forte via env, validação de assinatura/emissor/audiência.
- Autorização por papel; documentos só acessíveis pelo dono/Admin.
- Validação de entrada (FluentValidation) + limites de upload + checagem de tipo.
- Tratamento de erros sem vazar stack trace/segredos ao cliente.
- HTTPS, CORS restrito, headers de segurança, rate limiting básico.
- Segredos fora do código (env / secret manager); `.env` no `.gitignore`.
- Comparações sensíveis resistentes a timing.

> O fluxo "assinar → adulterar → validar falha" é a **demonstração central de segurança** do portfólio. Deve estar em teste automatizado **e** roteirizado para a entrevista.

---

## 9. Estratégia de Uso de IA

Objetivo: demonstrar **engenharia assistida por IA disciplinada** — IA como acelerador e revisor, com o desenvolvedor mantendo o julgamento técnico. Todos os prompts relevantes ficam versionados em `/docs/ai`, mostrando o *método*, não só o resultado.

### 9.1 Planejamento (descoberta, backlog, quebra de tarefas)
- Usar Claude Code para transformar requisitos em épicos/histórias com critérios de aceite.
- Refinar backlog: identificar dependências entre histórias e sugerir ordem de fatiamento vertical.
- Quebrar histórias em passos TDD (lista de testes a escrever primeiro).
- Registrar os prompts em `docs/ai/architecture-prompts.md` (parte de planejamento) e no `docs/ai/README.md`.

### 9.2 Desenvolvimento (testes, revisão, refatoração, documentação)
- **Geração de testes:** pedir a lista de casos de teste (incluindo bordas/negativos) **antes** da implementação — alinhado ao TDD. Prompts em `tdd-prompts.md`.
- **Revisão de código:** revisar o próprio diff antes do PR (bugs, edge cases, clareza). Usar o fluxo `/code-review`. Prompts em `review-prompts.md`.
- **Refatoração:** identificar duplicação e melhorar nomes/estrutura mantendo os testes verdes. Prompts em `refactor-prompts.md`.
- **Documentação:** gerar rascunhos de README, ADRs e docstrings, sempre revisados pelo autor.

### 9.3 Arquitetura
- Revisar decisões arquiteturais (trade-offs de CQRS, storage, auth).
- Gerar **ADRs** a partir de uma decisão tomada (contexto, decisão, consequências).
- Detectar violações de arquitetura (apoio ao teste de arquitetura automatizado).
- Prompts em `architecture-prompts.md`.

### 9.4 Segurança
- **Security reviews** do diff (fluxo `/security-review`).
- **Threat modeling** leve (STRIDE) sobre os fluxos de auth e assinatura.
- Identificação de vulnerabilidades comuns (segredos no código, validação ausente, criptografia mal usada).
- Prompts em `security-prompts.md`.

### 9.5 Estrutura obrigatória `/docs/ai`
```
docs/ai/
├─ README.md                # filosofia de uso de IA no projeto + índice
├─ architecture-prompts.md  # planejamento + decisões + ADRs + violações
├─ tdd-prompts.md           # geração de listas de testes / casos de borda
├─ review-prompts.md        # revisão de código / PRs
├─ security-prompts.md      # security review / threat modeling
└─ refactor-prompts.md      # refatorações seguras
```
Cada arquivo: o prompt usado, **por que** foi usado, e uma nota curta de **o que foi aceito/rejeitado** da resposta da IA — isso evidencia julgamento crítico, não cópia cega.

> Diferencial real: mostrar onde a sugestão da IA foi **rejeitada** e o porquê. Demonstra que IA é ferramenta sob controle do engenheiro.

---

## 10. Estratégia de CI/CD

### 10.1 Fluxo de trabalho (quando configurar)
- **GitHub Flow** desde o início: `main` sempre verde; trabalho em branches curtas (`feat/...`, `fix/...`); merge via PR.
- **Conventional Commits** desde o primeiro commit (`feat:`, `fix:`, `test:`, `docs:`, `chore:`, `refactor:`). Benefício: histórico legível e changelog automatizável.
- **Pull Requests** com template (descrição, checklist, como testar). Benefício: disciplina de revisão visível ao recrutador.

### 10.2 Pipeline CI (`ci.yml`) — configurar na Semana 1
Estágios, em ordem:
1. Restore + Build (`TreatWarningsAsErrors`).
2. Testes (unit + integração; Testcontainers exige Docker no runner).
3. Cobertura (**coverlet**) + relatório (ReportGenerator).
4. Análise **SonarQube** (qualidade + cobertura importada).
5. Quality Gate: build falha se reprovar.

Benefícios por ferramenta:
- **GitHub Actions:** automação por PR; impede merge de código quebrado.
- **coverlet + cobertura:** visibilidade objetiva de teste; vira badge.
- **SonarQube:** code smells, bugs, segurança, duplicação, e **Quality Gate** que reprova PR fora do padrão.

### 10.3 Quality Gate (recomendado)
- Cobertura global ≥ 70% (Domain/Application ≥ 80%).
- Zero novos bugs/vulnerabilidades "blocker"/"critical".
- Duplicação abaixo do limite do Sonar.
- Sem novos code smells "blocker".

### 10.4 CD (`cd.yml`) — Semana 4
- Build das imagens Docker (API e Web).
- Deploy disparado por merge em `main` ou manualmente (`workflow_dispatch`).
- Migrations aplicadas de forma controlada (passo explícito, não silencioso).

---

## 11. Estratégia de Deploy

### 11.1 Objetivo
URL pública acessível por recrutadores, com usuário demo e dados de exemplo (seed).

### 11.2 Dockerização
- **API:** Dockerfile multi-stage (build SDK → runtime enxuto), usuário não-root, porta exposta, health check.
- **Frontend:** build de produção do Angular servido por servidor estático leve (ex.: Nginx) em Dockerfile multi-stage.
- **docker-compose.yml:** orquestra `api` + `web` + `postgres` com volumes e rede; `.env` para segredos locais.

### 11.3 Banco PostgreSQL
- Local/CI: container PostgreSQL.
- Produção: instância gerenciada do provedor de hospedagem; migrations aplicadas em passo de deploy controlado; backup mínimo.

### 11.4 Variáveis de ambiente (nunca em código)
- `ConnectionStrings__Default`, `Jwt__Secret`, `Jwt__Issuer`/`Audience`, `Aes__MasterKey` (ou referência ao secret manager), `Pfx__Password`/caminho do PFX, `Cors__AllowedOrigins`, `ASPNETCORE_ENVIRONMENT`.
- Documentar todas em `.env.example` (sem valores reais).

### 11.5 Hospedagem (opções de baixo custo)
- **API + PostgreSQL:** provedores com tier gratuito/baixo custo que rodam container + Postgres gerenciado (ex.: Render, Railway, Fly.io) ou Azure (App Service + Postgres) caso queira alinhar com o ecossistema .NET.
- **Frontend:** hospedagem de estáticos (ex.: Vercel, Netlify, Azure Static Web Apps, ou o mesmo container Nginx).
- **CORS** configurado para o domínio do frontend; **HTTPS** obrigatório.

### 11.6 Demonstração
- Seed de usuário demo (`demo@.../senha demo`) e 1–2 documentos de exemplo.
- README com a URL e as credenciais demo destacadas.

---

## 12. Riscos Técnicos

| # | Risco | Impacto | Mitigação |
|---|-------|---------|-----------|
| R1 | Escopo grande para 3–4 semanas (júnior) | Projeto inacabado / features frágeis | Fatiamento vertical; MoSCoW; cortes seguros (dashboard, refresh token, PAdES são "could/should"). Entregar fluxo central impecável primeiro. |
| R2 | Criptografia mal aplicada (IV reusado, padding fraco, segredo no código) | Falha de segurança que um sênior detecta na hora | Seguir Seção 8; AES-GCM; RSA-PSS; segredos em env; cobrir com testes negativos; rodar `/security-review`. |
| R3 | Segredos/PFX vazarem no Git | Exposição real de material sensível | `.gitignore` para `.env`/PFX; PFX de teste é dummy; scan de segredos no CI; nunca logar segredos. |
| R4 | Licenças comerciais (MediatR, FluentAssertions v8) | Uso indevido / inconsistência | ADR-0008: fixar versões no tier gratuito ou trocar por OSS (Shouldly/AwesomeAssertions, mediator próprio). Decisão documentada. |
| R5 | Testcontainers exige Docker no runner do CI | Testes de integração falham no CI | Usar runner com Docker; separar job de integração; fallback para Postgres de serviço do Actions. |
| R6 | Mudanças de template do .NET 10 (OpenAPI sem Swashbuckle) | Tempo perdido configurando docs | Usar `Microsoft.AspNetCore.OpenApi` + Scalar; documentar no ADR. |
| R7 | Complexidade de validação X509 (cadeia/revogação) | Atraso ou validação incorreta | Escopo claro: validade + cadeia + key usage; revogação tratada conceitualmente (documentada como futura). |
| R8 | Free tier de hospedagem dorme/limita | Demo lenta/indisponível na entrevista | Aquecer antes; ter `docker compose up` local como plano B; gravar GIF/vídeo do fluxo. |
| R9 | Frontend consumir mais tempo que o previsto | Backend forte, UI fraca | UI mínima funcional com Angular Material; priorizar fluxo de assinatura/validação sobre estética. |
| R10 | Cobertura inflada (testar trivial, não comportamento) | Métrica boa, qualidade fraca | Focar testes em invariantes de domínio e regras de Application; cobertura é consequência, não meta isolada. |

---

## 13. Melhorias Futuras (v2.0)

- Assinatura embarcada em PDF no padrão **PAdES** (assinatura visível no documento).
- **Carimbo de tempo (TSA / RFC 3161)** para prova temporal.
- Verificação de **revogação** real via OCSP/CRL.
- Integração com **provedor de assinatura em nuvem** ou HSM/Azure Key Vault para guarda de chaves.
- Suporte a **múltiplos signatários** e fluxos de aprovação (workflow).
- **Webhooks**/notificações por e-mail ao assinar/validar.
- **Refresh tokens** + revogação de sessão + MFA.
- Internacionalização (i18n) PT/EN no frontend.
- Observabilidade: logs estruturados, métricas, tracing (OpenTelemetry).
- Verificação pública de assinatura por link (validação sem login).
- Suporte a certificados **A3** (token/smartcard) — conceitual.

---

## 14. Checklist Final

### Fundação & Arquitetura
- [ ] Estrutura Clean Architecture criada (Domain / Application / Infrastructure / API)
- [ ] Regra de dependência garantida por teste de arquitetura
- [ ] `Directory.Build.props` com nullable + warnings-as-errors
- [ ] `.editorconfig` e `.gitignore` configurados
- [ ] `Result<T>` e tratamento global de erros padronizados
- [ ] Pipeline behaviors (validação/logging/transação) configurados

### CI/CD & Qualidade
- [ ] GitHub Flow em uso (branches + PRs)
- [ ] Conventional Commits adotados
- [ ] Template de Pull Request criado
- [ ] Pipeline CI configurado (build + testes)
- [ ] SonarQube configurado
- [ ] Quality Gate ativo e "Passed"
- [ ] Cobertura de código publicada + badge
- [ ] Cobertura mínima atingida (global ≥ 70%, Domain/Application ≥ 80%)

### Testes (TDD)
- [ ] Testes unitários de Domain implementados
- [ ] Testes unitários de Application (handlers) implementados
- [ ] Testes de integração com Testcontainers (PostgreSQL)
- [ ] Testes de integração de API (WebApplicationFactory)
- [ ] Histórico de commits evidencia TDD (teste antes da implementação)

### Identidade & Autenticação
- [ ] Cadastro de usuário implementado (senha com hashing forte)
- [ ] Login com JWT implementado
- [ ] Autorização por papel (User/Admin)

### Documentos & Criptografia
- [ ] Upload de documentos implementado
- [ ] SHA-256 implementado (integridade)
- [ ] AES implementado (criptografia em repouso, modo autenticado)
- [ ] Verificação de integridade na leitura (byte alterado → falha)
- [ ] RSA implementado (assinatura sobre hash, RSA-PSS)
- [ ] X509 implementado (validade + cadeia + key usage)
- [ ] Certificados PFX/PKCS#12 implementados (carregados de secret)
- [ ] Fluxo "assinar → adulterar → validação falha" coberto por teste

### Auditoria & Frontend
- [ ] Trilha de auditoria implementada (append-only)
- [ ] Consulta de auditoria por documento/usuário
- [ ] Frontend de login/registro (guard + interceptor JWT)
- [ ] Frontend de documentos (lista + upload, Angular Material)
- [ ] Frontend de assinar/validar com feedback de resultado
- [ ] Dashboard administrativo implementado

### Segurança transversal
- [ ] Segredos fora do código (env/secret manager; `.env.example` documentado)
- [ ] PFX/segredos fora do Git (`.gitignore`); PFX de teste é dummy
- [ ] HTTPS + CORS restrito + headers de segurança + rate limiting básico
- [ ] Erros sem vazar stack trace/segredos
- [ ] Security review executado (`/security-review`)
- [ ] Threat modeling leve documentado (auth + assinatura)

### Documentação & IA
- [ ] Swagger/OpenAPI configurado (Microsoft.AspNetCore.OpenApi + UI)
- [ ] ADRs escritos (incl. ADR de licenças comerciais)
- [ ] `/docs/ai` completo (architecture/tdd/review/security/refactor)
- [ ] Diagramas de fluxo (autenticação + assinatura digital)
- [ ] Screenshots adicionados
- [ ] README profissional concluído (visão, arquitetura, tecnologias, como executar/testar, CI/CD, fluxos)

### Deploy
- [ ] Dockerfile da API
- [ ] Dockerfile do Frontend
- [ ] docker-compose.yml (api + web + postgres) funcional
- [ ] Variáveis de ambiente documentadas
- [ ] Deploy público realizado (URL acessível)
- [ ] Usuário/dados demo (seed) disponíveis
- [ ] Plano B de demonstração (GIF/vídeo + `docker compose up` local)

### Pronto para entrevista
- [ ] Fluxo completo demonstrável ponta a ponta
- [ ] Os 5 níveis de criptografia explicáveis verbalmente
- [ ] Roteiro de demo de 5 minutos preparado
- [ ] Decisões arquiteturais e trade-offs articuláveis

---

*Plano vivo: ajuste conforme o avanço real. Priorize sempre o fluxo central (upload → assinar → validar → auditar) impecável antes de ampliar escopo.*
