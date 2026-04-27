# Implementation Plan: Phase 0 Repository Foundation

**Branch**: `001-repo-foundation` | **Date**: 2026-04-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-repo-foundation/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See
`.specify/templates/plan-template.md` for the execution workflow.

## Summary

Create the Phase 0 repository foundation for `distributed-url-shortener`: validate
the canonical monorepo structure, add missing foundation files, create
`DistributedUrlShortener.sln`, add separate .NET 8 project shells for the
Shortener and Statistics runtimes, add a non-runtime Statistics.Infrastructure
class library, document the intended architecture and
trade-offs, and provide a safe local configuration baseline. The implementation
must remain deliberately non-business: no URL creation, redirect resolution,
event processing, analytics, authentication, dashboard, or Angular behavior.

The technical approach is to create minimal, independently buildable runtime
shells with explicit folder and project names, basic health endpoints for API
shells, environment-based configuration, and documentation that makes later
phases easy to extend without changing runtime boundaries.

## Technical Context

**Language/Version**: .NET 8 / C# 12 for Phase 0 runtime shells and
Statistics.Infrastructure; no NestJS, Angular, Ocelot, or Python implementation
in this phase  
**Primary Dependencies**: ASP.NET Core shared framework for API shells,
Microsoft.Extensions.Hosting for worker shells, Docker Compose for local
baseline, no external infrastructure packages in Phase 0  
**Storage**: N/A for implementation; Cassandra, Redis, Redpanda, MinIO,
DuckDB, and ClickHouse are documented only as future architecture components  
**Testing**: Build verification with `dotnet build`; basic manual health checks
for applicable API shells; no unit, integration, e2e, or load tests because no
business behavior exists yet  
**Target Platform**: Local development on Docker Compose and direct .NET CLI
execution  
**Project Type**: Monorepo distributed backend foundation with placeholder
bounded contexts for future ApiGateway, User API, and Angular frontend work  
**Performance Goals**: N/A for runtime behavior; Phase 0 success is structural,
build, documentation, and configuration readiness  
**Constraints**: English documentation, explicit bounded contexts, separate
executables/processes, non-runtime Statistics.Infrastructure class library,
Redirect Service independence, `/health` as the only Redirect Service endpoint
in Phase 0, no later-phase business behavior, no committed secrets, easy Phase 1
extension without replacing the Compose baseline  
**Scale/Scope**: Foundation only; the architecture remains aligned with future
conceptual targets of 100M URL creations/day and 1B redirects/day, but Phase 0
does not implement scalability mechanisms

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Service Boundaries**: PASS. Planned paths keep ownership explicit under
  `src/ApiGateway`, `src/User`, `src/Shortener`, `src/Statistics`, and
  `src/Front`; no shared domain logic is introduced.
- **Frontend Boundary**: PASS. `src/Front/` remains a placeholder only. No
  Angular implementation or direct backend/data-store access is planned.
- **Runtime Model**: PASS. Shortener API, Redirect Service, Statistics.Api,
  Statistics.EventWriter.Worker, and Statistics.BatchProcessor.Worker are
  planned as separate executable projects. Statistics.Infrastructure is planned
  as a supporting class library only, not a runtime or executable.
- **Shortener Context**: PASS. Shortener API and Redirect Service are separate
  projects under `src/Shortener/`; no quota, URL management, or redirect
  behavior is implemented.
- **Redirect Fast Path**: PASS. The plan preserves the future public
  `GET /{shortCode}` ownership by Redirect Service outside ApiGateway while
  explicitly forbidding `GET /{shortCode}`, redirect logic, cache lookup, and
  event publishing in Phase 0. Only `/health` is allowed.
- **Cassandra Modeling**: PASS. No Cassandra schema or storage model is
  implemented in Phase 0.
- **Analytics Pipeline**: PASS. Statistics runtime shells and high-level docs
  are planned, but no broker events, Parquet writes, DuckDB processing,
  ClickHouse writes, scheduling, or dashboard queries are implemented.
- **Observability and Operations**: PASS. Basic health endpoints for API shells,
  environment variables, `.env.example`, Docker Compose baseline, and
  documentation are first-class deliverables. The Compose file must not include
  Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, or observability
  services in Phase 0. Full logs, metrics, tracing, and observability stack are
  deferred.
