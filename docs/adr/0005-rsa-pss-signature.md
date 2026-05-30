# ADR-0005 — Assinatura digital com RSA-PSS sobre hash do documento

**Status:** Aceito  
**Data:** 2026-05-30  
**Autores:** João Gabriel

---

## Contexto

O sistema precisa assinar documentos digitalmente de forma que a assinatura seja verificável e prove autoria + integridade. A assinatura deve ser feita com material de chave real (certificado PFX) simulando uso de certificado A1 (ICP-Brasil).

## Decisão

**Assinar o hash SHA-256 do documento** (não o conteúdo completo) com **RSA-PSS** usando SHA-256 como hash interno.

### Por que assinar o hash, não o conteúdo?

- Documentos podem ser grandes (10 MB); assinar diretamente é ineficiente.
- Hash é um digest criptograficamente seguro que representa o conteúdo completo.
- Padrão da indústria (PAdES, CAdES, XAdES todos assinam o hash).
- O hash já foi computado e armazenado no upload — reutilização direta.

### Por que RSA-PSS, não PKCS#1 v1.5?

- PSS (Probabilistic Signature Scheme) é provadamente mais seguro.
- PKCS#1 v1.5 é determinístico e vulnerável a ataques de fault injection.
- RFC 8017 recomenda PSS para novas implementações.
- `System.Security.Cryptography` suporta nativamente via `RSASignaturePadding.Pss`.

### Certificado PFX (PKCS#12)

- Material de chave carregado de arquivo PFX protegido por senha.
- Caminho e senha vêm de configuração/secret, nunca do código.
- Validação de certificado: validade temporal + key usage (DigitalSignature).
- PFX de teste é dummy gerado para o ambiente de testes (não representa CA real).

## Consequências

**Positivas:**
- Demonstra Nível 3 (RSA), Nível 4 (X509), Nível 5 (PFX) do roteiro de criptografia.
- Fluxo "assinar → adulterar → validar falha" verificável em teste automatizado.
- `ISignatureService` abstrai o algoritmo — troca para ECDSA seria só nova implementação.

**Negativas / trade-offs:**
- Simulação educacional: sem TSA (carimbo de tempo), sem revogação real (CRL/OCSP).
- PFX protegido por senha em arquivo local — em produção real usar HSM ou Key Vault.
