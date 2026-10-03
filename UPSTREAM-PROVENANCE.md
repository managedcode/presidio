# Upstream source provenance

ManagedCode Presidio is a C#/.NET fork and port derived from Microsoft's Presidio project:

- Repository: <https://github.com/microsoft/presidio>
- Upstream revision used as the port baseline: `fa7897847193d99e77bb51f95ae9619131c97eea`
- Upstream `git describe` at that revision: `2.2.360-32-gfa789784`
- Upstream license: MIT, retained in this repository's `LICENSE`; applicable third-party notices are retained in `NOTICE`.

The source revision was previously recorded as a Git submodule. The submodule and its Python source have been removed from the active tree so this repository contains only the .NET implementation, tests, and tooling. The submodule gitlink and its history remain in Git history. Source paths in `PLAN-MIGRATION.md` refer to paths in the upstream repository at the recorded revision; they are provenance references, not files in this checkout.

Do not add upstream source, Python examples, Python tests, or Python-only tooling to the active .NET repository. When comparing a ported behavior with upstream, use the recorded public source revision and preserve attribution and license obligations.
