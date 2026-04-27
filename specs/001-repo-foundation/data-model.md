# Data Model: Phase 0 Repository Foundation

Phase 0 does not introduce business data, persistence schemas, message payloads,
or dashboard aggregates. The entities below describe repository and planning
artifacts that must exist after implementation.

## Repository Foundation

**Purpose**: Defines the canonical monorepo structure required before later
phases add behavior.

**Fields**:

- `rootFolders`: `src/`, `infra/`, `docs/`, `scripts/`, `tests/`
- `boundedContexts`: `ApiGateway`, `User`, `Shortener`, `Statistics`, `Front`
- `foundationFiles`: `.env.example`, `docker-compose.yml`,
  `DistributedUrlShortener.sln`, `docs/architecture.md`, `docs/tradeoffs.md`
- `changeLog`: `CHANGELOG.md`

**Validation Rules**:

- All root folders must exist.
- All bounded-context folders must exist under `src/`.
- Foundation files must be written in English where they contain prose.
- Existing unrelated user changes must not be reverted.

## Runtime Shell

**Purpose**: Represents a separately buildable process boundary for later
runtime behavior.

**Fields**:

- `name`: Runtime display name
- `projectPath`: Repository-relative project folder
- `projectFile`: Project file path
- `entryPoint`: Minimal startup file
- `runtimeKind`: `api`, `worker`, or `library`
- `healthEndpoint`: Optional endpoint for API shells
- `businessBehaviorAllowed`: Always `false` for Phase 0

**Validation Rules**:

- Application runtime shells must compile.
- Application runtime shells must have a minimal startup entry point.
- API shells must expose a basic `/health` endpoint.
- Worker shells must be executable projects, not class libraries.
- `Statistics.Infrastructure` must be a supporting project and must not become
  a standalone runtime or executable.
- `Statistics.Infrastructure` must be a class library only and must not contain
  business logic in Phase 0.

## Solution

**Purpose**: Groups the Phase 0 projects for build verification.

**Fields**:

- `name`: `DistributedUrlShortener.sln`
- `projects`: Shortener API, Redirect Service, Statistics.Api,
  Statistics.EventWriter.Worker, Statistics.BatchProcessor.Worker,
  Statistics.Infrastructure class library

**Validation Rules**:

- The solution must include every Phase 0 project.
- `dotnet build DistributedUrlShortener.sln` must complete successfully.

## Local Configuration Baseline

**Purpose**: Documents safe local configuration values and Compose variables.

**Fields**:

- `environmentFile`: `.env.example`
- `composeFile`: `docker-compose.yml`
- `safeExampleValues`: Non-secret local values only
- `phase0Ports`: Optional API-shell ports for local development

**Validation Rules**:

- No real secrets, tokens, passwords, or production endpoints may be committed.
- Compose variables must align with `.env.example` where practical.
- `docker-compose.yml` must not include Cassandra, Redis, Redpanda, MinIO,
  ClickHouse, Keycloak, or observability services in Phase 0.
- Phase 1 infrastructure services must extend the baseline rather than replace
  it.

## Documentation Baseline

**Purpose**: Explains the intended architecture, trade-offs, limitations, and
foundation changes.

**Fields**:

- `architectureDocument`: `docs/architecture.md`
- `tradeoffsDocument`: `docs/tradeoffs.md`
- `changelogDocument`: `CHANGELOG.md`

**Validation Rules**:

- Documentation must be in English.
- Architecture documentation must identify bounded contexts and high-level
  flows.
- Trade-off documentation must explain the separate Redirect Service, separate
  Statistics runtimes, shell-first approach, and known Phase 0 limitations.
- Changelog entries must describe repository changes without implying completed
  business behavior.
