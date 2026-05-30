# Review Prompts

## Exemplo 1 — Code review do RegisterUserHandler

**Prompt enviado:**
> "Revise este handler do ponto de vista de: bugs, edge cases, clareza, e alinhamento com Clean Architecture."

**Problemas identificados e corrigidos:**
- `Email.Create` e `PasswordPolicy.Validate` lançavam exceções; handler deve retornar `Result.Failure`, não deixar propagar → `try/catch` adicionado.
- `email.Value` (não `request.Email`) passado para `ExistsByEmailAsync` → email normalizado (lowercase) antes da verificação de duplicidade.

**Sem ação:**
- Sugestão de adicionar logging estruturado no handler → responsabilidade do pipeline behavior (MediatR), não do handler.

---

## Exemplo 2 — Review da DocumentConfiguration

**Prompt enviado:**
> "Revise este IEntityTypeConfiguration<Document> para EF Core. Problemas de mapeamento ou performance?"

**Aceito:**
- `OwnsOne(d => d.Hash)` para mapeamento inline do VO `DocumentHash` → hash_value e hash_algorithm como colunas da mesma tabela.
- Índice em `OwnerId` para queries de lista por usuário.

**Rejeitado:**
- Sugestão de JSON column para o VO → desnecessário; duas colunas simples são mais consultáveis e indexáveis.

---

## Exemplo 3 — Review do endpoint de upload

**Prompt enviado:**
> "Este endpoint usa IFormFile. Quais riscos de segurança e o que melhorar?"

**Aceito:**
- Validação de ContentType movida para o handler (não confiar no Content-Type do browser).
- Limite de 10 MB validado no handler, não apenas no servidor HTTP.
- Desabilitar antiforgery para APIs REST (`DisableAntiforgery`).

**Rejeitado:**
- Sugestão de scanear conteúdo do arquivo com antivírus → over-engineering para portfólio. Documentado como futura melhoria de produção.
