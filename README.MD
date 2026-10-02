# ManagedCode Presidio for .NET

> **Status:** Early alpha. The API surface area is evolving while we complete the port from the Python reference implementation.

[![CI](https://github.com/managedcode/presidio/actions/workflows/ci.yml/badge.svg)](https://github.com/managedcode/presidio/actions/workflows/ci.yml)
[![Release](https://github.com/managedcode/presidio/actions/workflows/release.yml/badge.svg)](https://github.com/managedcode/presidio/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

ManagedCode Presidio is the .NET 9 rewrite of Microsoft's [Presidio](https://github.com/microsoft/presidio) project.
The original Python codebase remains the authoritative reference implementation and is vendored in this repository as a Git submodule under `external/microsoft-presidio`. Our goal is feature parity with the Python release while delivering first-class NuGet packages for .NET applications.

The analyzer and anonymizer run in process. `AnalyzerEngine` uses configured deterministic recognizers by default and does not download NLP models. Optional ONNX NER requires local model artifacts. This port does not require Python, a sidecar, or a hosted AI service.

---

## Solution Layout

- `Presidio.slnx` – Visual Studio / `dotnet` solution for all libraries and tests.
- `src/ManagedCode.Presidio.Core` – cross-cutting primitives (`TextSpan`, `RecognizerResult`, `AnalysisExplanation`, etc.).
- `src/ManagedCode.Presidio.Analyzer` – contracts for recognizers and NLP artefacts.
- `src/ManagedCode.Presidio.Anonymizer` – operators, engine results, and shared anonymization abstractions.
- `src/ManagedCode.Presidio.ImageRedactor` – image request/result and bounding-box primitives; OCR and redaction engines remain unported.
- `src/ManagedCode.Presidio.Structured` – nested JSON analysis, reading, and anonymization.
- `src/ManagedCode.Presidio.Evaluation` – explicit evidence sanitization and an `IEvaluator` privacy metric.
- `tests/ManagedCode.Presidio.*` – unit and integration suites validating parity against the Python behaviour.
- `external/microsoft-presidio` – Git submodule pointing at the upstream Python repository.

---

## NuGet Packages

The solution produces the following packages (versioned centrally via `Directory.Build.props`):

- `ManagedCode.Presidio.Core`
- `ManagedCode.Presidio.Analyzer`
- `ManagedCode.Presidio.Anonymizer`
- `ManagedCode.Presidio.Structured`
- `ManagedCode.Presidio.ImageRedactor`
- `ManagedCode.Presidio.Evaluation`

Publishing is handled by the `release.yml` GitHub workflow which runs on pushes to `main`.

---

## Getting Started

### Prerequisites

- .NET SDK **9.0.301** (configured via `global.json`).
- Git 2.35+ with submodule support.
- Optionally, Python 3.9+ if you need to run the original reference code or regenerate fixtures.

### Clone the Repository

```bash
git clone https://github.com/managedcode/presidio.git
cd presidio
git submodule update --init --recursive
```

### Build & Test

```bash
dotnet restore Presidio.slnx
dotnet build Presidio.slnx --configuration Release
dotnet format Presidio.slnx
dotnet test Presidio.slnx --configuration Release
```

Run `dotnet format Presidio.slnx` before `dotnet test Presidio.slnx --configuration Release`.

### Usage Examples

The .NET APIs mirror the Python surface. The snippets below show the most common entry points.

#### Analyzing and anonymizing raw text

```csharp
using System.Collections.Generic;
using ManagedCode.Presidio.Analyzer;
using ManagedCode.Presidio.Anonymizer;

using var analyzer = new AnalyzerEngine();
var text = "Contact alice@example.org";
var recognizerResults = analyzer.Analyze(text, "en", ["EMAIL_ADDRESS"]);
var anonymizer = new AnonymizerEngine();
var result = anonymizer.Anonymize(
    text,
    recognizerResults,
    new Dictionary<string, OperatorConfig>
    {
        ["EMAIL_ADDRESS"] = new OperatorConfig("redact"),
    });

// result.Text == "Contact "
```

#### Sanitizing evaluation evidence

The optional Evaluation package accepts explicit entity types, language, and score threshold. It never rewrites chat messages or tool arguments implicitly.

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

`PresidioPrivacyEvaluator` implements `Microsoft.Extensions.AI.Evaluation.IEvaluator`, scans only the assistant response, and emits a numeric count metric without copying detected values into metric text. It does not call a model.

#### Working with collections (BatchAnonymizerEngine)

```csharp
using System.Collections.Generic;
using ManagedCode.Presidio.Anonymizer;

var batch = new BatchAnonymizerEngine(anonymizer);

var response = batch.AnonymizeDict(new[]
{
    new DictRecognizerResult(
        "names",
        new[] { "John", "Jill" },
        new[]
        {
            new[] { new ManagedCode.Presidio.Anonymizer.RecognizerResult("PERSON", 0, 4, 0.9) },
            new[] { new ManagedCode.Presidio.Anonymizer.RecognizerResult("PERSON", 0, 4, 0.9) },
        })
});

// response["names"] == ["<PERSON>", "<PERSON>"]
```

#### Deanonymizing previously encrypted entities

```csharp
using System.Collections.Generic;
using ManagedCode.Presidio.Anonymizer;

var deanonymizer = new DeanonymizeEngine();

var decrypted = deanonymizer.Deanonymize(
    cipherText: "My name is S184CMt9Drj7QaKQ21JTrpYzghnboTF9pn/neN8JME0=",
    entities: new[] { new OperatorResult(11, 55, "PERSON") },
    operators: new Dictionary<string, OperatorConfig>
    {
        ["DEFAULT"] = new OperatorConfig("decrypt", new Dictionary<string, object?>
        {
            [EncryptOperator.KeyParameter] = "WmZq4t7w!z%C&F)J",
        })
    });

// decrypted.Text == "My name is Chloë"
```

---

## Python Reference Submodule

The `external/microsoft-presidio` directory tracks the upstream Python implementation.
Tests under `tests/ManagedCode.Presidio.Core.IntegrationTests` load fixtures derived from the Python suite to prove behavioural parity of key primitives. As the port progresses, more fixtures will be mirrored directly from the submodule so that regressions can be caught early.

If you update the submodule revision, ensure you run:

```bash
git submodule update --remote external/microsoft-presidio
```

and commit the resulting SHA change.

---

## Roadmap

1. **Core parity** – finalise domain primitives and validate them against Python fixtures.
2. **Analyzer engine** – port recognizer registry, pattern recognizers, and ONNX-backed NER execution.
3. **Anonymizer engine** – reproduce the operator pipeline, conflict resolution, and policy parsing.
4. **Image/Structured pipelines** – deliver feature parity for image redaction and structured anonymisation flows.
5. **Packaging** – stabilise public APIs, documentation, and automate NuGet publishing.

The conversion does not yet claim full Python feature parity. Current implementation and known gaps are tracked in `PLAN-MIGRATION.md`.

---

## Contributing

We welcome pull requests and issues. Please read [CONTRIBUTING.md](CONTRIBUTING.md) and follow the existing coding conventions (see `.editorconfig`).
All code must target `.NET 9` and pass `dotnet format` + `dotnet test` before a PR can be merged.

---

## License

This project is licensed under the [MIT License](LICENSE).

---

## Maintainers

ManagedCode SAS is the primary maintainer of this fork.
For questions, please open a GitHub issue or contact the ManagedCode team.
