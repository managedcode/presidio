# Presidio .NET Migration Plan

This document tracks parity work between `external/microsoft-presidio` (Python) and the C# port.

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
| `RecognizerRegistry` & provider | `recognizer_registry.py` | ✅ | YAML-backed loader + reflective instantiation aligned with Python |
| NLP engines (spaCy, transformers, etc.) | `nlp_engine/` | 🚧 | Only ONNX pipeline ported (`OnnxNlpEngine`) |

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
| Australia | `AuAbnRecognizer`, `AuAcnRecognizer`, `AuMedicareRecognizer`, `AuTfnRecognizer` | ✅ |
| Finland | `FiPersonalIdentityCodeRecognizer` | ✅ |
| India | `InAadhaarRecognizer`, `InGstinRecognizer`, `InPanRecognizer`, `InPassportRecognizer`, `InVehicleRegistrationRecognizer`, `InVoterRecognizer` | ✅ |
| Italy | `ItDriverLicenseRecognizer`, `ItFiscalCodeRecognizer`, `ItIdentityCardRecognizer`, `ItPassportRecognizer`, `ItVatCodeRecognizer` | ✅ |
| Korea | `KrRrnRecognizer` | ✅ |
| Poland | `PlPeselRecognizer` | ✅ |
| Singapore | `SgFinRecognizer`, `SgUenRecognizer` | ✅ |
| Spain | `EsNieRecognizer`, `EsNifRecognizer` | ✅ |
| Thailand | `ThTninRecognizer` | ✅ |
| UK | `NhsRecognizer`, `UkNinoRecognizer` | ✅ |
| US | `MedicalLicenseRecognizer`, `UsBankRecognizer`, `UsLicenseRecognizer`, `UsItinRecognizer`, `UsPassportRecognizer`, `UsSsnRecognizer` | ✅ |

## NLP Engine Recognizers

| Recognizer | Status | Notes |
| --- | --- | --- |
| ONNX NER (`OnnxNerRecognizer`) | ✅ | Ported and default for English |
| spaCy / Stanza recognizers | 🚧 | Need integration once NLP bindings exist |
| Transformers recognizer | 🚧 | Evaluate after deciding on ML stack |
| GLiNER recognizer | 🚧 | Depends on GLiNER .NET availability |

## Third-Party / Remote Recognizers

| Recognizer | Status | Notes |
| --- | --- | --- |
| Azure Health De-ID (`AzureHealthDeidRecognizer`) | ❓ | Requires remote API integration |
| Azure AI Language (`AzureAILanguageRecognizer`) | ❓ | Pending decision |

## Next Actions

- Continue porting any remaining country-specific recognizers not yet covered (e.g., Australia-specific business identifiers beyond the current scope, additional EU IDs, etc.).
- Prioritize recognizer backlog based on customer demand and add coverage tests alongside each port.
- Implement .NET equivalents for spaCy/Stanza/Transformers NLP engines or design alternative pipelines that meet parity guarantees.
