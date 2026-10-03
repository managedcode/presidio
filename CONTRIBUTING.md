# Contributing to ManagedCode Presidio

ManagedCode Presidio is a C#/.NET port maintained by ManagedCode from Microsoft's upstream Presidio project. The active implementation, examples, and test suite in this repository are .NET. The exact upstream baseline is recorded in [UPSTREAM-PROVENANCE.md](UPSTREAM-PROVENANCE.md); upstream source code is not included in the active tree or needed to build, test, or use the .NET packages.

## Before opening a change

- Read [AGENTS.md](AGENTS.md) and the affected project files.
- Keep public behavior aligned with the upstream Presidio contract where it has been ported, and record remaining gaps in [PLAN-MIGRATION.md](PLAN-MIGRATION.md).
- Add meaningful positive and negative C# tests for behavior changes. Tests should exercise the real in-process pipeline; do not substitute production behavior with mocks or stubs.
- Preserve deterministic operation without model downloads. Optional ONNX tests may run only with explicitly configured local artifacts; a supplied but invalid artifact path must fail the test.
- Keep package APIs and documentation honest about unsupported behavior. In particular, image request/result models do not implement OCR, image redaction, or DICOM processing.

## Build and verify

The repository targets .NET 9 and pins SDK 9.0.301 in `global.json`. From the repository root, run:

```bash
dotnet restore Presidio.slnx
dotnet build Presidio.slnx --configuration Release
dotnet format Presidio.slnx --verify-no-changes
dotnet test Presidio.slnx --configuration Release
```

Format the solution before running the full test suite. Optional local ONNX assets are configured with `PRESIDIO_ONNX_MODEL_PATH`, `PRESIDIO_ONNX_VOCAB_PATH`, and `PRESIDIO_ONNX_CONFIG_PATH`; tests skip only when none of these are configured and fail when configured assets are invalid.

## Pull requests and releases

Use focused commits with clear messages. Update `PLAN-MIGRATION.md` or the relevant package documentation when a change materially changes supported behavior. The GitHub Actions workflows build, test, analyze, pack, and publish the .NET packages. Do not publish from a local substitute package or report a release complete until the workflow succeeds and the resulting package versions are available from NuGet.

## Attribution and licensing

Keep `UPSTREAM-PROVENANCE.md`, `NOTICE`, and the repository's license intact when changing attribution or ported behavior. ManagedCode contributions are licensed under this repository's MIT license. Follow the contribution requirements stated by the repository hosting the change; Microsoft CLA instructions from the upstream repository do not govern this fork automatically.
