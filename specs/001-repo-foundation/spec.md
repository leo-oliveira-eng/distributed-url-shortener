# Feature Specification: Phase 0 Repository Foundation

**Feature Branch**: `001-repo-foundation`  
**Created**: 2026-04-26  
**Status**: Draft  
**Input**: User description: "Create the specification for Phase 0 - Repository Foundation of the distributed-url-shortener project. Phase 0 must prepare repository structure, local configuration baseline, documentation baseline, .NET solution, and initial runnable project shells while preserving the constitution and avoiding business behavior."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Validate Repository Foundation (Priority: P1)

A maintainer can inspect the repository and immediately understand where each bounded context, runtime, infrastructure asset, script, document, and test category belongs before later phases add business behavior.

**Why this priority**: The project cannot evolve safely unless its boundaries and ownership structure are visible from the start.

**Independent Test**: Review the repository tree and confirm that all required top-level folders and bounded-context folders exist with names that match the constitution.

**Acceptance Scenarios**:

1. **Given** a fresh checkout of the repository, **When** a maintainer lists the root structure, **Then** `src/`, `infra/`, `docs/`, `scripts/`, and `tests/` are present.
2. **Given** the `src/` folder, **When** a maintainer inspects bounded contexts, **Then** `ApiGateway/`, `User/`, `Shortener/`, `Statistics/`, and `Front/` are present.
3. **Given** the `Shortener/` and `Statistics/` bounded contexts, **When** a maintainer inspects project folders, **Then** separate folders exist for Shortener API, Redirect Service, Statistics.Api, Statistics.EventWriter.Worker, Statistics.BatchProcessor.Worker, and a shared Statistics Python package/module with no business logic.

---

### User Story 2 - Build Initial Runtime Shells (Priority: P1)

A developer can build the initial solution and verify that every Phase 0 runtime shell is represented as a separate application without any URL shortening, redirect, analytics, dashboard, authentication, or frontend feature behavior.

**Why this priority**: Separate executable boundaries are a constitutional requirement and are the foundation for incremental implementation in later phases.

**Independent Test**: Open the solution and run the documented .NET build verification for Shortener projects, then run the documented Python verification for Statistics shells; all Phase 0 shells start or build according to their runtime without business behavior.

**Acceptance Scenarios**:

1. **Given** the repository foundation is present, **When** a developer opens `DistributedUrlShortener.sln`, **Then** it includes only the Phase 0 .NET projects that belong to the Shortener bounded context.
2. **Given** the solution has been created, **When** a developer builds it, **Then** the Shortener API and Redirect Service shells compile successfully.
3. **Given** the Statistics Python shell folders exist, **When** a developer follows the documented Python setup, **Then** Statistics.Api, Statistics.EventWriter.Worker, and Statistics.BatchProcessor.Worker can be installed or started as minimal Python shells.
4. **Given** the API runtime shells, **When** a developer starts an applicable API shell, **Then** a basic `/health` endpoint is available without exposing business behavior.
5. **Given** the worker runtime shells, **When** a developer reviews or starts their entry points, **Then** each worker has a minimal executable entry point and no event-consuming, scheduling, batch-processing, or analytics behavior.

---

### User Story 3 - Use Local Configuration Baseline (Priority: P2)

A developer can understand the expected local configuration and Docker Compose baseline without relying on committed secrets or manually inventing environment variable names.

**Why this priority**: Reproducible local development is required by the constitution, but Phase 0 should only establish the baseline that later phases extend.

**Independent Test**: Review `.env.example` and `docker-compose.yml`; confirm that they are aligned, secret-free, and structured so Phase 1 infrastructure services can be added without replacing the baseline.

**Acceptance Scenarios**:

1. **Given** the repository foundation, **When** a developer opens `.env.example`, **Then** it documents the environment variables required by Phase 0 shells and local baseline configuration without real secrets.
2. **Given** the repository foundation, **When** a developer opens `docker-compose.yml`, **Then** it uses environment variables where practical and remains intentionally minimal for Phase 0 without Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, or observability services.
3. **Given** later infrastructure work begins, **When** Phase 1 extends local services, **Then** the Phase 0 Compose and environment baseline are extended rather than replaced and bounded-context ownership remains unchanged.

---

### User Story 4 - Understand Architecture and Trade-offs (Priority: P2)

A reviewer can read the documentation baseline and understand the intended architecture, runtime boundaries, redirect-path independence, authenticated API routing, and known limitations of Phase 0.

**Why this priority**: This repository is portfolio-oriented, so architectural clarity and trade-off documentation are part of the deliverable rather than optional polish.

**Independent Test**: Review `docs/architecture.md` and `docs/tradeoffs.md`; confirm that both are written in English and explicitly cover the required Phase 0 architectural decisions.

**Acceptance Scenarios**:

