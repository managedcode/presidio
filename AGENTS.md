# Presidio .NET Migration Notes

## Overview

This document guides the C#/.NET port of Microsoft's Presidio project. ManagedCode maintains the port; upstream repository identity, baseline revision, and license are recorded in `UPSTREAM-PROVENANCE.md`. The active tree contains the .NET implementation, tests, and tooling only.

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
- for Presidio port tasks, compare behavior with the upstream source revision recorded in `UPSTREAM-PROVENANCE.md`; do not add upstream-language source, examples, tests, or tools to the active tree
- for Presidio migration tasks, NEVER rely on stubs, mocks, or placeholder implementations; deliver the full real functionality
- for Presidio migration NLP components, use `Microsoft.ML.Tokenizers` when implementing tokenization
- for Presidio migration tasks, eliminate dependencies on `ManagedCode.Presidio.PythonBridge`; port required logic to C#
- when selecting dependencies, prefer official NuGet packages under permissive licenses (MIT) that match upstream functionality
- integration tests must use explicit native test data for upstream-derived scenarios and verify the real C# pipeline; tests must not read source files from an upstream language tree
- for Presidio analyzer tests, NEVER add stubbed recognizer tests; port the Python scenarios to exercise the real analyzer pipeline end-to-end
- for Presidio analyzer parity work, keep iterating without pausing for confirmation and focus solely on integration tests that validate real functionality
- for Presidio migration tasks, do not stop to ask the user for clarification mid-task; follow the migration plan and deliver completed work
- for Presidio migration tasks, when the user says "продовжити"/"continue", proceed through the target file step by step without asking for additional confirmation
- for Presidio migration tasks, when you see a way to improve something, note the idea in the working file and then implement it without waiting for user approval
- the default `AnalyzerEngine` must not download or require model artifacts; ONNX NLP is opt-in and uses explicitly configured local model, vocabulary, and configuration files
- when configuring a PII policy or requesting entities, validate every selected entity against recognizers available for the exact language and fail closed on unsupported selections
- evaluation evidence sanitization is an explicit call over selected content; never wrap model clients or silently rewrite tool, authorization, or chat semantics
- for Presidio migration tasks, default to continuing the migration workflow without waiting for "продовжити"/"continue"; halt only if the user explicitly redirects
- for Presidio migration tasks, when the user specifies an execution order for follow-up work, honor that sequence without reconfirming and keep progressing task-by-task
- for Presidio migration tasks, capture any important follow-up items directly in the working file as TODOs so they are not lost
- for Presidio test work, ALWAYS include negative/error scenarios alongside positive cases to validate failure paths
- for Presidio recognizer coverage, ensure EU social security numbers are handled alongside US SSN patterns
- use enums and constants over magic strings and numbers
- for .NET work, always run `dotnet format` before `dotnet test` and confirm the suite passes
- avoid template placeholders (e.g., `Class1.cs`, `UnitTest1.cs`); name files and types according to their real domain purpose
- keep documentation, code comments, and commit messaging in English


## Solution Layout

- `src/ManagedCode.Presidio.Core` – foundational domain objects such as `TextSpan`, `AnalysisExplanation`, `RecognizerResult`, and shared metadata keys.
- `src/ManagedCode.Presidio.Analyzer` – analyzer abstractions (`EntityRecognizer`, `NlpArtifacts`, `Token`) with lazy-loading recognizer lifecycle management and ONNX-backed NER.
- `src/ManagedCode.Presidio.Anonymizer` – anonymization primitives (`PiiEntity`, `OperatorConfig`, `OperatorResult`, `EngineResult`) mirroring the Python engine contracts.
- `src/ManagedCode.Presidio.ImageRedactor` – image request/result and bounding-box models; OCR and redaction engines remain unported.
- `src/ManagedCode.Presidio.Structured` – nested JSON string-field analysis, stream/path reading, and anonymization; uses RFC 6901 JSON Pointer paths and requires object or array roots.
- `tests/*` – unit and integration suites covering the C# modules (including parity tests against Python behaviours).

## Current Status

The repository has functioning analyzer and anonymizer pipelines, nested JSON analysis and transformation, and a Microsoft.Extensions.AI.Evaluation integration. The default analyzer uses deterministic recognizers in process and does not fetch a model. Optional ONNX NER requires local configured assets. `PLAN-MIGRATION.md` tracks parity gaps; no package should be described as complete while upstream capabilities remain absent.

## Next Steps

Continue porting upstream analyzer, anonymizer, structured, and image scenarios with positive and negative evidence; keep exact known gaps in `PLAN-MIGRATION.md`. Do not make hosted recognizers, remote NLP providers, Python, or sidecars mandatory runtime dependencies. Run `dotnet format` before `dotnet test`; local runtime roll-forward is not proof of the native net9 CI gate. Release only after package artifacts, CI, GitHub release, and NuGet availability are verified.

This file should be updated as major milestones are completed.
