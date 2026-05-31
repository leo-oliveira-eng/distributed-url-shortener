# Feature Specification: Phase 1 Minimal Local Infrastructure

**Feature Branch**: `002-local-infrastructure`  
**Created**: 2026-05-31  
**Status**: Draft  
**Input**: User description: "Specify Phase 1 - Minimal Local Infrastructure for the distributed-url-shortener project. Run the base local infrastructure with Docker Compose, health checks, idempotent initialization scripts, safe local example configuration, and English documentation without implementing application business behavior or the full observability stack."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start the Local Infrastructure (Priority: P1)

A developer can start the Phase 1 local infrastructure from a fresh checkout using the documented example configuration and a single Compose command.

**Why this priority**: Later phases need a reproducible local environment before application behavior can depend on storage, messaging, object storage, analytics storage, identity, or ingress boundaries.

**Independent Test**: Run `docker compose --env-file .env.example config`, then `docker compose --env-file .env.example up -d`, and confirm that the expected Phase 1 services are created on the shared local network.

**Acceptance Scenarios**:

1. **Given** a fresh checkout with the documented local prerequisites, **When** a developer validates the Compose configuration with `.env.example`, **Then** the configuration resolves without missing required variables or committed secrets.
2. **Given** the validated configuration, **When** a developer starts the environment, **Then** Nginx, Redis, Cassandra, Redpanda, MinIO, ClickHouse, and Keycloak start as readable, stable service names on the shared local network.
3. **Given** a stateful Phase 1 service, **When** a developer inspects its Compose definition, **Then** its local state is assigned to a named volume where persistence is appropriate.

---

### User Story 2 - Verify Infrastructure Readiness (Priority: P1)

A developer can determine whether each essential local service is ready before using the environment for later feature work.

**Why this priority**: A running process is not necessarily usable. Explicit readiness checks make failures visible and keep local setup diagnosable.

**Independent Test**: Run `docker compose ps` and the documented readiness commands for every essential Phase 1 service after startup.

**Acceptance Scenarios**:

1. **Given** the environment has started, **When** a developer runs `docker compose ps`, **Then** the documentation makes it clear which services should report healthy and how to verify any service without a practical container health status.
2. **Given** an essential service is still starting, **When** its readiness check runs, **Then** the check reports that the service is not ready instead of silently succeeding.
3. **Given** a service fails readiness validation, **When** a developer consults the local infrastructure documentation, **Then** they can identify the service-specific verification command and the relevant local configuration variables.

---

### User Story 3 - Initialize Local Infrastructure Repeatedly (Priority: P1)

A developer can initialize the Cassandra keyspace, ClickHouse database foundation, Redpanda topics, and MinIO buckets more than once without breaking the local environment or duplicating resources incorrectly.

**Why this priority**: Idempotent initialization is a constitutional requirement and prevents local setup from becoming dependent on an undocumented one-time sequence.

**Independent Test**: Run each documented initialization script twice against the running environment and confirm that both runs succeed while the expected resources remain available.

**Acceptance Scenarios**:

1. **Given** a running Cassandra service, **When** the Cassandra initialization script runs twice, **Then** the local-development keyspace remains valid without destructive recreation and no table is created unless it is indispensable for readiness validation.
2. **Given** a running Redpanda service, **When** the topic initialization script runs twice, **Then** the future `click-events` topic exists exactly as documented without requiring application producers or consumers.
3. **Given** a running MinIO service, **When** the bucket initialization script runs twice, **Then** the future raw analytics bucket exists without writing Parquet files.
4. **Given** a running ClickHouse service, **When** the schema initialization script runs twice, **Then** the minimal analytics database foundation remains valid without dashboard queries or application writes and no table is created unless it is indispensable for readiness validation.

---

### User Story 4 - Understand Active and Deferred Boundaries (Priority: P2)

A maintainer can read the Phase 1 documentation and distinguish active local infrastructure from placeholders and later-phase application behavior.

**Why this priority**: Phase 1 prepares dependencies without pretending that redirect routing, authenticated APIs, analytics processing, or observability have been implemented.

