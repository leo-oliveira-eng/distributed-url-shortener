# Changelog

## Unreleased - 2026-04-26

### Added

- Added a portfolio-grade `README.md` describing the distributed URL shortener architecture, key features, scalability model, service responsibilities, analytics pipeline, observability strategy, trade-offs, quick start flow, and project status.
- Added Spec Kit configuration, templates, workflows, integration manifests, and PowerShell helper scripts under `.specify/`.
- Added the Git extension package for Spec Kit, including command definitions, configuration templates, and Bash/PowerShell automation scripts.
- Added the initial project constitution defining architecture principles, runtime boundaries, redirect fast path rules, analytics pipeline constraints, observability expectations, and repository governance.
- Added initial monorepo service placeholders for the Angular frontend, Shortener bounded context, and Statistics services.
- Added test folder scaffolding for end-to-end, integration, and load testing.
- Added the Phase 0 Repository Foundation specification and planning artifacts under `specs/001-repo-foundation/`.

### Changed

- Updated `.gitignore` with local IDE/workflow ignore rules.

### Repository Structure

- `src/Front/`
- `src/Shortener/`
- `src/Statistics/DistributedUrlShortener.Statistics.Api/`
- `src/Statistics/DistributedUrlShortener.Statistics.EventWriter.Worker/`
- `src/Statistics/DistributedUrlShortener.Statistics.BatchProcessor.Worker/`
- `src/Statistics/DistributedUrlShortener.Statistics.Infrastructure/`
- `.specify/`
- `.windsurf/`
- `docs/`
- `infra/`
- `scripts/`
- `tests/`
