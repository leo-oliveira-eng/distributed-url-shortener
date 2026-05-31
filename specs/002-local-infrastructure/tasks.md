# Tasks: Phase 1 Minimal Local Infrastructure

**Input**: Approved design documents from `/specs/002-local-infrastructure/`
**Prerequisites**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/phase1-local-infrastructure-contracts.md`, `quickstart.md`

**Tests**: Phase 1 is infrastructure-only. Validation is limited to Compose rendering, startup, status inspection, service-specific readiness checks, idempotent initializer execution, expected-resource inspection, security review, scope review, and normal shutdown. Do not add business tests, application integration tests, end-to-end tests, or load tests.

**Organization**: Tasks are grouped in the approved implementation order: Setup, Infrastructure Configuration, Compose Services, Initialization Scripts, Documentation, Validation, and Polish. Story labels preserve traceability to the approved user stories without introducing application-feature work.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel because it touches a different file and has no dependency on another incomplete task in the phase.
- **[Story]**: Maps the task to an approved specification story: `[US1]` startup, `[US2]` readiness, `[US3]` repeatable initialization, or `[US4]` active and deferred boundaries.
- Every task includes concrete repository file paths and an independently verifiable completion condition.

## Path Conventions

- Compose orchestration: `docker-compose.yml`
- Safe local configuration: `.env.example`
- Infrastructure configuration: `infra/`
- Initialization scripts: `scripts/init/`
- Local runbook: `docs/local-infrastructure.md`
- Architecture documentation: `docs/architecture.md`
- Trade-off documentation: `docs/tradeoffs.md`
- Application shells that must remain unchanged: `src/`

---

## Phase 1: Setup

**Purpose**: Prepare the Phase 1 file layout while preserving the approved Phase 0 baseline and existing user work.

- [ ] T001 Inspect `docker-compose.yml`, `.env.example`, `README.md`, `CHANGELOG.md`, `docs/architecture.md`, `docs/tradeoffs.md`, `infra/.gitkeep`, `scripts/.gitkeep`, and `src/` before editing; record that Phase 1 extends the existing Compose baseline and does not modify application shells.
- [ ] T002 Create the required service ownership directories `infra/nginx/`, `infra/redis/`, `infra/cassandra/`, `infra/redpanda/`, `infra/minio/`, `infra/clickhouse/`, and `infra/keycloak/`.
- [ ] T003 Create `scripts/init/` for the four explicit infrastructure initialization scripts, then remove `infra/.gitkeep` and `scripts/.gitkeep` only after real files exist under `infra/` and `scripts/`.

**Checkpoint**: The repository has explicit Phase 1 infrastructure and initialization ownership paths without any `src/` changes.

---

## Phase 2: Infrastructure Configuration

**Purpose**: Add safe local configuration and tracked service-owned files before wiring Compose services.

- [ ] T004 [US1] Extend `.env.example` without removing Phase 0 variables: add the exact image or source tags, loopback host ports, safe local-only credentials, and initialization resource names listed in `specs/002-local-infrastructure/plan.md`; label credentials unsuitable for production and keep `NGINX_HTTPS_PORT` reserved but unmapped.
- [ ] T005 [P] [US4] Create `infra/nginx/nginx.conf` with an active `GET /healthz` success response, explicit non-success handling for other requests, and comments documenting future public redirect and authenticated API boundaries as placeholders only; do not add any proxy route.
- [ ] T006 [P] [US2] Create `infra/redis/redis.conf` with local persistence settings and document that authenticated access is completed by `REDIS_PASSWORD` wiring in `docker-compose.yml`; do not add application cache behavior.
- [ ] T007 [P] [US2] Create `infra/cassandra/README.md` documenting the single-node local-development configuration, CQL readiness command, slow-start expectation, keyspace-only initialization boundary, and prohibition on Phase 1 tables, indexes, repositories, reads, or writes.
- [ ] T008 [P] [US2] Create `infra/redpanda/README.md` documenting one-node developer mode, internal versus loopback-published Kafka and admin listeners, `rpk cluster health` readiness, topic-only initialization, and the absence of producers, consumers, schemas, or messages.
- [ ] T009 [P] [US2] Create `infra/minio/Dockerfile` as a local multi-stage build of MinIO Community Edition from official source tag `RELEASE.2025-10-15T17-29-55Z`, producing an Alpine-based runtime that can serve MinIO and execute the planned readiness check without using an unofficial or floating server image.
- [ ] T010 [P] [US2] Create `infra/minio/README.md` documenting the pinned source build, slower first startup, API readiness endpoint, local-only exposure, `raw-events` bucket-only initialization, absence of objects and Parquet files, and archived-upstream maintenance risk.
- [ ] T011 [P] [US2] Create `infra/clickhouse/README.md` documenting HTTP and native ports, the loopback native-port default chosen to avoid the MinIO collision, authenticated `SELECT 1` readiness, database-only initialization, and the prohibition on Phase 1 tables, views, inserts, or dashboard queries.
- [ ] T012 [P] [US4] Create `infra/keycloak/README.md` documenting local `start-dev` usage, bootstrap admin examples, management `/health/ready`, loopback-only exposure, intentionally deferred realm import, and the absence of login flows, clients, users, JWT validation, and production identity configuration.

**Checkpoint**: Every tracked service-specific file is local-development focused, version-pinned where applicable, and explicit about deferred functionality.

---

## Phase 3: Compose Services

**Purpose**: Extend the single Phase 0 Compose baseline with the approved long-running services and opt-in helpers.

**CRITICAL**: All tasks in this phase touch `docker-compose.yml` and must run sequentially.

- [ ] T013 [US1] Extend `docker-compose.yml` in place while retaining the Phase 0 `x-phase0` anchor and existing `app` network; declare named volumes `redis-data`, `cassandra-data`, `redpanda-data`, `minio-data`, `clickhouse-data`, and `keycloak-data`.
- [ ] T014 [US1] Add `nginx` and `redis` services to `docker-compose.yml`: join both to `app`, bind published ports to `127.0.0.1`, mount `infra/nginx/nginx.conf` and `infra/redis/redis.conf` read-only, persist Redis data in `redis-data`, wire authenticated Redis startup from `REDIS_PASSWORD`, and add practical health checks for Nginx `/healthz` and Redis authenticated `PING`.
- [ ] T015 [US1] Add the `cassandra` service to `docker-compose.yml`: use the exact configured image, join `app`, bind the configured CQL and diagnostic internode ports to `127.0.0.1`, persist `/var/lib/cassandra` in `cassandra-data`, configure the documented local cluster name, and add a generous-start-period `cqlsh -e "DESCRIBE CLUSTER"` health check.
- [ ] T016 [US1] Add the `redpanda` service to `docker-compose.yml`: use the exact configured image, join `app`, persist `/var/lib/redpanda/data` in `redpanda-data`, configure one-node developer mode with stable internal and loopback-published external Kafka/admin listeners, and add an `rpk cluster health` health check that fails until the one-node cluster is healthy.
- [ ] T017 [US1] Add the `minio` service to `docker-compose.yml`: build `infra/minio/Dockerfile` with `MINIO_SOURCE_TAG`, join `app`, bind API and console ports to `127.0.0.1`, persist `/data` in `minio-data`, wire safe root credentials, start the server with its console address, and add `/minio/health/ready` health validation.
- [ ] T018 [US1] Add the `clickhouse` service to `docker-compose.yml`: use the exact configured image, join `app`, bind HTTP and native ports to `127.0.0.1`, persist `/var/lib/clickhouse` in `clickhouse-data`, wire local user/password variables without auto-creating application schema, and add authenticated `SELECT 1` health validation.
- [ ] T019 [US1] Add the `keycloak` service to `docker-compose.yml`: use the exact configured image, join `app`, bind HTTP and management ports to `127.0.0.1`, persist `/opt/keycloak/data` in `keycloak-data`, wire safe bootstrap admin variables, enable health, run `start-dev` without realm import, and add a management `/health/ready` health check with a realistic start period.
- [ ] T020 [US3] Add `cassandra-init`, `redpanda-init`, `minio-init`, and `clickhouse-init` helper services to `docker-compose.yml`; place each under profile `init`, join `app`, mount only its matching `scripts/init/*.sh` file read-only, pass only required local variables, wait on the matching healthy long-running service, and keep default startup limited to the seven long-running services.
- [ ] T021 [US1] Audit `docker-compose.yml` after service wiring: confirm every long-running service and helper joins `app`, all published host bindings use `127.0.0.1`, all six stateful services use named volumes, tracked configuration mounts are read-only, all required `.env.example` variables are consumed consistently, no `latest` long-running image tag exists, and no observability or application service was added.

**Checkpoint**: `docker-compose.yml` defines exactly seven default long-running infrastructure services and four opt-in initialization helpers.

---

## Phase 4: Initialization Scripts

**Purpose**: Add visible, isolated, idempotent resource creation for later phases.

- [ ] T022 [P] [US3] Create LF-encoded `scripts/init/cassandra-keyspace.sh`: require a non-empty `CASSANDRA_KEYSPACE`, fail visibly when `cassandra` is unavailable, wait for `cqlsh` readiness, run `CREATE KEYSPACE IF NOT EXISTS` with local-only `SimpleStrategy` and replication factor `1`, print a clear result, and verify rerun safety without creating any table or destructive reset.
- [ ] T023 [P] [US3] Create LF-encoded `scripts/init/redpanda-topics.sh`: require a non-empty `REDPANDA_CLICK_EVENTS_TOPIC`, fail visibly when `redpanda` is unavailable or unhealthy, detect whether the topic exists, create it only when absent with one partition and replication factor `1`, print a clear result, and verify rerun safety without producing messages or adding schemas.
- [ ] T024 [P] [US3] Create LF-encoded `scripts/init/minio-buckets.sh`: require non-empty MinIO credentials and `MINIO_RAW_EVENTS_BUCKET`, fail visibly when the MinIO alias cannot be configured, run `mc mb --ignore-existing local/${MINIO_RAW_EVENTS_BUCKET}`, print a clear result, and verify rerun safety without writing objects or Parquet files.
- [ ] T025 [P] [US3] Create LF-encoded `scripts/init/clickhouse-database.sh`: require non-empty ClickHouse credentials and `CLICKHOUSE_DATABASE`, fail visibly when authenticated `SELECT 1` readiness fails, run `CREATE DATABASE IF NOT EXISTS`, print a clear result, and verify rerun safety without creating tables, views, inserts, or dashboard queries.

**Checkpoint**: Each initializer is independently rerunnable, fails clearly when its dependency or required variables are invalid, and creates only its approved minimal resource.

---

## Phase 5: Documentation

**Purpose**: Make startup, readiness, initialization, shutdown, active boundaries, and deferred scope inspectable in English.

- [ ] T026 [US4] Create `docs/local-infrastructure.md` as the Phase 1 runbook: document Docker prerequisites, exact images and MinIO source build, shared network, named volumes, all configurable loopback ports, safe local-only credentials, `docker compose --env-file .env.example config`, `up -d --build`, `ps`, all seven readiness commands, all four `init` profile commands, double-execution verification, four resource-inspection commands, normal `down`, optional explicit volume reset, port-conflict and delayed-readiness diagnostics, active Nginx health behavior, placeholder boundaries, and deferred functionality.
- [ ] T027 [P] [US4] Update `docs/architecture.md` with a Phase 1 section describing the seven active local infrastructure services, one shared network, Nginx health-only ingress, unchanged separate application shells, Redirect Service independence outside ApiGateway, and empty analytics foundations without an active pipeline.
- [ ] T028 [P] [US4] Update `docs/tradeoffs.md` with Phase 1 decisions: single-node Compose versus production topology, explicit opt-in initialization versus hidden bootstrap, loopback-only examples versus production hardening, deferred Keycloak realm import, and the required MinIO source-only archived-upstream risk.
- [ ] T029 [US4] Update `README.md` from Phase 0 foundation status to Phase 1 minimal local infrastructure status; link `docs/local-infrastructure.md` and `specs/002-local-infrastructure/quickstart.md`, summarize startup/readiness/initialization/shutdown entry points, and keep later URL, redirect, analytics, authentication, frontend, and observability behavior explicitly deferred.
- [ ] T030 Update `CHANGELOG.md` under `Unreleased` with the seven-service Phase 1 Compose environment, tracked infrastructure configuration, four idempotent initialization scripts, English documentation updates, and the explicit absence of application business behavior.

**Checkpoint**: A maintainer can follow one English runbook and distinguish active infrastructure from placeholders and later-phase application behavior.

---

## Phase 6: Validation

**Purpose**: Execute the approved infrastructure smoke flow and verify constitutional scope protection.

### Compose Rendering And Startup

- [ ] T031 [US1] Run `docker compose --env-file .env.example config` against `.env.example` and `docker-compose.yml`; verify rendering succeeds with no blank required variable, no committed real secret, no external endpoint, no floating long-running image tag, exactly seven default long-running services, four `init` profile helpers, six named volumes, the shared `app` network, and no observability service.
- [ ] T032 [US1] Run `docker compose --env-file .env.example up -d --build` against `docker-compose.yml`; verify the pinned MinIO source image builds and exactly `nginx`, `redis`, `cassandra`, `redpanda`, `minio`, `clickhouse`, and `keycloak` start by default.
- [ ] T033 [US2] Run `docker compose --env-file .env.example ps` against `docker-compose.yml`; wait through documented startup windows and verify all seven long-running services are present and report healthy status.

### Service Readiness

- [ ] T034 [US2] Verify Nginx readiness from `infra/nginx/nginx.conf` with `Invoke-WebRequest http://localhost:<nginx-http-port>/healthz`; confirm success occurs only for the active health endpoint.
- [ ] T035 [US2] Verify Redis readiness from `infra/redis/redis.conf` with `docker compose --env-file .env.example exec -T redis redis-cli -a <redis-password> ping`; confirm authenticated `PONG`.
- [ ] T036 [US2] Verify Cassandra readiness documented in `infra/cassandra/README.md` with `docker compose --env-file .env.example exec -T cassandra cqlsh -e "DESCRIBE CLUSTER"`; confirm the command fails while bootstrapping and succeeds only when usable.
- [ ] T037 [US2] Verify Redpanda readiness documented in `infra/redpanda/README.md` with `docker compose --env-file .env.example exec -T redpanda rpk cluster health`; confirm it reports a healthy one-node cluster.
- [ ] T038 [US2] Verify MinIO readiness documented in `infra/minio/README.md` with `Invoke-WebRequest http://localhost:<minio-api-port>/minio/health/ready`; confirm success only after the server is ready.
- [ ] T039 [US2] Verify ClickHouse readiness documented in `infra/clickhouse/README.md` with `docker compose --env-file .env.example exec -T clickhouse clickhouse-client --user <clickhouse-user> --password <clickhouse-password> --query "SELECT 1"`; confirm authenticated success.
- [ ] T040 [US2] Verify Keycloak readiness documented in `infra/keycloak/README.md` with `Invoke-WebRequest http://localhost:<keycloak-management-port>/health/ready`; confirm success only after `start-dev` bootstrapping completes.

### Repeatable Initialization And Resource Inspection

- [ ] T041 [US3] Run `docker compose --env-file .env.example --profile init run --rm cassandra-init` twice from `docker-compose.yml`; inspect with `cqlsh -e "DESCRIBE KEYSPACES"` and confirm the configured keyspace remains available with no Phase 1 Cassandra table.
- [ ] T042 [US3] Run `docker compose --env-file .env.example --profile init run --rm redpanda-init` twice from `docker-compose.yml`; inspect with `rpk topic list` and confirm exactly the configured future `click-events` topic exists without requiring any message.
- [ ] T043 [US3] Run `docker compose --env-file .env.example --profile init run --rm minio-init` twice from `docker-compose.yml`; inspect through the documented `minio-init` client command and confirm the configured future `raw-events` bucket exists and contains no object.
- [ ] T044 [US3] Run `docker compose --env-file .env.example --profile init run --rm clickhouse-init` twice from `docker-compose.yml`; inspect with authenticated `SHOW DATABASES` and confirm the configured database exists with no Phase 1 ClickHouse table or view.

### Security And Boundary Review

- [ ] T045 [US4] Inspect `infra/nginx/nginx.conf` and rendered `docker-compose.yml`; confirm `/healthz` is the only active Nginx route, future redirect and authenticated API boundaries do not proxy, and Redirect Service remains independent from ApiGateway, authentication, Shortener API, dashboard queries, and analytics processing.
- [ ] T046 [US4] Inspect `.env.example`, rendered `docker-compose.yml`, and `infra/minio/Dockerfile`; confirm every published port binds to `127.0.0.1`, example credentials are local-only, no production credential or real external endpoint exists, Keycloak uses `start-dev`, and MinIO builds from exact source tag `RELEASE.2025-10-15T17-29-55Z`.
- [ ] T047 [US4] Inspect `src/Shortener/Shortener.Api/`, `src/Shortener/Redirect.Service/`, and `docker-compose.yml`; confirm Phase 1 adds no URL creation, URL listing, quota enforcement, redirect resolution, Redis cache logic in application code, or application dependency wiring.
- [ ] T048 [US4] Inspect `src/`, `scripts/init/cassandra-keyspace.sh`, and `scripts/init/clickhouse-database.sh`; confirm Phase 1 adds no Cassandra repository code, Cassandra table, Cassandra index, ClickHouse table, ClickHouse view, application read, application write, or dashboard query.
- [ ] T049 [US4] Inspect `src/Statistics/`, `scripts/init/redpanda-topics.sh`, `scripts/init/minio-buckets.sh`, and `docker-compose.yml`; confirm Phase 1 adds no Redpanda producer or consumer, message, schema, MinIO object, Parquet file, DuckDB dependency, batch processing, analytics processing, or aggregate schema.
- [ ] T050 [US4] Inspect `src/User/`, `src/ApiGateway/`, `src/Front/`, `infra/keycloak/README.md`, and `docker-compose.yml`; confirm Phase 1 adds no User API behavior, realm import, client, user, login flow, JWT validation, Ocelot or ApiGateway routing, Angular work, or production identity configuration.
- [ ] T051 [US4] Inspect `docker-compose.yml`, `infra/`, and `docs/local-infrastructure.md`; confirm Phase 1 adds readiness checks only and no Prometheus, Grafana, OpenTelemetry Collector, Jaeger, Tempo, OpenSearch, Elasticsearch, Fluent Bit, or equivalent observability-stack component.
- [ ] T052 [US4] Inspect `tests/`, `src/`, and the final changed-file list from `git status --short`; confirm Phase 1 adds no business test, application integration test, end-to-end test, load test, or application-code modification.

### Shutdown

- [ ] T053 [US1] Run `docker compose --env-file .env.example down` against `docker-compose.yml`; verify containers and the Compose network stop without deleting named volumes or requiring cleanup of unrelated local resources.

**Checkpoint**: The complete Phase 1 smoke flow succeeds, all resources survive repeated initialization, normal shutdown is non-destructive, and prohibited later-phase behavior remains absent.

---

## Phase 7: Polish

**Purpose**: Perform final documentation, format, and changed-file quality checks.

- [ ] T054 [P] Verify all touched documentation and project-doc comments are in English in `.env.example`, `docker-compose.yml`, `README.md`, `CHANGELOG.md`, `docs/local-infrastructure.md`, `docs/architecture.md`, `docs/tradeoffs.md`, `infra/nginx/nginx.conf`, `infra/redis/redis.conf`, and `infra/*/README.md`.
- [ ] T055 [P] Verify `scripts/init/cassandra-keyspace.sh`, `scripts/init/redpanda-topics.sh`, `scripts/init/minio-buckets.sh`, and `scripts/init/clickhouse-database.sh` are LF-encoded, executable where required by the Compose invocation, narrowly scoped, and free of destructive reset behavior.
- [ ] T056 Verify the completed implementation against `specs/002-local-infrastructure/spec.md`, `specs/002-local-infrastructure/plan.md`, `specs/002-local-infrastructure/data-model.md`, `specs/002-local-infrastructure/contracts/phase1-local-infrastructure-contracts.md`, and `specs/002-local-infrastructure/quickstart.md`.
- [ ] T057 Run `git status --short` and review the final changed-file list against `specs/002-local-infrastructure/plan.md`; confirm only Phase 1 infrastructure, scripts, configuration, documentation, and approved planning artifacts changed, with no unrelated user work reverted.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start here.
- **Infrastructure Configuration (Phase 2)**: Depends on Setup paths.
- **Compose Services (Phase 3)**: Depends on `.env.example` and tracked service configuration from Phase 2; tasks T013-T021 are sequential because they all edit `docker-compose.yml`.
- **Initialization Scripts (Phase 4)**: Depends on Compose helper contracts from T020; T022-T025 can be implemented in parallel.
- **Documentation (Phase 5)**: Depends on stable configuration, Compose service names, and initializer commands.
- **Validation (Phase 6)**: Depends on all implementation and documentation tasks.
- **Polish (Phase 7)**: Depends on successful validation.

### User Story Traceability

- **User Story 1 (P1) - Start the Local Infrastructure**: T004, T013-T019, T021, T031-T032, T053.
- **User Story 2 (P1) - Verify Infrastructure Readiness**: T006-T012, T033-T040.
- **User Story 3 (P1) - Initialize Local Infrastructure Repeatedly**: T020, T022-T025, T041-T044.
- **User Story 4 (P2) - Understand Active and Deferred Boundaries**: T005, T012, T026-T029, T045-T052.

### Critical Path

1. Complete T001-T004.
2. Complete independent service files T005-T012.
3. Extend `docker-compose.yml` sequentially with T013-T021.
4. Complete initializer scripts T022-T025.
5. Complete documentation T026-T030.
6. Execute the full smoke flow T031-T053.
7. Complete polish checks T054-T057.

### Parallel Opportunities

- T005-T012 can run in parallel after T004 because each task owns a different service-specific file.
- T022-T025 can run in parallel after T020 because each task owns a different initializer script.
- T027 and T028 can run in parallel after T026 because they touch different documentation files.
- T054 and T055 can run in parallel after validation because they inspect different quality dimensions.

---

## Parallel Example: Infrastructure Configuration

```text
Task: "Create infra/nginx/nginx.conf with health-only active behavior"
Task: "Create infra/redis/redis.conf with local persistence configuration"
Task: "Create infra/minio/Dockerfile from the pinned official source tag"
Task: "Create infra/keycloak/README.md documenting start-dev and deferred realm import"
```

## Parallel Example: Initialization Scripts

```text
Task: "Create scripts/init/cassandra-keyspace.sh"
Task: "Create scripts/init/redpanda-topics.sh"
Task: "Create scripts/init/minio-buckets.sh"
Task: "Create scripts/init/clickhouse-database.sh"
```

---

## Implementation Strategy

### MVP First

1. Complete Setup, Infrastructure Configuration, and Compose Services.
2. Validate Compose rendering and default startup with T031-T033.
3. Verify all seven readiness checks with T034-T040.
4. Stop and review before adding initialization resources.

### Incremental Delivery

1. Establish the seven-service local environment.
2. Add the four explicit initialization scripts and run each twice.
3. Document active and deferred boundaries.
4. Run security, constitution, and shutdown validation.

### Scope Guard

- Do not modify files under `src/`.
- Do not add Cassandra tables or ClickHouse tables.
- Do not add Redpanda producers, consumers, schemas, or messages.
- Do not add MinIO objects, Parquet files, or DuckDB.
- Do not add JWT validation, ApiGateway routing, Ocelot configuration, Angular work, business tests, application integration tests, end-to-end tests, load tests, or an observability stack.
- Keep Redirect Service independent and keep Nginx limited to its active `/healthz` response plus documented placeholder boundaries.

