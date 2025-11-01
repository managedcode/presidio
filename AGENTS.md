# Presidio .NET Migration Notes

## Overview

This document tracks the migration of the legacy Python Presidio implementation to a .NET 9 codebase. ManagedCode maintains this .NET port, with the canonical Python sources vendored via the `external/microsoft-presidio` submodule serving as the authoritative reference while we build the C# implementation.

# Conversations

any resulting updates to agents.md should go under the section "## Rules to follow"
When you see a convincing argument from me on how to solve or do something. add a summary for this in agents.md. so you learn what I want over time.
If I say any of the following point, you do this: add the context to agents.md, and associate this with a specific type of task.
if I say "never do x" in some way.
if I say "always do x" in some way.
if I say "the process is x" in some way.
If I tell you to remember something, you do the same, update
if I say "do/don’t", define a process, or confirms success/failure, add a concise rule tied to the relevant task type.
if I say "always/never X", "prefer X over Y", "I like/dislike X", or "remember this", update this file.
When a mistake is corrected, capture the new rule and remove obsolete guidance.
When a workflow is defined or refined, document it here.
Strong negative language indicates a critical mistake; add an emphatic rule immediately.

Update guidelines:
- Actionable rules tied to task types.
- Capture why, not just what.
- One clear instruction per bullet.
- Group related rules.
- Remove obsolete rules entirely.

---

## Rules To Follow
- for Presidio migration tasks, ALWAYS mirror functionality, tests, and docs from `external/microsoft-presidio` to guarantee parity
- for Presidio migration tasks, NEVER rely on stubs, mocks, or placeholder implementations; deliver the full real functionality
- for Presidio migration NLP components, use `Microsoft.ML.Tokenizers` when implementing tokenization
- for Presidio migration tasks, eliminate dependencies on `ManagedCode.Presidio.PythonBridge`; port required logic to C#
- when selecting dependencies, prefer official NuGet packages under permissive licenses (MIT) that match upstream functionality
- integration tests must cover real data from the original Python project to verify parity; ensure all tests pass without hacks
- for Presidio analyzer tests, NEVER add stubbed recognizer tests; port the Python scenarios to exercise the real analyzer pipeline end-to-end
- for Presidio analyzer parity work, keep iterating without pausing for confirmation and focus solely on integration tests that validate real functionality
- for Presidio migration tasks, do not stop to ask the user for clarification mid-task; follow the migration plan and deliver completed work
- use enums and constants over magic strings and numbers
- for .NET work, always run `dotnet format` before `dotnet test` and confirm the suite passes
- avoid template placeholders (e.g., `Class1.cs`, `UnitTest1.cs`); name files and types according to their real domain purpose
- keep documentation, code comments, and commit messaging in English


## Solution Layout

- `src/ManagedCode.Presidio.Core` – foundational domain objects such as `TextSpan`, `AnalysisExplanation`, `RecognizerResult`, and shared metadata keys.
- `src/ManagedCode.Presidio.Analyzer` – analyzer abstractions (`EntityRecognizer`, `NlpArtifacts`, `Token`) with lazy-loading recognizer lifecycle management and ONNX-backed NER.
- `src/ManagedCode.Presidio.Anonymizer` – anonymization primitives (`PiiEntity`, `OperatorConfig`, `OperatorResult`, `EngineResult`) mirroring the Python engine contracts.
- `src/ManagedCode.Presidio.ImageRedactor` / `src/ManagedCode.Presidio.Structured` – placeholders for their respective pipelines.
- `tests/*` – unit and integration suites covering the C# modules (including parity tests against Python behaviours).

## Current Status

The core types compile and are covered by unit tests; the integration test suite validates parity of critical behaviours (e.g., `RecognizerResult` semantics) against scenarios captured in the original Python tests. The analyzer/anonymizer engines are intentionally skeletal and ready to receive the full algorithmic port.

## Next Steps

### Phase 1 — Core Parity (in progress)
- Mirror Python domain types (`RecognizerResult`, `PiiEntity`, `OperatorConfig`, etc.) and prove equivalence via integration tests that ingest canonical scenarios from `tests/` in the Python repo.
- Expand the shared test harness to load JSON/YAML fixtures directly from `external/microsoft-presidio` for deterministic coverage.

### Phase 2 — Analyzer Engine
- Port `AnalyzerEngine`, `RecognizerRegistry`, pattern recognizers, and context enhancers.
- Reproduce Python unit suites (`presidio-analyzer/tests`) as C# integration tests; compare scored outputs to golden data.
- Introduce ONNX-backed NER execution (using models referenced in the Python repo) and abstract inference to enable GPU/CPU parity.

### Phase 3 — Anonymizer Engine
- Implement operator pipeline (mask, redact, hash, replace, FPE, etc.) and conflict resolution consistent with Python behaviour.
- Backfill structured-data traversal and policy parsing; reuse original YAML policies as integration fixtures.

### Phase 4 — Image & Structured Pipelines
- Recreate image redaction and structured anonymization flows, focusing on API compatibility and deterministic output.
- Add scenario tests leveraging sample images/JSON from the Python repository to verify pixel/record level transforms.

### Phase 5 — Packaging & Tooling
- Produce NuGet packages for core/analyzer/anonymizer components.
- Expose minimal public APIs and documentation for consumption, and wire CI to publish artifacts.
- Remove remaining Python scaffolding once feature parity and documentation are complete.

This plan should be revisited at the end of each phase to incorporate learnings and refine subsequent milestones.

This file should be updated as major milestones are completed.
