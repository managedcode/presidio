# ManagedCode Presidio for .NET

> **Status:** Early alpha. Public APIs and upstream parity are still evolving.

[![CI](https://github.com/managedcode/presidio/actions/workflows/ci.yml/badge.svg)](https://github.com/managedcode/presidio/actions/workflows/ci.yml)
[![Release](https://github.com/managedcode/presidio/actions/workflows/release.yml/badge.svg)](https://github.com/managedcode/presidio/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

ManagedCode Presidio is a C#/.NET fork and port of Microsoft's [Presidio](https://github.com/microsoft/presidio) privacy-protection project. The .NET implementation is maintained in this repository and distributed as NuGet packages. The original source revision used as the port's baseline is recorded in [UPSTREAM-PROVENANCE.md](https://github.com/managedcode/presidio/blob/main/UPSTREAM-PROVENANCE.md); upstream source files are not part of the active checkout. The port is not a line-for-line API mirror, and it does not claim complete upstream feature parity.

Core PII analysis and anonymization execute in process. The default analyzer uses deterministic configured recognizers and does not download model artifacts. Optional ONNX NER can be enabled with explicitly configured local model, vocabulary, and configuration files. No Python runtime, sidecar, cloud recognizer, or hosted model service is required. Optional AI-facing evaluation integration uses Microsoft.Extensions.AI.Evaluation abstractions; sanitizer and evaluator behavior is explicit and does not rewrite model conversations or tool calls.

## Current .NET surface

- **Core** provides shared text spans, recognition results, explanations, and metadata.
- **Analyzer** provides the recognizer registry, deterministic pattern and checksum recognizers, and opt-in local ONNX NER.
- **Anonymizer** provides text anonymization operators and batch/dictionary helpers.
- **Structured** analyzes string values in nested JSON, reads JSON, and applies the anonymizer to a cloned JSON tree. Object and array roots are supported; mapping paths use RFC 6901 JSON Pointers.
- **Evaluation** provides explicit evidence sanitization and a Microsoft.Extensions.AI.Evaluation `IEvaluator` privacy metric.
- **ImageRedactor** currently provides image request/result and bounding-box data models only. OCR, image redaction, DICOM processing, and verification are not implemented by this package.

The detailed component status and known gaps are maintained in [PLAN-MIGRATION.md](https://github.com/managedcode/presidio/blob/main/PLAN-MIGRATION.md). In particular, optional ONNX NLP does not imply that every upstream model or NLP backend is available.

## NuGet packages

The solution builds six packages, with version managed centrally in `Directory.Build.props`:

- `ManagedCode.Presidio.Core`
- `ManagedCode.Presidio.Analyzer`
- `ManagedCode.Presidio.Anonymizer`
- `ManagedCode.Presidio.Structured`
- `ManagedCode.Presidio.ImageRedactor`
- `ManagedCode.Presidio.Evaluation`

The [release workflow](https://github.com/managedcode/presidio/blob/main/.github/workflows/release.yml) builds and publishes the package set. Package installation and API compatibility should be checked against the published version available on NuGet; a local build is not a released package.

## Requirements and setup

- .NET SDK **9.0.301** (selected by `global.json`).
- Git 2.35 or newer.

Clone the .NET repository and restore its solution:

```bash
git clone https://github.com/managedcode/presidio.git
cd presidio
dotnet restore Presidio.slnx
```

No submodule initialization, Python installation, model download, or network AI service is needed for the default analyzer and anonymizer.

## Examples

### Analyze and anonymize text

```csharp
using ManagedCode.Presidio.Analyzer;
using ManagedCode.Presidio.Anonymizer;

using var analyzer = new AnalyzerEngine();
const string text = "Contact alice@example.org";
var findings = analyzer.Analyze(text, "en", ["EMAIL_ADDRESS"]);

var anonymizer = new AnonymizerEngine();
var result = anonymizer.Anonymize(
    text,
    findings,
    new Dictionary<string, OperatorConfig>
    {
        ["EMAIL_ADDRESS"] = new("redact"),
    });

// result.Text == "Contact "
```

### Sanitize selected evaluation evidence

Sanitization is an explicit operation over selected text. Callers choose language, entity types, threshold, and replacement behavior; no chat, authorization, or tool semantics are changed implicitly.

```csharp
using ManagedCode.Presidio.Analyzer;
using ManagedCode.Presidio.Anonymizer;
using ManagedCode.Presidio.Evaluation;

using var analyzer = new AnalyzerEngine();
var policy = new PresidioPiiPolicy("en", ["EMAIL_ADDRESS"], scoreThreshold: 0.5);
var sanitizer = new PresidioEvidenceSanitizer(analyzer, new AnonymizerEngine());
var safeEvidence = sanitizer.Sanitize(
    "The customer is alice@example.org",
    policy,
    PresidioEvidenceRedactionMode.ReplaceWithEntity);

// safeEvidence.Text == "The customer is <EMAIL_ADDRESS>"
```

`PresidioPrivacyEvaluator` implements `Microsoft.Extensions.AI.Evaluation.IEvaluator`, scans assistant-response evidence, and reports a numeric finding count without copying detected values into metric text. It does not call a model.

## Build and test

```bash
dotnet restore Presidio.slnx
dotnet build Presidio.slnx --configuration Release
dotnet format Presidio.slnx --verify-no-changes
dotnet test Presidio.slnx --configuration Release
```

Format before running the full test suite. Optional ONNX tests use only local artifacts explicitly provided with `PRESIDIO_ONNX_MODEL_PATH`, `PRESIDIO_ONNX_VOCAB_PATH`, and `PRESIDIO_ONNX_CONFIG_PATH`. The suite skips those tests only when no paths are configured; configured but invalid paths fail.

## Upstream attribution and license

The port is derived from Microsoft's Presidio project, licensed under MIT. Its provenance is recorded in [UPSTREAM-PROVENANCE.md](https://github.com/managedcode/presidio/blob/main/UPSTREAM-PROVENANCE.md), and the original revision remains part of this repository's Git history without placing upstream Python source or tooling in the active tree. This project preserves the MIT license and applicable third-party notices in [LICENSE](https://github.com/managedcode/presidio/blob/main/LICENSE) and [NOTICE](https://github.com/managedcode/presidio/blob/main/NOTICE).

ManagedCode SAS maintains this fork. For contribution guidance, see [CONTRIBUTING.md](https://github.com/managedcode/presidio/blob/main/CONTRIBUTING.md).
