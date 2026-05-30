# ADR-0004 — AES-256-GCM para criptografia em repouso

**Status:** Aceito  
**Data:** 2026-05-30

## Decisão

**AES-256-GCM** (modo autenticado) para criptografia de documentos armazenados.

## Justificativa

- **GCM (Galois/Counter Mode):** modo autenticado — detecta adulteração do ciphertext sem necessidade de MAC separado. Um byte alterado no storage → decrypt falha com `AuthenticationTagMismatchException`.
- **AES-256:** chave de 256 bits, aprovado por NIST, disponível em `System.Security.Cryptography`.
- **Nonce/IV único por operação:** `RandomNumberGenerator.GetBytes(12)` garante que o mesmo plaintext produz ciphertexts diferentes. Nunca reutilizar nonce com a mesma chave.
- **Envelope nonce∥tag∥ciphertext:** formato auto-contido armazenado como bytes; sem metadados separados.

## Cuidados implementados

- Chave de 32 bytes validada na inicialização do serviço.
- Chave vem de `Aes:MasterKey` em configuração (base64, 256 bits).
- Em produção: usar envelope encryption (chave de dados protegida por chave mestra em KMS/Key Vault).

## Alternativas consideradas

- **AES-CBC + HMAC:** requer MAC separado, suscetível a padding oracle se mal implementado. Descartado.
- **ChaCha20-Poly1305:** igualmente seguro, mas AES-GCM é mais familiar e tem suporte nativo melhor no .NET. Descartado.
