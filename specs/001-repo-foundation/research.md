# Research: Phase 0 Repository Foundation

## Decision: Use .NET 8 project shells for Shortener and Python 3.12+ shells for Statistics

**Rationale**: The corrected Phase 0 boundary requires Shortener API and
Redirect Service to remain .NET 8+ projects, while everything inside the
Statistics bounded context is implemented as Python project/package shells.
Statistics.Api is a FastAPI application shell, Statistics.EventWriter.Worker and
Statistics.BatchProcessor.Worker are Python worker application shells, and any
shared Statistics code is a Python package/module with no business logic. This
keeps runtime ownership explicit without adding Statistics projects to
`DistributedUrlShortener.sln`.

**Alternatives considered**:

- Add Statistics Python projects to the .NET solution: rejected because the
  solution should include only Phase 0 .NET projects from the Shortener bounded
  context.
- Leave Statistics folders as placeholders: rejected because the acceptance
  criteria require minimal runnable Python shells.

## Decision: Use minimal API shells with `/health`

**Rationale**: API shells need to be independently startable and verifiable
without business behavior. Shortener API and Redirect Service use minimal
ASP.NET Core health-only shells. Statistics.Api uses a minimal FastAPI shell. A
basic health endpoint confirms that the process can start and respond while
avoiding dependency checks for services not introduced in Phase 0.

**Alternatives considered**:

- Add full OpenAPI, controllers, or sample endpoints: rejected because this
  would imply product behavior before the relevant phases.
- Add dependency health checks for Cassandra, Redis, Redpanda, MinIO, or
  ClickHouse: rejected because those dependencies are out of Phase 0 scope.

## Decision: Use minimal Python worker shells

**Rationale**: Statistics.EventWriter.Worker and
Statistics.BatchProcessor.Worker must be separate executable processes, but
Phase 0 must not consume events, write Parquet files, schedule jobs, or process
analytics. Minimal Python entry points establish process boundaries without fake
work.

**Alternatives considered**:

- Implement no-op background loops: rejected because loops can look like worker
  behavior and create unnecessary lifecycle complexity.
- Use shared Python modules only for workers: rejected because the constitution
  requires separate runtime processes.

## Decision: Keep Docker Compose as a minimal app-shell baseline

**Rationale**: The constitution requires reproducible local development, and
Phase 0 must provide a Compose foundation that later infrastructure can extend.
The Compose file should focus on project naming, networking, environment values,
and optional app-shell execution rather than introducing Phase 1 data services
early. It must not include Cassandra, Redis, Redpanda, MinIO, ClickHouse,
Keycloak, or observability services in Phase 0, and Phase 1 must extend this
baseline rather than replace it.

**Alternatives considered**:

- Add Cassandra, Redis, Redpanda, MinIO, ClickHouse, and Keycloak immediately:
  rejected because schema initialization, topics, buckets, health checks, and
  integration behavior belong to Phase 1 and later.
- Provide only comments or an empty Compose file: rejected because the baseline
  should be validateable and practical for future extension.

## Decision: Document architecture and trade-offs before implementing behavior

**Rationale**: The repository is portfolio-oriented, so Phase 0 must make
runtime boundaries, redirect independence, authenticated API routing, and the
future analytics flow inspectable before business features are added.

**Alternatives considered**:

- Defer architecture documentation until services exist: rejected because later
  implementation work depends on these boundaries.
- Encode decisions only in README: rejected because the roadmap specifically
  requires `docs/architecture.md` and `docs/tradeoffs.md`.

## Decision: Update `CHANGELOG.md` during planning and implementation

**Rationale**: The repository already has an `Unreleased` changelog section.
Recording the Phase 0 spec and plan artifacts now, then the implemented
foundation files later, keeps repository evolution auditable.

**Alternatives considered**:

- Wait until all Phase 0 implementation is complete: rejected because the spec
  and plan are meaningful repository changes.
- Omit changelog updates for documentation-only work: rejected because the user
  requested changelog consideration and the repository already tracks foundation
  changes there.
