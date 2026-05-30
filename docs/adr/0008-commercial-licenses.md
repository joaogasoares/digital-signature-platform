# ADR-0008 — Licenças comerciais em dependências (2025)

**Status:** Aceito  
**Data:** 2026-05-30

## Contexto

Algumas bibliotecas populares do ecossistema .NET mudaram para licença comercial em 2025:

| Biblioteca | Mudança | Versão afetada |
|---|---|---|
| MediatR | Licença comercial para uso em empresa | v12+ |
| FluentAssertions | Licença comercial | v8+ |

## Decisão

Fixar nas versões OSS (Apache 2.0 / MIT) para o portfólio:
- **MediatR 11.1.0** + **MediatR.Extensions.Microsoft.DependencyInjection 11.1.0** — última versão Apache 2.0.
- **FluentAssertions 7.2.0** — última versão MIT.

## Alternativas open-source mapeadas

| Substituível por | Alternativa OSS |
|---|---|
| FluentAssertions | Shouldly, AwesomeAssertions |
| MediatR | Mediator (Mediator.Net), custom IMediator simples |

## Por que isso importa

Demonstrar consciência de licenciamento é sinal de maturidade para recrutadores sêniores. Um projeto de portfólio usando bibliotecas comerciais sem licença é um red flag.
