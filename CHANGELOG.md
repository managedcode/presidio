# Changelog

This changelog covers the ManagedCode C#/.NET port. Historical Microsoft Presidio upstream changes are not releases of these NuGet packages; upstream source provenance is recorded in [UPSTREAM-PROVENANCE.md](UPSTREAM-PROVENANCE.md).

## [Unreleased]

### Changed

- Correct the root README embedded in all six NuGet packages so it describes the supported .NET behavior and limitations.
- Remove the upstream Python source checkout and Python-only development instructions from the active repository while retaining the exact upstream commit, attribution, and license notices.
- Make repository dependency automation and CodeQL analyze the active .NET projects.