1. **Given** a reviewer opens `docs/architecture.md`, **When** they read the bounded-context overview, **Then** they can identify ApiGateway, User, Shortener, Statistics, and Front responsibilities.
2. **Given** a reviewer opens `docs/architecture.md`, **When** they read the request-flow overview, **Then** it clearly states that future public `GET /{shortCode}` ownership belongs to Redirect Service outside ApiGateway, Phase 0 only allows `/health`, and authenticated APIs go through ApiGateway.
3. **Given** a reviewer opens `docs/tradeoffs.md`, **When** they read the initial decisions, **Then** it explains why Redirect Service is separate from Shortener API, why Statistics runtimes are separate, why Phase 0 creates shells first, and what Phase 0 intentionally does not solve.

### Edge Cases

- Existing placeholder folders may already exist; Phase 0 must validate or refine them without removing unrelated user work.
- Existing documentation may already describe the target system; Phase 0 must preserve useful content while ensuring required baseline documents exist in English.
- Empty runtime shells must not contain fake URL creation, redirect, event publishing, event writing, batch processing, dashboard, authentication, or frontend behavior.
- API endpoints must be limited to shell readiness through `/health`; Redirect Service must not implement `GET /{shortCode}`, redirect logic, cache lookup, or event publishing in Phase 0.
- Local configuration examples must avoid real credentials, tokens, private endpoints, or production-like secrets.
- Compose content must not include Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, observability services, Cassandra schema, Redis behavior, Redpanda topics, MinIO buckets, ClickHouse tables, or Keycloak realm setup.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The repository MUST contain the base folders `src/`, `infra/`, `docs/`, `scripts/`, and `tests/`.
- **FR-002**: The `src/` folder MUST contain bounded-context folders for `ApiGateway/`, `User/`, `Shortener/`, `Statistics/`, and `Front/`.
- **FR-003**: The Shortener bounded context MUST contain separate project folders for Shortener API and Redirect Service.
- **FR-004**: The Statistics bounded context MUST contain Python project/package shells only, including `statistics_api/`, `statistics_event_writer/`, `statistics_batch_processor/`, and `shared/`.
- **FR-005**: The repository MUST include `.env.example`, `docker-compose.yml`, `docs/architecture.md`, and `docs/tradeoffs.md`.
- **FR-006**: The repository MUST include a solution named `DistributedUrlShortener.sln`.
- **FR-007**: The solution MUST include only the Phase 0 .NET project shells that belong to the Shortener bounded context: Shortener API and Redirect Service.
- **FR-008**: Each Shortener project shell MUST compile successfully, and each Statistics Python shell MUST be installable or runnable according to the chosen minimal Python project setup.
- **FR-009**: Each application project shell MUST have a minimal startup entry point.
- **FR-010**: API project shells MUST expose a basic `/health` endpoint when applicable.
- **FR-011**: Project shells MUST use environment-based configuration for Phase 0 settings.
- **FR-012**: Runtime boundaries MUST be explicit in folder names, project names, and solution membership.
- **FR-013**: The Redirect Service shell MUST remain separate from Shortener API and MUST NOT depend on ApiGateway, authentication, Shortener API runtime behavior, dashboard behavior, or analytics processing.
- **FR-014**: Redirect Service MUST expose only `/health` in Phase 0 and MUST NOT implement `GET /{shortCode}`, redirect logic, cache lookup, or event publishing.
- **FR-015**: The Phase 0 scope MUST NOT implement URL creation, redirect resolution, Cassandra schema, Redis cache behavior, Redpanda event publishing, MinIO/Parquet writing, DuckDB batch processing, ClickHouse dashboard queries, Keycloak integration, Angular frontend implementation, observability stack, or load tests.
- **FR-016**: `docker-compose.yml` MUST provide a minimal local-development foundation that is easy to extend in Phase 1.
- **FR-017**: `docker-compose.yml` MUST NOT include Cassandra, Redis, Redpanda, MinIO, ClickHouse, Keycloak, or observability services in Phase 0.
- **FR-018**: Phase 1 infrastructure work MUST extend the Phase 0 `docker-compose.yml` baseline rather than replace it.
- **FR-019**: `docker-compose.yml` MUST use variables from `.env.example` where practical and MUST NOT contain committed secrets.
- **FR-020**: `.env.example` MUST document required Phase 0 variables with safe example values and without real secrets.
- **FR-021**: `docs/architecture.md` MUST describe the intended high-level architecture and the main bounded contexts: ApiGateway, User, Shortener, Statistics, and Front.
- **FR-022**: `docs/architecture.md` MUST explicitly document that future public `GET /{shortCode}` is owned by Redirect Service outside ApiGateway, while Phase 0 only allows `/health`.
- **FR-023**: `docs/architecture.md` MUST document that authenticated APIs go through ApiGateway.
- **FR-024**: `docs/architecture.md` MUST include the planned analytics flow at a high level without implementing analytics behavior.
- **FR-025**: `docs/tradeoffs.md` MUST document initial architectural trade-offs, including separate Redirect Service, separate Python Statistics runtimes, a shared Statistics Python package/module with no business logic, project shells before business behavior, and known Phase 0 limitations.
- **FR-026**: Statistics.Api MUST be a Python FastAPI application shell, Statistics.EventWriter.Worker MUST be a Python worker application shell, Statistics.BatchProcessor.Worker MUST be a Python worker application shell, and any shared Statistics code MUST be a Python package/module with no business logic.
- **FR-027**: All generated project documentation, specs, plans, task lists, architecture docs, and project-doc comments MUST be written in English.
- **FR-028**: Basic build verification MUST be documented as a first-class requirement for Phase 0 completion.
- **FR-029**: Health checks, configuration, documentation, and build verification MUST be treated as first-class Phase 0 requirements rather than follow-up cleanup.
- **FR-030**: Statistics Python shells MUST use Python 3.12+ and keep dependency choices minimal; Statistics.Api may include FastAPI and the minimal server dependency needed to expose `/health`.
- **FR-031**: Statistics.Api MUST expose only a basic `/health` endpoint in Phase 0.
- **FR-032**: Statistics.EventWriter.Worker MUST start successfully but MUST NOT consume events in Phase 0.
- **FR-033**: Statistics.BatchProcessor.Worker MUST start successfully but MUST NOT schedule jobs or process batches in Phase 0.
- **FR-034**: No Python Statistics project or package MUST be added to `DistributedUrlShortener.sln`.