**Independent Test**: Review the Nginx configuration and Phase 1 documentation, then confirm that active behavior, placeholders, and deferred behavior are stated explicitly.

**Acceptance Scenarios**:

1. **Given** the local Nginx configuration, **When** a maintainer reviews it, **Then** any future public redirect boundary and future authenticated API boundary are described as placeholders unless their current behavior is actually available.
2. **Given** the Phase 1 environment includes Keycloak, **When** a maintainer reviews the documentation, **Then** it states whether a safe infrastructure-only local realm import is included or intentionally deferred and confirms that login integration is not implemented.
3. **Given** the Phase 1 documentation, **When** a maintainer reviews the scope, **Then** they can identify that Redirect Service remains independent and no application business behavior or complete observability stack is introduced.

### Edge Cases

- A service may need additional startup time after its container starts; readiness checks must fail until the service is usable.
- Re-running initialization against existing named volumes must preserve the expected resources and must not require manual cleanup.
- A developer may have a local port collision; all exposed local ports must be documented and configurable through safe example variables.
- A required example variable may be missing or blank; Compose validation or the documented verification flow must expose the configuration problem.
- Nginx placeholder boundaries must not proxy to unavailable business endpoints or introduce a Redirect Service dependency on ApiGateway.
- A safe Keycloak realm import may be deferred if Phase 1 gains no infrastructure-readiness value from it; the documentation must record the decision.
- Full observability services must remain absent even though service readiness checks are required.
- Initialization failure for one service must be diagnosable and rerunnable without destructive reset of unrelated local services.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Phase 1 MUST extend the existing `docker-compose.yml` baseline rather than replace the Phase 0 foundation.
- **FR-002**: The local Compose environment MUST include services with stable, readable names for Nginx, Redis, Cassandra, Redpanda, MinIO, ClickHouse, and Keycloak.
- **FR-003**: All Phase 1 services MUST join one documented shared local network.
- **FR-004**: Stateful services MUST use named local volumes where persistence is appropriate.
- **FR-005**: Essential services MUST define practical health checks where supported, and the documentation MUST provide explicit readiness verification commands for every essential service.
- **FR-006**: Phase 1 MUST NOT add Prometheus, Grafana, an OpenTelemetry Collector, Jaeger, Tempo, OpenSearch, Elasticsearch, Fluent Bit, or any equivalent complete observability stack component.
- **FR-007**: `.env.example` MUST retain the Phase 0 application variables and add safe local example values for the Compose project name, application environment, Nginx ports, Redis port, Cassandra ports, Redpanda ports, MinIO API and console ports, ClickHouse HTTP and native ports, and Keycloak port.
- **FR-008**: `.env.example` MUST document safe local-only usernames and passwords where required and MUST include the bucket, topic, keyspace, and database names consumed by initialization scripts.
- **FR-009**: The repository MUST contain `infra/nginx/`, `infra/cassandra/`, `infra/redis/`, `infra/redpanda/`, `infra/minio/`, `infra/clickhouse/`, and `infra/keycloak/`.
- **FR-010**: The repository MUST contain a minimal local Nginx configuration that clearly distinguishes active behavior from placeholder boundaries.
- **FR-011**: The Nginx configuration MAY describe the future public redirect route boundary and future authenticated API boundary at a high level, but MUST NOT assume unavailable business endpoints or make Redirect Service depend on ApiGateway.
- **FR-012**: Redis MUST be available as a local service with a readiness check, but Phase 1 MUST NOT add cache behavior to application code.
- **FR-013**: Cassandra MUST be available as a local service with a local-development readiness check.
- **FR-014**: Phase 1 MUST provide an idempotent Cassandra initialization script that creates a documented local-development keyspace strategy. The default Phase 1 foundation MUST create the keyspace only and MUST NOT create tables.
- **FR-015**: A provisional Cassandra table MAY be created only if it is indispensable for readiness validation. Any such table MUST be narrowly scoped, documented as future query-driven schema, and explicitly documented as unused by applications in Phase 1. Phase 1 MUST NOT add application reads, application writes, high-volume secondary indexes, or generic relational-style joins.
- **FR-016**: Redpanda MUST be available as a local Kafka-compatible broker service with a readiness check.
- **FR-017**: Phase 1 MUST provide an idempotent Redpanda topic initialization script that creates at least the future `click-events` topic without adding application producers or consumers.
- **FR-018**: MinIO MUST be available as a local object-storage service with a readiness check.
- **FR-019**: Phase 1 MUST provide an idempotent MinIO bucket initialization script that creates a documented future raw analytics bucket without writing Parquet files.
- **FR-020**: ClickHouse MUST be available as a local analytical-storage service with a readiness check.
- **FR-021**: Phase 1 MUST provide an idempotent ClickHouse initialization script that creates a documented local-development database foundation. The default Phase 1 foundation MUST create the database only and MUST NOT create tables.
- **FR-022**: A provisional ClickHouse table MAY be created only if it is indispensable for readiness validation. Any such table MUST be narrowly scoped, documented as future analytics schema, and explicitly documented as unused by applications in Phase 1. Phase 1 MUST NOT add dashboard queries or application writes.
- **FR-023**: Keycloak MUST be available as a local identity-infrastructure service with a practical readiness check where supported.
- **FR-024**: A Keycloak realm import MAY be included only when it is safe, local-only, minimal, and infrastructure-only; otherwise, its deferral MUST be documented.
- **FR-025**: Phase 1 MUST NOT implement User API behavior, authentication flows, JWT validation, ApiGateway integration, frontend login, or production identity configuration.
- **FR-026**: The repository MUST contain idempotent initialization scripts for the Cassandra keyspace, ClickHouse database foundation, Redpanda topics, and MinIO buckets under `scripts/` or documented service-specific script locations.
- **FR-027**: Each required initialization script MUST be safe to run at least twice against an already initialized local environment.
- **FR-028**: `docs/local-infrastructure.md` MUST explain prerequisites, startup, readiness verification, initialization, repeated initialization verification, shutdown, active Phase 1 behavior, placeholder behavior, and deferred scope.
- **FR-029**: `docs/architecture.md`, `docs/tradeoffs.md`, and `README.md` MUST be updated where needed to reflect the Phase 1 local environment and preserve the later-phase architecture boundaries.
- **FR-030**: All new and updated documentation, specifications, plans, tasks, decision records, and runbooks MUST be written in English.
- **FR-031**: Runtime applications MUST continue to use documented environment variables and MUST NOT receive committed real secrets, production credentials, or real external endpoints.
- **FR-032**: Redirect Service MUST remain independent from authentication, ApiGateway, Shortener API, dashboard queries, and analytics processing.
- **FR-033**: Phase 1 MUST NOT implement URL creation, redirect resolution, Redis cache logic in application code, Cassandra repository code, Redpanda producers or consumers in application code, MinIO Parquet writing, DuckDB processing, ClickHouse dashboard queries, User API behavior, ApiGateway or Ocelot routing, Angular frontend behavior, or load testing.
- **FR-034**: Phase 1 MAY add infrastructure-only smoke checks or helper scripts but MUST NOT add business tests, end-to-end tests, load tests, or application integration tests.
- **FR-035**: Phase 1 validation MUST document and require successful execution of `docker compose --env-file .env.example config`, `docker compose --env-file .env.example up -d`, `docker compose ps`, service-specific readiness checks, every required initialization script at least twice, and `docker compose down`.
- **FR-036**: Phase 1 documentation MUST explain how the infrastructure supports later phases without claiming that later-phase application behavior is active.

