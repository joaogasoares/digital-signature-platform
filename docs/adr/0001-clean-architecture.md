# ADR-0001 — Clean Architecture com CQRS leve via MediatR

**Status:** Aceito  
**Data:** 2026-05-30  
**Autores:** João Gabriel

---

## Contexto

O projeto precisa demonstrar domínio de arquitetura de software para uma vaga em empresa de identidade digital. A estrutura deve ser:
- Testável por unidade sem depender de banco, framework ou criptografia real.
- Clara o suficiente para um avaliador sênior entender o design em 5 minutos.
- Extensível sem reescrita (troca de ORM, storage, algoritmo criptográfico não deve tocar regras de negócio).

## Decisão

Adotar **Clean Architecture** (Robert C. Martin) com quatro camadas, e **CQRS leve** via MediatR para organizar casos de uso.

### Camadas e regra de dependência

```
Domain  ←  Application  ←  Infrastructure  ←  Api
```

Dependências só apontam para dentro. Nenhuma camada interna conhece a externa.

| Camada | Responsabilidade | Dependências permitidas |
|--------|-----------------|------------------------|
| Domain | Entidades, VOs, invariantes, eventos de domínio | Nenhuma externa |
| Application | Comandos/queries (MediatR), ports (interfaces), validators | Apenas Domain |
| Infrastructure | EF Core, crypto, storage, implementações de ports | Domain + Application |
| Api | Endpoints HTTP, DI composition root, middleware | Todas (só para compor DI) |

### CQRS leve com MediatR

Cada caso de uso = um `Command` ou `Query` + `Handler`. Permite:
- Adicionar pipeline behaviors (validação, logging, transação) sem alterar handlers.
- Testar handlers isoladamente com mocks das ports.

### Regra executável

A dependência é garantida por teste automatizado (`DigitalSignature.ArchitectureTests`), não apenas por convenção. Violação quebra o CI.

## Consequências

**Positivas:**
- Domain puro (zero dependências NuGet) — testável sem nenhuma infra.
- Troca de ORM, storage ou algoritmo criptográfico: só toca Infrastructure.
- Padrão reconhecível por qualquer desenvolvedor sênior .NET.

**Negativas / trade-offs:**
- Mais boilerplate que um projeto CRUD simples (Command + Handler + Validator por feature).
- MediatR v11.x (versão gratuita) — decisão de licença documentada em ADR-0008.
- Overhead de indireção para features muito simples (aceitável dado o objetivo de portfólio).

## Alternativas consideradas

- **MVC sem separação de camadas:** rápido de montar, mas não demonstra design. Descartado.
- **Vertical Slice Architecture:** válida, mas menos reconhecida como padrão em entrevistas do domínio. Descartada.
- **CQRS full (buses, eventos de integração):** complexidade desnecessária para o escopo. CQRS leve suficiente.