### Constitution Alignment *(mandatory)*

- **Redirect Path Impact**: Phase 0 creates only a separate Redirect Service shell. It must preserve the future public `GET /{shortCode}` boundary outside ApiGateway, but Phase 0 MUST NOT implement `GET /{shortCode}`. Only `/health` is allowed, with no redirect logic, cache lookup, or event publishing.
- **Shortener API Impact**: Phase 0 creates only the Shortener API shell. It must not implement URL creation, URL management, quotas, retention, or ApiGateway behavior.
- **Frontend Impact**: Phase 0 may preserve or create an empty `src/Front/` placeholder. Angular frontend implementation is out of scope.
- **Authentication/API Gateway Impact**: Phase 0 may preserve or create an ApiGateway folder placeholder. Ocelot routing, JWT validation, Keycloak integration, and authenticated route behavior are out of scope.
- **Storage Impact**: Phase 0 must not create Cassandra schemas, Redis cache behavior, retention policies, or storage access paths.
- **Analytics Impact**: Phase 0 creates only Python Statistics runtime shells, a shared Python package/module if needed, and high-level analytics documentation. It must not implement ClickEvent publishing, broker consumption, Parquet storage, batch processing, scheduling, ClickHouse writes, dashboard queries, or shared Statistics business logic.
- **Observability Impact**: Phase 0 includes basic health endpoints for applicable API shells and configuration readiness. Full logs, metrics, traces, and observability stack are out of scope until later phases.
- **Documentation Impact**: Phase 0 requires English documentation updates for architecture, trade-offs, local baseline, runtime boundaries, and build verification.

### Key Entities *(include if feature involves data)*

- **Repository Foundation**: The structural baseline that organizes bounded contexts, infrastructure, documentation, scripts, tests, local configuration, solution membership, and runtime shells.
- **Runtime Shell**: A minimal separately executable application shell that builds, installs, or starts according to its runtime, uses environment-based configuration, and contains no business behavior from later phases.
- **Statistics Shared Package**: A Python package/module under `src/Statistics/shared/` that supports future Statistics code organization but contains no business logic in Phase 0.
- **Documentation Baseline**: The English project documentation that explains architecture, trade-offs, local configuration, runtime boundaries, and Phase 0 limitations.
- **Local Configuration Baseline**: The safe example environment values and minimal Compose foundation used to make future local development reproducible.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of required top-level folders and bounded-context folders exist with names matching the constitution.
- **SC-002**: 100% of required Phase 0 files exist: `.env.example`, `docker-compose.yml`, `docs/architecture.md`, `docs/tradeoffs.md`, and `DistributedUrlShortener.sln`.
- **SC-003**: 100% of required Shortener .NET projects are included in the solution and complete a clean build successfully; 0 Statistics Python projects are included in the .NET solution.
- **SC-004**: 100% of applicable API project shells expose a basic health endpoint.
- **SC-005**: 0 later-phase business capabilities are implemented as part of Phase 0.
- **SC-006**: 100% of generated Phase 0 documentation and specification content is written in English.
- **SC-007**: A reviewer can identify every Phase 0 runtime boundary from folder names, project names, and documentation in under 5 minutes.
- **SC-008**: `.env.example` and `docker-compose.yml` contain 0 committed real secrets, include 0 Phase 1 infrastructure services, and provide a baseline that can be extended in Phase 1.

## Assumptions

- The primary Phase 0 users are maintainers, reviewers, and developers preparing the repository for later implementation phases.
- Existing useful files such as README, roadmap, constitution, `.gitignore`, and placeholder folders should be preserved unless they conflict with the constitution.
- Empty placeholders are acceptable for bounded contexts whose implementation starts in later phases, especially `src/ApiGateway/`, `src/User/`, and `src/Front/`.
- Phase 0 build verification focuses on project shells, not infrastructure dependencies or end-to-end behavior.
- Statistics implementation in Phase 0 uses Python 3.12+ shells only, Statistics.Api uses FastAPI for its health-only API shell, and Shortener implementation remains .NET 8+.
- Later phases will add infrastructure initialization, schemas, real service dependencies, business behavior, frontend implementation, observability, integration tests, and load tests.
