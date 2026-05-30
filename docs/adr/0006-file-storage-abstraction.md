# ADR-0006 — Abstração de storage via IFileStorage

**Status:** Aceito  
**Data:** 2026-05-30

## Decisão

Abstrair o storage de arquivos via `IFileStorage` (porta em Application). Implementação atual: `FileSystemStorage` (filesystem local).

## Justificativa

- Domain e Application não conhecem "onde" o arquivo fica — apenas que pode ser salvo, lido e deletado.
- Troca para S3, Azure Blob ou GCS = nova implementação de `IFileStorage` sem tocar regra de negócio.
- Testes de Application usam mock de `IFileStorage` (NSubstitute) sem I/O real.

## Implementação atual

`FileSystemStorage`:
- Arquivos salvos em `Storage:BasePath` (configurável via env).
- Organização por data: `yyyy/MM/dd/{guid}{ext}`.
- Retorna path relativo (portável entre ambientes).

## Consequências

- Para produção real: substituir por Azure Blob + SAS token ou S3. Zero mudança na Application.
- Cuidado: `FileSystemStorage` não funciona em múltiplas instâncias do container sem volume compartilhado. Aceitável para portfólio com deploy de instância única.
