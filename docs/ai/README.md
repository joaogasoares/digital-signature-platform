# AI-Assisted Engineering — Digital Signature Platform

Este diretório contém os prompts reais utilizados com **Claude Code** durante o desenvolvimento do projeto. O objetivo é demonstrar *engenharia assistida por IA disciplinada*: IA como acelerador e revisor, com o desenvolvedor mantendo o julgamento técnico.

## Filosofia de uso

- IA sugere → desenvolvedor decide.
- O que foi **aceito** está no código. O que foi **rejeitado** está documentado aqui — com o motivo.
- Demonstrar onde a IA errou é mais valioso do que mostrar só onde acertou.

## Índice

| Arquivo | Conteúdo |
|---|---|
| [architecture-prompts.md](architecture-prompts.md) | Decisões de arquitetura, ADRs, planejamento de camadas |
| [tdd-prompts.md](tdd-prompts.md) | Geração de casos de teste, disciplina TDD |
| [review-prompts.md](review-prompts.md) | Code review de diffs, PRs |
| [security-prompts.md](security-prompts.md) | Security review, threat modeling |
| [refactor-prompts.md](refactor-prompts.md) | Refatorações seguras |

---

## Formato dos exemplos

Cada exemplo tem:
1. **Prompt enviado**
2. **O que foi aceito** (com link para o código)
3. **O que foi rejeitado** (com motivo)