### Constitution Alignment *(mandatory)*

- **Redirect Path Impact**: Phase 1 provides local infrastructure and may expose a placeholder Nginx boundary for future public `GET /{shortCode}` traffic. It MUST NOT implement redirect resolution or route Redirect Service through ApiGateway. Redirect Service remains independent from authentication, Shortener API, dashboard queries, and analytics processing.
- **Shortener API Impact**: None. Phase 1 MUST NOT implement URL creation, URL management, quotas, retention behavior, Cassandra repositories, or Redis cache behavior in Shortener API.
- **Frontend Impact**: None. Angular frontend behavior and frontend login remain out of scope.
- **Authentication/API Gateway Impact**: Keycloak is infrastructure-only in Phase 1. User API behavior, login flows, JWT validation, ApiGateway integration, and Ocelot routing remain out of scope.
- **Storage Impact**: Redis, Cassandra, MinIO, and ClickHouse become locally runnable dependencies. Cassandra initialization defaults to a keyspace only, and ClickHouse initialization defaults to a database only. A provisional table is permitted only when indispensable for readiness validation, must be documented as unused by applications in Phase 1, and must remain narrowly scoped. Cassandra modeling must remain query-driven, and application reads and writes remain out of scope.
- **Analytics Impact**: Redpanda MUST include the future `click-events` topic and MinIO MUST include a future raw analytics bucket where appropriate. ClickHouse receives a database-only foundation by default. Phase 1 MUST NOT publish or consume events, write Parquet files, process data with DuckDB, define dashboard behavior, or query analytics.
- **Observability Impact**: Service health and readiness checks are required. The complete local observability stack remains deferred to Phase 10.
- **Documentation Impact**: Phase 1 requires English local-infrastructure instructions and targeted updates to architecture, trade-offs, and README documentation.

