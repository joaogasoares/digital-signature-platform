# Security Prompts

## Exemplo 1 — Threat Modeling do fluxo de autenticação

**Prompt enviado:**
> "Faça threat modeling STRIDE simplificado do fluxo de login: POST /api/users/login com email+senha, JWT gerado, retornado ao cliente."

**Análise recebida e aceita:**
- **S (Spoofing):** mitigação → PBKDF2-SHA512 com 350k iterations; comparação timing-safe via `CryptographicOperations.FixedTimeEquals`.
- **T (Tampering):** mitigação → JWT assinado (HMAC-SHA256); payload alterado invalida assinatura.
- **I (Information disclosure):** mitigação → mesma mensagem de erro para "email não encontrado" e "senha errada" (anti-enumeração). Implementado e testado.
- **D (DoS):** mitigação básica → rate limiting recomendado (marcado como futura melhoria).
- **E (Elevation):** mitigação → `ClaimTypes.Role` no JWT; endpoints verificam papel via `[Authorize(Roles = "Admin")]`.

**Rejeitado:**
- Sugestão de adicionar refresh token imediatamente → escopo v2. JWT de 60min aceitável para portfólio.

---

## Exemplo 2 — Security review do AesEncryptionService

**Prompt enviado:**
> "Revise este AesEncryptionService do ponto de vista de segurança: [código]"

**Problemas encontrados e corrigidos:**
- Nonce gerado com `RandomNumberGenerator.GetBytes` (correto) → confirmado.
- Modo GCM autenticado detecta adulteração → confirmado.
- Chave lida de `IConfiguration["Aes:MasterKey"]` → validação de 32 bytes adicionada.

**Rejeitado:**
- Sugestão de envelope encryption (chave de dados protegida por outra chave) → válido em produção real, mas sobre-engenharia para portfólio educacional. Documentado em Melhorias Futuras.

---

## Exemplo 3 — Threat Modeling do fluxo de assinatura

**Prompt enviado:**
> "Threat model do fluxo: usuário assina documento, PFX carregado de config, hash assinado com RSA-PSS. Principais riscos?"

**Aceito:**
- **Exposure de chave privada:** PFX carregado com `EphemeralKeySet` (não persiste no keystore do OS). Senha de PFX nunca logada.
- **Re-assinatura não autorizada:** handler verifica `document.OwnerId == request.SignerId`. Invariante de "já assinado" impede duplicata.
- **Certificado inválido:** `IsCertificateValid` valida expiração e key usage antes de assinar.
- **Hash manipulado antes de assinar:** hash vem do banco (calculado e armazenado no upload). Não re-calculado do arquivo no momento da assinatura.
