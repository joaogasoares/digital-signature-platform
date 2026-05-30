# Refactor Prompts

## Exemplo 1 — Extrair política de senha

**Prompt enviado:**
> "RegisterUserHandler valida senha inline. PasswordPolicy está no Domain. Como unificar sem duplicação e sem violar Clean Architecture?"

**Aceito:**
- `PasswordPolicy.Validate(password)` estático no Domain.
- Handler chama `PasswordPolicy.Validate` via try/catch e retorna `Result.Failure`.
- Sem validador FluentValidation duplicando a regra → uma fonte de verdade.

---

## Exemplo 2 — Consolidar fluxo hash+cifra no UploadDocument

**Prompt enviado:**
> "UploadDocumentHandler faz: read stream → hash → encrypt → store. Há duplicação ou pode simplificar?"

**Aceito:**
- `hashService.Compute(plainBytes)` antes de `encryptionService.Encrypt(plainBytes)`.
- Uma única leitura do stream para MemoryStream → sem double-read.
- Ordem explícita: hash do plaintext (não do ciphertext), conforme ADR-0005.

**Rejeitado:**
- Sugestão de stream pipeline (hash + encrypt em passagem única) → possível mas obscurece a ordem e dificulta debugging. Manter legível.

---

## Exemplo 3 — InternalsVisibleTo vs público

**Prompt enviado:**
> "Handlers são internal. Testes precisam acessar. Devo tornar público ou usar InternalsVisibleTo?"

**Aceito:**
- `InternalsVisibleTo` em `AssemblyInfo.cs` → handlers ficam internal (encapsulamento real), apenas os projetos de teste específicos têm acesso.

**Rejeitado:**
- Sugestão de tornar handlers públicos → exporia detalhes de implementação. `internal` é a visibilidade correta para handlers; a port pública é a interface `IRequest`.