- **Testing and Load**: PASS. Build verification is required. Business-rule
  unit tests, integration tests, e2e tests, Angular tests, and load tests are
  deferred because no business behavior changes.
- **Documentation**: PASS. `docs/architecture.md`, `docs/tradeoffs.md`,
  README/local flow touch-ups if needed, and `CHANGELOG.md` updates are planned
  in English.

## Project Structure

### Documentation (this feature)

```text
specs/001-repo-foundation/
  plan.md
  research.md
  data-model.md
  quickstart.md
  contracts/
    phase0-runtime-contracts.md
  checklists/
    requirements.md
```

### Source Code (repository root)

```text
DistributedUrlShortener.sln
.env.example
docker-compose.yml
CHANGELOG.md
src/
  ApiGateway/
  User/
  Shortener/
    DistributedUrlShortener.Shortener.Api/
      DistributedUrlShortener.Shortener.Api.csproj
      Program.cs
    DistributedUrlShortener.Redirect.Service/
      DistributedUrlShortener.Redirect.Service.csproj
      Program.cs
  Statistics/
    DistributedUrlShortener.Statistics.Api/
      DistributedUrlShortener.Statistics.Api.csproj
      Program.cs
    DistributedUrlShortener.Statistics.EventWriter.Worker/
      DistributedUrlShortener.Statistics.EventWriter.Worker.csproj
      Program.cs
    DistributedUrlShortener.Statistics.BatchProcessor.Worker/
      DistributedUrlShortener.Statistics.BatchProcessor.Worker.csproj
      Program.cs
    DistributedUrlShortener.Statistics.Infrastructure/  # class library only
      DistributedUrlShortener.Statistics.Infrastructure.csproj
  Front/
infra/
docs/
  architecture.md
  tradeoffs.md
scripts/
tests/
  e2e/
  integration/
  load/
```

**Structure Decision**: The plan follows the constitution's bounded-context
layout. Shortener API and Redirect Service both live under `src/Shortener/` but
are separate projects to preserve independent deployment and scaling. Statistics
API, event writer, and batch processor live under `src/Statistics/` as separate
applications because HTTP query serving, continuous event consumption, and batch
processing have different lifecycles. Statistics.Infrastructure is a class
library only and must not contain business logic in Phase 0. `src/ApiGateway/`,
`src/User/`, and `src/Front/` remain placeholders because their implementation
belongs to later phases.

## Complexity Tracking

No constitution violations are introduced by this plan.

## Phase 0 Research Output

See [research.md](./research.md).

## Phase 1 Design Output

See [data-model.md](./data-model.md), [quickstart.md](./quickstart.md), and
[phase0-runtime-contracts.md](./contracts/phase0-runtime-contracts.md).

## Changelog Impact

`CHANGELOG.md` should be updated under the current `Unreleased` section to
record the Phase 0 repository foundation planning artifacts and, during
implementation, the resulting foundation files and project shells.

## Post-Design Constitution Check

- **Service Boundaries**: PASS. Design artifacts preserve canonical bounded
  contexts and do not introduce shared domain behavior.
- **Frontend Boundary**: PASS. Frontend remains an empty placeholder in this
  plan.
- **Runtime Model**: PASS. Runtime contracts require separate executable
  project shells, a non-runtime Statistics.Infrastructure class library, and
  solution membership.
- **Shortener Context**: PASS. Shortener API and Redirect Service remain
  separate and business-free.
- **Redirect Fast Path**: PASS. The Redirect Service health shell exposes only
  `/health` and does not implement `GET /{shortCode}`, redirect logic, cache
  lookup, event publishing, or ApiGateway/authentication dependencies.
- **Cassandra Modeling**: PASS. Storage is out of scope.
- **Analytics Pipeline**: PASS. Statistics shells do not consume, process, or
  query analytics data.
- **Observability and Operations**: PASS. Health, environment configuration,
  Compose baseline, and build verification are included.
- **Testing and Load**: PASS. Build and health verification are the only Phase 0
  checks; later test categories are deferred explicitly.
- **Documentation**: PASS. Required documentation and changelog impact are
  covered in English.
