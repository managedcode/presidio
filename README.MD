# ManagedCode Presidio for .NET

> **Status:** Early alpha. The API surface area is evolving while we complete the port from the Python reference implementation.

[![CI](https://github.com/managedcode/presidio/actions/workflows/ci.yml/badge.svg)](https://github.com/managedcode/presidio/actions/workflows/ci.yml)
[![Release](https://github.com/managedcode/presidio/actions/workflows/release.yml/badge.svg)](https://github.com/managedcode/presidio/actions/workflows/release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

ManagedCode Presidio is the .NET 9 rewrite of Microsoft's [Presidio](https://github.com/microsoft/presidio) project.  
The original Python codebase remains the authoritative reference implementation and is vendored in this repository as a Git submodule under `external/microsoft-presidio`. Our goal is feature parity with the Python release while delivering first-class NuGet packages for .NET applications.

---

## Solution Layout

- `Presidio.sln` – Visual Studio / `dotnet` solution for all libraries and tests.
- `src/ManagedCode.Presidio.Core` – cross-cutting primitives (`TextSpan`, `RecognizerResult`, `AnalysisExplanation`, etc.).
- `src/ManagedCode.Presidio.Analyzer` – contracts for recognizers and NLP artefacts.
- `src/ManagedCode.Presidio.Anonymizer` – operators, engine results, and shared anonymization abstractions.
- `src/ManagedCode.Presidio.ImageRedactor` – image-domain types (bounding boxes, request/response models).
- `src/ManagedCode.Presidio.Structured` – helpers for structured/semi-structured payload anonymization.
- `src/ManagedCode.Presidio.PythonBridge` – utility helpers that locate and consume resources from the Python submodule.
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
- `ManagedCode.Presidio.PythonBridge`

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
dotnet restore Presidio.sln
dotnet build Presidio.sln --configuration Release
dotnet format
dotnet test Presidio.sln --configuration Release
```

> We always run `dotnet format` before `dotnet test` to ensure consistent styling across the solution.

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

Progress is tracked in `AGENTS.md`.

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
