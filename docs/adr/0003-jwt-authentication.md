# ADR-0003 — Autenticação via JWT stateless

**Status:** Aceito  
**Data:** 2026-05-30  
**Autores:** João Gabriel

---

## Contexto

O sistema precisa autenticar usuários e autorizar acesso a recursos protegidos. A estratégia deve ser simples de implementar, stateless e adequada para uma SPA Angular consumindo uma API REST.

## Decisão

**JWT (JSON Web Tokens)** com algoritmo **HMAC-SHA256** para geração e validação de tokens.

### Configuração

- Token válido por 60 minutos (configurável via `Jwt:ExpiryMinutes`).
- Claims: `sub` (user id), `email`, `role`, `jti` (token id único).
- Segredo via configuração (`Jwt:Secret`) — **nunca** no código ou no repositório.
- `ClockSkew = TimeSpan.Zero` — sem tolerância de tempo; expiração exata.
- `ValidateIssuer` e `ValidateAudience` habilitados.

### Proteção contra user enumeration

O endpoint de login retorna **exatamente a mesma mensagem** para "email não encontrado" e "senha incorreta", prevenindo enumeração de usuários. Verificado em teste específico (`Handle_wrong_password_never_reveals_reason`).

## Consequências

**Positivas:**
- Stateless: sem sessão no servidor; escala horizontalmente.
- Simples de consumir no Angular (interceptor HTTP adiciona `Authorization: Bearer`).
- Testável com `IClock` fake (expira em cenários controlados sem aguardar o relógio real).

**Negativas / trade-offs:**
- Token não pode ser invalidado antes da expiração (sem refresh token ou blacklist).
- Para produção real: usar HTTPS obrigatório + `Jwt:Secret` de 256 bits gerado aleatoriamente via secret manager.
- Refresh token não implementado (v1); listado em Melhorias Futuras.

## Alternativas consideradas

- **Cookies de sessão:** requer servidor stateful ou session store compartilhado. Descartado.
- **OAuth2 / OIDC (ex.: OpenIddict, IdentityServer):** correto para produção com múltiplos clientes, mas sobre-engenharia para o portfólio. Descartado.
