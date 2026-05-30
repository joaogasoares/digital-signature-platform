# ADR-0002 — PostgreSQL como banco de dados + EF Core como ORM

**Status:** Aceito  
**Data:** 2026-05-30  
**Autores:** João Gabriel

---

## Contexto

O projeto precisa de persistência relacional para usuários, documentos, assinaturas e auditoria. A escolha deve equilibrar robustez, custo de hospedagem (tier gratuito para portfólio) e alinhamento com o ecossistema .NET.

## Decisão

**PostgreSQL 16** como banco de dados relacional.  
**EF Core 9.x com Npgsql** como ORM + provedor.

### Justificativas

**PostgreSQL:**
- Tier gratuito em provedores populares (Render, Railway, Fly.io, Supabase).
- Suporte nativo a UUID, tipos JSON, arrays — adequado para os dados do projeto.
- Open-source, produção-grade, amplamente adotado em empresas de identidade digital.

**EF Core:**
- ORM padrão do ecossistema .NET; reconhecido imediatamente em entrevistas.
- Migrations versionadas no repositório — rastreabilidade e reprodutibilidade.
- Abstração via `DbContext` permite testar Infrastructure com banco real (Testcontainers) sem impactar Domain/Application.

**Versão 9.x (não 10.x):**
- EF Core 10.x exige .NET 10 exclusivamente; 9.x suporta net10.0 e evita incompatibilidades com Npgsql que ainda não tem release estável para EF 10.

## Consequências

**Positivas:**
- Migrations no Git: histórico de evolução do schema rastreável.
- `AppDbContext` isolado em Infrastructure — Domain e Application não conhecem EF Core.
- Deploy gratuito possível em múltiplos provedores.

**Negativas / trade-offs:**
- Migrations devem ser aplicadas manualmente em produção (nunca auto-migrate silencioso).
- EF Core adiciona dependência pesada a Infrastructure — aceitável; Infrastructure é o lugar correto para isso.
- Npgsql não suportará EF Core 10 até release oficial; fixar em 9.x até então.

## Alternativas consideradas

- **SQL Server / Azure SQL:** custo em produção, menos portável. Descartado.
- **Dapper:** mais controle, mas sem migrations nativas. Overhead para portfólio. Descartado.
- **SQLite:** simples para dev, mas não representa produção. Descartado para o banco principal.
