# Research: Phase 0 Repository Foundation

## Decision: Use .NET 8 project shells for Phase 0 runtime applications and a class library for Statistics.Infrastructure

**Rationale**: The feature specification explicitly requires real `.NET 8+`
projects for Shortener API, Redirect Service, Statistics.Api,
Statistics.EventWriter.Worker, and Statistics.BatchProcessor.Worker, plus a
real Statistics.Infrastructure project. Statistics.Infrastructure is a class
library only, not a runtime or executable, and must contain no business logic in
Phase 0. This satisfies the immediate need for buildable project boundaries and
keeps all Phase 0 executables consistent while later phases can revisit
implementation language choices only through an explicit plan and documentation
update.

**Alternatives considered**:

- Use Python for Statistics workers immediately: rejected because the Phase 0
  specification asks for `.NET 8+` projects and the constitution requires
  separate runtimes rather than a specific Statistics implementation language.
- Leave Statistics folders as placeholders: rejected because the acceptance
  criteria require real projects that build.

## Decision: Use minimal ASP.NET Core API shells with `/health`

**Rationale**: API shells need to be independently startable and verifiable
without business behavior. A basic health endpoint confirms that the process can
start and respond while avoiding dependency checks for services not introduced
in Phase 0.

**Alternatives considered**:

- Add full OpenAPI, controllers, or sample endpoints: rejected because this
  would imply product behavior before the relevant phases.
- Add dependency health checks for Cassandra, Redis, Redpanda, MinIO, or
  ClickHouse: rejected because those dependencies are out of Phase 0 scope.

## Decision: Use minimal Generic Host worker shells

**Rationale**: Statistics.EventWriter.Worker and
Statistics.BatchProcessor.Worker must be separate executable processes, but
Phase 0 must not consume events, write Parquet files, schedule jobs, or process
analytics. Minimal Generic Host entry points establish process boundaries
without fake work.

**Alternatives considered**:

- Implement no-op background loops: rejected because loops can look like worker
  behavior and create unnecessary lifecycle complexity.
- Use class libraries for workers: rejected because the constitution requires
  separate runtime processes.

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
