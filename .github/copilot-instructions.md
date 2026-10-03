# ManagedCode Presidio .NET repository instructions

This repository is the C#/.NET port of Microsoft's Presidio. The active source, tests, examples, and automation are .NET; do not add upstream-language source, test frameworks, package managers, or workflows. Upstream identity, baseline commit, and license are recorded in `UPSTREAM-PROVENANCE.md`.

## Build and test

Use the .NET 9 SDK selected by `global.json`:

```bash
dotnet restore Presidio.slnx
dotnet build Presidio.slnx --configuration Release
dotnet format Presidio.slnx --verify-no-changes
dotnet test Presidio.slnx --configuration Release
```

Run format before the full test suite. Add meaningful positive and negative tests for behavior changes. Keep tests on the actual in-process C# implementation; never test by reading upstream-language source files.

## Runtime boundaries

- Default analysis uses deterministic in-process recognizers and must not download model artifacts.
- Optional ONNX NLP requires explicitly configured local artifacts. Missing optional artifacts may skip ONNX integration cases; explicitly configured invalid artifacts must fail.
- AI integrations use Microsoft.Extensions.AI / Microsoft Agent Framework abstractions only.
- Structured JSON support and image-package limitations are described in `README.MD` and `PLAN-MIGRATION.md`; do not claim unsupported OCR, image redaction, DICOM, or full upstream parity.
- Preserve MIT licensing and attribution in `LICENSE`, `NOTICE`, and `UPSTREAM-PROVENANCE.md`.
