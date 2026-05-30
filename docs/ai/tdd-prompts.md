# TDD Prompts

## Exemplo 1 — Casos de teste para Email VO

**Prompt enviado:**
> "Vou implementar um Value Object Email em C#. Liste todos os casos de teste que devo escrever ANTES de codificar, incluindo casos de borda."

**Aceito:**
- Valid: formato correto, maiúsculas normalizadas, tag (+), subdomínio
- Invalid: null/vazio, sem @, sem TLD, > 320 chars, não-hex
- Equality: dois Emails com mesmo valor são iguais (record)

**Rejeitado:**
- Sugestão de testar internacionalização (IDN domains) → fora do escopo do portfólio; documentado como futura melhoria.

---

## Exemplo 2 — Testes do RegisterUserHandler

**Prompt enviado:**
> "Handler RegisterUser tem: IUserRepository, IPasswordHasher, IUnitOfWork. Quais testes escrever primeiro com NSubstitute antes de implementar o handler?"

**Aceito:**
- Sucesso retorna id (não vazio)
- Email duplicado retorna falha com mensagem específica
- Senha fraca retorna falha
- Email inválido retorna falha
- `PasswordHash` armazenado (nunca a senha em claro)
- `SaveChangesAsync` chamado exatamente 1x em sucesso

**Rejeitado:**
- Sugestão de testar via WebApplicationFactory no unit test → separado para integração. Unit test = sem banco, sem HTTP.

---

## Exemplo 3 — Testes de AES

**Prompt enviado:**
> "Quero testar AesEncryptionService com AES-256-GCM. Quais testes garantem que a implementação está correta E segura?"

**Aceito:**
- Round-trip: decrypt(encrypt(x)) == x
- Ciphertext diferente do plaintext
- Dois encrypts do mesmo input geram ciphertexts diferentes (nonce único)
- Byte adulterado no ciphertext lança exceção (autenticação GCM)

**Rejeitado:**
- Sugestão de test vector com chave conhecida → válido, mas requer fixtures externas. Adicionado como TODO para aprimoramento futuro.