### Key Entities *(include if feature involves data)*

- **Local Infrastructure Environment**: The reproducible Phase 1 set of seven local services, their shared network, named volumes, exposed local ports, and safe example configuration.
- **Initialization Resource**: A minimal local resource created idempotently for later phases, such as a Cassandra keyspace, Redpanda topic, MinIO bucket, or ClickHouse database foundation. Tables are excluded by default and allowed only when indispensable for readiness validation.
- **Nginx Boundary Placeholder**: A documented local ingress configuration that distinguishes current readiness behavior from future redirect and authenticated API boundaries without depending on unavailable application endpoints.
- **Safe Local Configuration Example**: The documented, non-production environment-variable set used to validate and start the local environment without committed real secrets or external endpoints.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can validate and start all 7 required local infrastructure services from a fresh checkout using the documented example configuration and a single startup command.
- **SC-002**: 100% of essential Phase 1 services have a documented readiness verification method, and each service can be confirmed ready after local startup.
- **SC-003**: All 4 required initialization routines complete successfully when run twice in succession, with the expected resources still available after the second run.
- **SC-004**: 100% of required local port, credential-example, and initialization-resource variables are documented with safe local-only values, with 0 committed real secrets and 0 configured real external endpoints.
- **SC-005**: 100% of the 7 required infrastructure folders exist and clearly correspond to the Phase 1 services.
- **SC-006**: The environment can be shut down with the documented command without requiring manual removal of unrelated local resources.
- **SC-007**: A maintainer can identify active Phase 1 behavior, placeholders, deferred observability, and prohibited application behavior from the documentation in under 10 minutes.
- **SC-008**: Phase 1 adds 0 application business behaviors and preserves the Phase 0 runtime boundaries, including Redirect Service independence.

## Assumptions

- The primary Phase 1 users are developers and maintainers preparing a reproducible local environment for later implementation phases.
- Docker and Docker Compose are local prerequisites and are available before Phase 1 startup validation begins.
- Safe local-only example credentials are acceptable for developer infrastructure and are not suitable for production or external exposure.
- Named volumes may preserve local state across normal shutdown; initialization scripts therefore must tolerate existing resources.
- Minimal infrastructure-readiness schema means a Cassandra keyspace and ClickHouse database foundation only. A narrowly scoped provisional table is allowed only when it is indispensable for readiness validation and explicitly documented as unused by applications in Phase 1.
- The future `click-events` topic and raw analytics bucket may be created now because they establish infrastructure readiness without implementing the analytics pipeline.
- A Keycloak realm import is optional in Phase 1 and should be deferred unless it adds clear infrastructure-readiness value without introducing authentication-flow scope.
- Full observability, business tests, application integration tests, end-to-end tests, and load tests belong to later phases.
