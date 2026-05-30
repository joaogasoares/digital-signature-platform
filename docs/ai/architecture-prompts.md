# Architecture Prompts

## Exemplo 1 — Escolha de arquitetura

**Prompt enviado:**
> "Sou dev júnior fazendo portfólio para vaga em empresa de certificação digital. Projeto: plataforma de assinatura de documentos. Qual arquitetura usar: Clean Architecture + CQRS, Vertical Slice, ou MVC simples? Quero impressionar seniores."

**Aceito:**
- Clean Architecture com 4 camadas (Domain/Application/Infrastructure/API)
- CQRS leve via MediatR (Command+Handler por caso de uso)
- Regra de dependência verificável por teste automatizado (NetArchTest)

**Rejeitado:**
- Sugestão de usar DDD completo com Aggregates, Domain Events propagados via bus → complexidade desnecessária para o escopo. Um documento tem vida simples; modelagem CRUD bem estruturada é suficiente.

---

## Exemplo 2 — Design de ISignatureService

**Prompt enviado:**
> "Preciso de uma interface para assinatura digital. O sistema pode ter RSA hoje e ECDSA amanhã. Mostre a interface ideal."

**Aceito:**
- Interface `ISignatureService` com `Sign`, `Verify`, `GetThumbprint`, `GetAlgorithm`, `IsCertificateValid`
- Retorno de `bool` + out param para erros de certificado (evita exceções para fluxo de negócio)

**Rejeitado:**
- Sugestão de `Task<byte[]> SignAsync(...)` → operações de chave em memória são síncronas; async adiciona overhead sem benefício para RSA local. Mantido síncrono.

---

## Exemplo 3 — Planejamento de semanas

**Prompt enviado:**
> "Tenho 4 semanas, sou júnior, quero TDD real, 5 níveis de criptografia (SHA-256, AES, RSA, X509, PFX). Quebre em semanas com passos diários."

**Aceito:**
- Semana 1: fundação + CI + RegisterUser + JWT
- Semana 2: documentos + SHA-256 + AES-GCM
- Semana 3: RSA-PSS + X509 + PFX + auditoria
- Semana 4: Docker + docs + deploy

**Rejeitado:**
- Sugestão de frontend Angular nas primeiras 2 semanas → adiado para não travar backend. "Backend sólido primeiro, UI depois."
