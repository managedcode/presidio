# Presidio .NET Migration Plan

This document tracks behavior ported from Microsoft's Presidio project into the C# implementation. Source paths in the tables are relative to the upstream repository and revision documented in [UPSTREAM-PROVENANCE.md](UPSTREAM-PROVENANCE.md); upstream source is not included in the active tree.

## Status Legend

- ✅ Complete in C#
- 🚧 Planned / not yet ported
- ❓ Needs investigation / decide if we port

## Core Infrastructure

| Component | Python Source | Status | Notes |
| --- | --- | --- | --- |
| `EntityRecognizer` base | `presidio_analyzer/entity_recognizer.py` | ✅ | Ported as `EntityRecognizer` (C#) |
| `Pattern` helper | `predefined_recognizers/generic/pattern_recognizer.py` | ✅ | Implemented as `Pattern` (C#) |
| `PatternRecognizer` | same | ✅ | Implemented with regex caching & validation hooks |
| `LocalRecognizer` / remote base classes | `local_recognizer.py`, `remote_recognizer.py` | ✅ | Ported as `LocalRecognizer`/`RemoteRecognizer` with unit coverage |
| `RecognizerRegistry` & provider | `recognizer_registry.py` | 🚧 | YAML-backed loader is present and now fails closed for unsupported enabled definitions; batch/request API parity remains incomplete. |
| NLP engines (spaCy, Stanza, transformers, etc.) | `nlp_engine/` | 🚧 | Optional local ONNX NER exists. The default analyzer has no model dependency and uses deterministic recognizers only. |

## Generic Recognizers

| Recognizer | Python Source | Status | Notes | Tests |
| --- | --- | --- | --- | --- |
| Credit Cards | `predefined_recognizers/generic/credit_card_recognizer.py` | ✅ | Luhn checksum implemented (`CreditCardRecognizer`) | `tests/ManagedCode.Presidio.Analyzer.Tests/CreditCardPatternRecognizerTests.cs` |
| IBAN | `predefined_recognizers/generic/iban_recognizer.py` + `iban_patterns.py` | ✅ | Country regex map + checksum (`IbanRecognizer`) | `tests/ManagedCode.Presidio.Analyzer.Tests/IbanRecognizerTests.cs` |
| ABA Routing | `predefined_recognizers/country_specific/us/aba_routing_recognizer.py` | ✅ | Checksum + formatting (`AbaRoutingRecognizer`) | `tests/ManagedCode.Presidio.Analyzer.Tests/AbaRoutingRecognizerTests.cs` |
| Crypto wallet | `predefined_recognizers/generic/crypto_recognizer.py` | ✅ | Base58 + Bech32 validation (`CryptoRecognizer`) | `tests/ManagedCode.Presidio.Analyzer.Tests/CryptoRecognizerTests.cs` |
| Date | `predefined_recognizers/generic/date_recognizer.py` | ✅ | Regex suite covering ISO, slash/dash, and month formats (`DateRecognizer`) | `tests/ManagedCode.Presidio.Analyzer.Tests/DateRecognizerTests.cs` |
| Email | `predefined_recognizers/generic/email_recognizer.py` | ✅ | Regex + domain validation (`EmailRecognizer`) | `tests/ManagedCode.Presidio.Analyzer.Tests/EmailRecognizerTests.cs` |
| IP address | `predefined_recognizers/generic/ip_recognizer.py` | ✅ | Regex parity with `IpRecognizer` + `IPAddress` validation | `tests/ManagedCode.Presidio.Analyzer.Tests/IpRecognizerTests.cs` |
| Phone | `predefined_recognizers/generic/phone_recognizer.py` | ✅ | Uses `PhoneRecognizer` backed by libphonenumber (`PhoneNumbers`) | `tests/ManagedCode.Presidio.Analyzer.Tests/PhoneRecognizerTests.cs` |
| URL | `predefined_recognizers/generic/url_recognizer.py` | ✅ | CommonRegex port (`UrlRecognizer`) with schema/non-schema support | `tests/ManagedCode.Presidio.Analyzer.Tests/UrlRecognizerTests.cs` |

## Country-Specific Recognizers

| Country | Python Class | Status |
| --- | --- | --- |
| Australia | `AuAbnRecognizer`, `AuAcnRecognizer`, `AuMedicareRecognizer`, `AuTfnRecognizer` | ✅ | ACN checksum has positive and negative upstream cases. |
| Finland | `FiPersonalIdentityCodeRecognizer` | ✅ |
| India | `InAadhaarRecognizer`, `InGstinRecognizer`, `InPanRecognizer`, `InPassportRecognizer`, `InVehicleRegistrationRecognizer`, `InVoterRecognizer` | ✅ |
| Italy | `ItDriverLicenseRecognizer`, `ItFiscalCodeRecognizer`, `ItIdentityCardRecognizer`, `ItPassportRecognizer`, `ItVatCodeRecognizer` | ✅ |
| Korea | `KrRrnRecognizer` | ✅ |
| Poland | `PlPeselRecognizer` | ✅ |
| Singapore | `SgFinRecognizer`, `SgUenRecognizer` | ✅ |
| Spain | `EsNieRecognizer`, `EsNifRecognizer` | ✅ |
| Thailand | `ThTninRecognizer` | ✅ |
| UK | `NhsRecognizer`, `UkNinoRecognizer` | ✅ |
| US | `MedicalLicenseRecognizer`, `UsBankRecognizer`, `UsLicenseRecognizer`, `UsItinRecognizer`, `UsPassportRecognizer`, `UsSsnRecognizer` | 🚧 | Driver-license patterns exist; more upstream scenario and context parity is needed. |

## NLP Engine Recognizers

| Recognizer | Status | Notes |
| --- | --- | --- |
| ONNX NER (`OnnxNerRecognizer`) | 🚧 | Optional and requires explicitly available local model, vocabulary, and configuration files. |
| spaCy / Stanza recognizers | 🚧 | Need integration once NLP bindings exist |
| Transformers recognizer | 🚧 | Evaluate after deciding on ML stack |
| GLiNER recognizer | 🚧 | Depends on GLiNER .NET availability |

## Third-Party / Remote Recognizers

| Recognizer | Status | Notes |
| --- | --- | --- |
| Azure Health De-ID (`AzureHealthDeidRecognizer`) | Excluded | Cloud-specific backend is outside this in-process port. |
| Azure AI Language (`AzureAILanguageRecognizer`) | Excluded | Cloud-specific backend is outside this in-process port. |

## Anonymizer Status

The operator pipeline includes keep, redact, mask, replace, hash, encrypt, and decrypt. AHDS surrogate/deanonymize-keep behavior and some operator/parameter parity remain unported; this is not full anonymizer parity.

## Structured Status

`ManagedCode.Presidio.Structured` analyzes nested JSON string values into entity mappings, reads JSON, and applies the existing anonymizer pipeline to a cloned JSON tree. Entity mapping keys use RFC 6901 JSON Pointers (for example, `/contacts/0/email`) so property names containing dots or slashes remain unambiguous. The API accepts object and array roots and rejects primitive roots. Python DataFrame/Pandas analysis, sampling strategies, CSV reader parity, and broader upstream structured APIs remain incomplete.

## Image Redactor Status

Only image request/result and bounding-box models are ported. OCR engines, image processing, redaction, DICOM, and verification paths from `presidio-image-redactor` remain unported. The package is not a complete image-redaction implementation.

## Next Actions

- Continue adding supported local recognizers and port their upstream positive and negative scenarios.
- Prioritize recognizer backlog based on customer demand and add coverage tests alongside each port.
- Complete JSON collection semantics and native CSV/tabular support without requiring Python or remote services.
- Investigate local OCR/image dependencies before describing image redaction as supported.
