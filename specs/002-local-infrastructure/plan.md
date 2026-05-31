# Implementation Plan: Phase 1 Minimal Local Infrastructure

**Branch**: `002-local-infrastructure` | **Date**: 2026-05-31 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-local-infrastructure/spec.md`

## Summary

Extend the Phase 0 Compose baseline into a reproducible local infrastructure
environment with Nginx, Redis, Cassandra, Redpanda, MinIO, ClickHouse, and
Keycloak. Keep all Phase 0 application runtimes unchanged. Add practical
readiness checks, named volumes for stateful services, safe local-only example
configuration, and four idempotent initialization routines that create only a
Cassandra keyspace, Redpanda `click-events` topic, MinIO `raw-events` bucket,
and ClickHouse database.

The Compose design uses one shared `app` network, loopback-only host port
bindings, exact image tags, read-only configuration mounts, and opt-in
initialization helpers under the `init` profile. Nginx exposes only its own
health response and documented placeholders. Keycloak runs in development mode
without a realm import. The complete observability stack remains deferred.

## Technical Context

**Language/Version**: Docker Compose v2 YAML; POSIX shell scripts executed
inside Linux containers; existing .NET 8 and Python 3.12+ application shells
remain unchanged

**Primary Dependencies**: Nginx `1.30.2-alpine`, Redis `8.6.3-alpine`,
Cassandra `5.0.8-bookworm`, Redpanda `v26.1.4`, MinIO Community Edition built
locally from official source tag `RELEASE.2025-10-15T17-29-55Z`, MinIO Client
`RELEASE.2025-08-13T08-35-41Z`, ClickHouse `26.5.1.882-jammy`, Keycloak
`26.6.2`

**Storage**: Docker named volumes for Redis, Cassandra, Redpanda, MinIO,
ClickHouse, and Keycloak local development state; no application-owned data;
no Cassandra or ClickHouse tables

**Testing**: Compose rendering, startup, status inspection, per-service
readiness verification, double execution of each initializer, expected-resource
inspection, security review of committed configuration, and normal shutdown

**Target Platform**: Local Docker Compose environment on a developer machine

**Project Type**: Infrastructure-only extension of a monorepo distributed
backend foundation

**Performance Goals**: N/A for application throughput or latency; local startup
must be diagnosable and all services must become verifiably ready

**Constraints**: Preserve Redirect Service independence and all Phase 0 runtime
boundaries; use query-driven Cassandra principles; keep analytics non-blocking;
do not implement business behavior, application dependency wiring, full
observability, authentication flows, or production hardening; write all
documentation in English

**Scale/Scope**: One local instance of each required service. Production
clustering, capacity tuning, high availability, cloud deployment, and later
application behavior are intentionally deferred.

## Constitution Check

*GATE: PASS before research. Re-check after design.*

- **Redirect Service Independence**: PASS. Nginx does not proxy to Redirect
  Service or ApiGateway. Redirect Service remains an unchanged independent
  Phase 0 shell. Public redirect routing and `GET /{shortCode}` remain deferred.
- **Query-Driven Cassandra Principles**: PASS. Cassandra becomes runnable, but
  initialization creates only one local keyspace. No table, repository,
  secondary index, join-like model, query pattern implementation, or TTL policy
  is introduced. Phase 2 must define query-driven tables.
- **Non-Blocking Analytics Architecture**: PASS. Redpanda receives only the
  future `click-events` topic, MinIO only the future `raw-events` bucket, and
  ClickHouse only an empty database. No producer, consumer, Parquet write,
  DuckDB processing, aggregate table, or dashboard query is added.
- **Separate Runtime Execution Models**: PASS. Existing APIs, workers, and
  Redirect Service remain separate processes. Compose adds infrastructure
  processes and short-lived initialization helpers only.
- **Reproducible Local Development**: PASS. Compose uses exact versions, one
  documented network, named volumes, safe `.env.example` values, readiness
  checks, and idempotent scripts.
- **English-Only Documentation**: PASS. Planning artifacts and planned
  documentation updates are English-only.
- **Observability Phase Boundary**: PASS. Container health and readiness checks
  are included. Prometheus, Grafana, OpenTelemetry Collector, tracing backends,
  and logging backends remain deferred to Phase 10.
- **Frontend/API Gateway Boundary**: PASS. `src/Front/` and `src/ApiGateway/`
  remain untouched. No authenticated routing or Ocelot configuration is added.
- **Testing Boundary**: PASS. Validation is infrastructure smoke verification
  only. No business, application integration, end-to-end, or load tests are
  introduced.

## Technical Approach

### Docker Compose Strategy

Extend the existing `docker-compose.yml` in place. Retain the Phase 0
configuration anchor and `app` network, then add the seven required
infrastructure services. Add four short-lived helper services under the
`init` profile:

- `cassandra-init`
- `redpanda-init`
- `minio-init`
- `clickhouse-init`

The default `docker compose --env-file .env.example up -d` command starts only
the seven long-running services. Developers run initialization explicitly after
readiness succeeds. This keeps startup behavior visible and makes each
initializer rerunnable in isolation.

### Network Topology

All long-running services and initialization helpers join the existing
`${COMPOSE_PROJECT_NAME}-app` bridge network through the Compose network key
`app`. Service discovery uses stable Compose names:

```text
nginx
redis
cassandra
redpanda
minio
clickhouse
keycloak
```

Host port mappings bind to `127.0.0.1` only. This is a local developer
environment, not a remotely exposed stack. Containers communicate through
service names and internal container ports, not host ports.

### Volume Strategy

Use Docker named volumes for stateful local services:

```text
redis-data
cassandra-data
redpanda-data
minio-data
clickhouse-data
keycloak-data
```

Nginx is stateless and uses a read-only configuration mount. Normal
`docker compose down` preserves named volumes. Volume deletion is an explicit,
separately documented reset action and is not part of routine shutdown.

### Environment Variable Strategy

Retain all Phase 0 variables and add safe local-only values grouped by service.
Use variables for image tags, loopback host ports, local credentials where
required, and initialization resource names. Values in `.env.example` are
development examples only and must be labeled unsuitable for production.

Required additions:

```text
NGINX_IMAGE
NGINX_HTTP_PORT
NGINX_HTTPS_PORT
REDIS_IMAGE
REDIS_PORT
REDIS_PASSWORD
CASSANDRA_IMAGE
CASSANDRA_CQL_PORT
CASSANDRA_INTERNODE_PORT
CASSANDRA_KEYSPACE
REDPANDA_IMAGE
REDPANDA_KAFKA_PORT
REDPANDA_ADMIN_PORT
REDPANDA_CLICK_EVENTS_TOPIC
MINIO_SOURCE_TAG
MINIO_MC_IMAGE
MINIO_API_PORT
MINIO_CONSOLE_PORT
MINIO_ROOT_USER
MINIO_ROOT_PASSWORD
MINIO_RAW_EVENTS_BUCKET
CLICKHOUSE_IMAGE
CLICKHOUSE_HTTP_PORT
CLICKHOUSE_NATIVE_PORT
CLICKHOUSE_USER
CLICKHOUSE_PASSWORD
CLICKHOUSE_DATABASE
KEYCLOAK_IMAGE
KEYCLOAK_HTTP_PORT
KEYCLOAK_MANAGEMENT_PORT
KEYCLOAK_BOOTSTRAP_ADMIN_USERNAME
KEYCLOAK_BOOTSTRAP_ADMIN_PASSWORD
```

`NGINX_HTTPS_PORT` is reserved and documented but not mapped until local TLS is
configured in a later phase. `CASSANDRA_INTERNODE_PORT` is configurable for
diagnostics but is not required by the single-node local topology.

### Health-Check Strategy

Use Compose health checks where a practical in-container command exists.
Document a host-side readiness command for every service, including Keycloak,
whose official image intentionally excludes HTTP clients.

| Service | Compose health check | Host-side readiness verification |
|---------|----------------------|----------------------------------|
| Nginx | `wget -qO- http://localhost/healthz` | `Invoke-WebRequest http://localhost:${NGINX_HTTP_PORT}/healthz` |
| Redis | `redis-cli -a "$REDIS_PASSWORD" ping` | `docker compose exec -T redis redis-cli -a "$REDIS_PASSWORD" ping` |
| Cassandra | `cqlsh -e "DESCRIBE CLUSTER"` | `docker compose exec -T cassandra cqlsh -e "DESCRIBE CLUSTER"` |
| Redpanda | `rpk cluster health` with healthy-state assertion | `docker compose exec -T redpanda rpk cluster health` |
| MinIO | `wget -qO- http://localhost:9000/minio/health/ready` from the Alpine runtime | `Invoke-WebRequest http://localhost:${MINIO_API_PORT}/minio/health/ready` |
| ClickHouse | `clickhouse-client --query "SELECT 1"` | `docker compose exec -T clickhouse clickhouse-client --user "$CLICKHOUSE_USER" --password "$CLICKHOUSE_PASSWORD" --query "SELECT 1"` |
| Keycloak | Bash TCP request to management `/health/ready` | `Invoke-WebRequest http://localhost:${KEYCLOAK_MANAGEMENT_PORT}/health/ready` |

Health checks must have realistic start periods, especially for Cassandra and
Keycloak. A container process starting is not treated as readiness.

### Initialization Strategy

Add four LF-encoded shell scripts under `scripts/init/`. Mount each script
read-only into its matching opt-in Compose helper. Helpers depend on the
matching long-running service health check and create resources with
`IF NOT EXISTS`, an equivalent client flag, or an existence check before
creation. Scripts must fail clearly when readiness or credentials are invalid.

Default resources:

```text
Cassandra keyspace: ${CASSANDRA_KEYSPACE}
Redpanda topic: ${REDPANDA_CLICK_EVENTS_TOPIC} = click-events
MinIO bucket: ${MINIO_RAW_EVENTS_BUCKET} = raw-events
ClickHouse database: ${CLICKHOUSE_DATABASE}
```

Do not create Cassandra tables, ClickHouse tables, MinIO objects, Redpanda
messages, Keycloak realms, users, clients, or application-owned records.

### Local Developer Workflow

1. Validate configuration with `.env.example`.
2. Build the pinned MinIO source image and start all seven services with
   `docker compose --env-file .env.example up -d --build`.
3. Inspect `docker compose --env-file .env.example ps`.
4. Wait for health checks and run the documented host-side readiness commands.
5. Run each `init` profile helper twice.
6. Inspect the four expected resources.
7. Stop services with `docker compose --env-file .env.example down`.

## Service Plan

| Service | Image/version selection | Ports and storage | Configuration and readiness | Risks and mitigations |
|---------|-------------------------|-------------------|-----------------------------|-----------------------|
| Nginx | `nginx:1.30.2-alpine` | Bind HTTP `127.0.0.1:${NGINX_HTTP_PORT}:80`; reserve HTTPS variable only; no volume | Add `infra/nginx/nginx.conf`; active `/healthz` returns success; comments describe future redirect and authenticated API boundaries without proxying | Placeholder routes could imply active behavior. Keep only `/healthz` active and return explicit non-success responses elsewhere. |
| Redis | `redis:8.6.3-alpine` | Bind `127.0.0.1:${REDIS_PORT}:6379`; mount `redis-data:/data` | Add `infra/redis/redis.conf`; enable simple local persistence and require the safe local password; verify with authenticated `PING` | Default Redis images can be open when ports are published. Bind loopback only and require a local password. |
| Cassandra | `cassandra:5.0.8-bookworm` | Bind CQL `127.0.0.1:${CASSANDRA_CQL_PORT}:9042`; optionally bind diagnostic internode port; mount `cassandra-data:/var/lib/cassandra` | Use official single-node defaults and documented local cluster name; verify with `cqlsh`; create keyspace only with `SimpleStrategy` and replication factor `1` | Initial startup is slow. Add a generous start period, health retries, and explicit log guidance. |
| Redpanda | `redpandadata/redpanda:v26.1.4` | Bind Kafka `127.0.0.1:${REDPANDA_KAFKA_PORT}:9092` and admin `127.0.0.1:${REDPANDA_ADMIN_PORT}:9644`; mount `redpanda-data:/var/lib/redpanda/data` | Configure one-node developer mode inline in Compose with internal and external listeners; verify with `rpk cluster health`; create only `click-events` | Listener configuration is easy to misconfigure. Document internal versus external addresses and assert cluster health before topic creation. |
| MinIO | Local multi-stage image built from official source tag `RELEASE.2025-10-15T17-29-55Z`; use `minio/mc:RELEASE.2025-08-13T08-35-41Z` only for the init helper | Bind API `127.0.0.1:${MINIO_API_PORT}:9000` and console `127.0.0.1:${MINIO_CONSOLE_PORT}:9001`; mount `minio-data:/data` | Add `infra/minio/Dockerfile`; verify `/minio/health/ready`; create only `raw-events` with `mc mb --ignore-existing` | Community Edition is source-only and archived. Build the last official security release locally, document slower first startup, keep exposure loopback-only, and record the maintenance risk in `docs/tradeoffs.md`. |
| ClickHouse | `clickhouse:26.5.1.882-jammy` | Bind HTTP `127.0.0.1:${CLICKHOUSE_HTTP_PORT}:8123` and native `127.0.0.1:${CLICKHOUSE_NATIVE_PORT}:9000`; mount `clickhouse-data:/var/lib/clickhouse` | Use safe local username/password environment variables; verify with `SELECT 1`; create database only | Host port `9000` collides with MinIO defaults. Default `CLICKHOUSE_NATIVE_PORT` to `9002` and document internal port `9000`. |
| Keycloak | `quay.io/keycloak/keycloak:26.6.2` | Bind HTTP `127.0.0.1:${KEYCLOAK_HTTP_PORT}:8080` and management `127.0.0.1:${KEYCLOAK_MANAGEMENT_PORT}:9000`; mount `keycloak-data:/opt/keycloak/data` | Run `start-dev`, set `KC_HEALTH_ENABLED=true`, use safe bootstrap admin examples, verify management `/health/ready`; do not import a realm | Startup may be slow and development mode is intentionally insecure. Bind loopback only, label credentials local-only, add start period, and defer realm import and login flows to Phase 7. |

## Initialization Plan

### Cassandra Keyspace Initialization

Add `scripts/init/cassandra-keyspace.sh`. Execute it through `cassandra-init`
using the Cassandra image and the shared network. Wait for `cqlsh` readiness,
then run:

```sql
CREATE KEYSPACE IF NOT EXISTS <keyspace>
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};
```

This is a local one-node strategy only. It creates no tables. Future Phase 2
Cassandra work must document query patterns, partition keys, clustering,
bucketing where needed, TTL behavior, and why high-volume secondary indexes are
not used.

### Redpanda Topic Initialization

Add `scripts/init/redpanda-topics.sh`. Execute it through `redpanda-init` using
the Redpanda image and shared network. Confirm cluster health, test whether
`${REDPANDA_CLICK_EVENTS_TOPIC}` exists, and create it only when absent. Use one
partition and replication factor `1` for the one-node local environment.

This creates no producer, consumer, schema, or message. Future analytics work
may revisit topic sizing and retention.

### MinIO Bucket Initialization

Add `scripts/init/minio-buckets.sh`. Execute it through `minio-init` using the
pinned MinIO Client image and shared network. Configure a temporary client alias
from local credentials, then run:

```text
mc mb --ignore-existing local/${MINIO_RAW_EVENTS_BUCKET}
```

This creates no objects and writes no Parquet files. Future Event Writer work
owns raw object layout and retention.

### ClickHouse Database Initialization

Add `scripts/init/clickhouse-database.sh`. Execute it through `clickhouse-init`
using the ClickHouse image and shared network. Wait for `SELECT 1`, then run:

```sql
CREATE DATABASE IF NOT EXISTS <database>;
```

This creates no table, view, query, aggregate, or application write path.
Future batch and dashboard phases own ClickHouse schema design.

## Documentation Plan

### `README.md`

- Change project status from Phase 0 foundation to Phase 1 local
  infrastructure.
- Add the Compose startup, readiness, initialization, and shutdown entry points.
- Keep runtime boundaries and deferred behavior explicit.
- Link `docs/local-infrastructure.md` and the Phase 1 quickstart.

### `docs/local-infrastructure.md`

- Add prerequisites, exact local images, ports, credentials, network, volumes,
  and local-only warnings.
- Explain startup, MinIO source build behavior, service-specific readiness,
  initialization, re-execution, resource inspection, shutdown, and optional
  volume reset.
- Separate active Phase 1 infrastructure from placeholder boundaries and
  deferred functionality.
- Explain common diagnostics for port conflicts and delayed readiness.

### `docs/architecture.md`

- Add a Phase 1 section showing the seven active infrastructure services.
- Preserve Redirect Service independence and existing separate runtime shells.
- Clarify that Nginx has no business proxy routes yet.
- Clarify that analytics infrastructure resources exist without an active
  pipeline.

### `docs/tradeoffs.md`

- Record single-node Compose simplicity versus production topology.
- Record explicit initialization versus automatic hidden bootstrap.
- Record loopback-only safe local defaults versus production hardening.
- Record Keycloak realm-import deferral.
- Record the MinIO source-only distribution and archive risk, including why the
  required local MinIO dependency is retained for this approved phase.

## Validation Plan

### Compose Validation

Run and require success:

```powershell
docker compose --env-file .env.example config
docker compose --env-file .env.example up -d --build
docker compose --env-file .env.example ps
docker compose --env-file .env.example down
```

`docker compose ps` output must show the seven long-running services and healthy
status after startup windows complete.

### Infrastructure Validation

- Run the documented readiness command for each service.
- Run every `init` profile helper once.
- Inspect the Cassandra keyspace, Redpanda topic, MinIO bucket, and ClickHouse
  database.
- Run every `init` profile helper a second time.
- Reinspect the same four resources and confirm that no table, object, message,
  or application-owned data was created.
- Inspect Nginx configuration and verify that it does not proxy to unavailable
  application endpoints.

### Security Validation

- Review `.env.example` and rendered Compose output for real secrets, external
  endpoints, and production credentials.
- Confirm every published port binds to `127.0.0.1`.
- Confirm credentials are labeled local-only and unsuitable for production.
- Confirm Keycloak uses development mode only for local execution.
- Confirm the MinIO source tag is exact and not a floating `latest` reference.

## Risk Analysis

| Risk | Detection strategy | Mitigation strategy |
|------|--------------------|---------------------|
| Service startup ordering | Inspect `docker compose ps`, helper failures, and service logs | Keep initialization explicit, use helper `depends_on` health conditions, and make scripts rerunnable |
| Cassandra readiness delays | `cqlsh` readiness fails while logs show bootstrap progress | Use a generous health-check start period and document retry/log commands |
| Redpanda readiness complexity | `rpk cluster health` reports missing controller, leader, or responding node | Use one-node developer mode, stable internal listeners, and require healthy cluster state before topic creation |
| MinIO initialization timing | `/minio/health/ready` or `mc alias set` fails | Wait for readiness, keep `mc mb --ignore-existing`, and allow isolated reruns |
| MinIO source-only upstream | Build or security review reveals unavailable or stale source dependency | Pin the last official security source release, document the archived-upstream risk, and revisit the architecture before any production-oriented phase |
| Keycloak startup duration | Management `/health/ready` returns non-success while logs show bootstrapping | Add a realistic start period and document that readiness may lag container start |
| Local port conflicts | Compose startup reports bind failure | Put every host port in `.env.example`, choose non-conflicting defaults, bind loopback only, and document override examples |
| Credential misuse | Review finds local example credentials copied into non-local configuration | Label examples local-only, avoid external endpoints, and state that Phase 1 is not production configuration |

## Deliverables

### Files To Create

```text
docs/local-infrastructure.md
infra/nginx/nginx.conf
infra/redis/redis.conf
infra/cassandra/README.md
infra/redpanda/README.md
infra/minio/Dockerfile
infra/minio/README.md
infra/clickhouse/README.md
infra/keycloak/README.md
scripts/init/cassandra-keyspace.sh
scripts/init/redpanda-topics.sh
scripts/init/minio-buckets.sh
scripts/init/clickhouse-database.sh
```

### Files To Modify

```text
.env.example
docker-compose.yml
README.md
docs/architecture.md
docs/tradeoffs.md
CHANGELOG.md
```

### Compose Changes

- Add seven default long-running infrastructure services.
- Add six named volumes for stateful data.
- Retain and reuse the existing `app` network.
- Add four `init` profile helpers.
- Mount Nginx and Redis configuration read-only.
- Build the MinIO server image locally from the pinned official source tag.
- Bind all published ports to `127.0.0.1`.

### Environment Variable Additions

Add the service image, port, local credential, and initialization-resource
variables listed in the Environment Variable Strategy section. Retain all Phase
0 application variables unchanged.

## Project Structure

### Documentation (this feature)

```text
specs/002-local-infrastructure/
  plan.md
  research.md
  data-model.md
  quickstart.md
  contracts/
    phase1-local-infrastructure-contracts.md
  checklists/
    requirements.md
```

### Source Code (repository root)

```text
.env.example
docker-compose.yml
README.md
CHANGELOG.md
docs/
  architecture.md
  local-infrastructure.md
  tradeoffs.md
infra/
  nginx/
    nginx.conf
  redis/
    redis.conf
  cassandra/
    README.md
  redpanda/
    README.md
  minio/
    Dockerfile
    README.md
  clickhouse/
    README.md
  keycloak/
    README.md
scripts/
  init/
    cassandra-keyspace.sh
    redpanda-topics.sh
    minio-buckets.sh
    clickhouse-database.sh
```

**Structure Decision**: Phase 1 changes infrastructure, scripts, configuration,
and documentation only. Existing files under `src/` remain untouched. Service
folders under `infra/` make infrastructure ownership explicit while keeping
the Compose file as the single local orchestration entry point.

## Complexity Tracking

No constitution violations are introduced by this plan.

## Phase 0 Research Output

See [research.md](./research.md).

## Phase 1 Design Output

See [data-model.md](./data-model.md),
[quickstart.md](./quickstart.md), and
[phase1-local-infrastructure-contracts.md](./contracts/phase1-local-infrastructure-contracts.md).

## Changelog Impact

Update the current `Unreleased` section in `CHANGELOG.md` during implementation
to record the Phase 1 local infrastructure environment, initialization scripts,
documentation, and explicit absence of application business behavior.

## Post-Design Constitution Check

- **Redirect Service Independence**: PASS. Nginx placeholders do not proxy, and
  no redirect behavior or dependency is introduced.
- **Query-Driven Cassandra Principles**: PASS. The design creates a keyspace
  only and defers tables to a query-driven Phase 2 design.
- **Non-Blocking Analytics Architecture**: PASS. The design creates an empty
  topic, bucket, and database only. Analytics execution remains absent.
- **Separate Runtime Execution Models**: PASS. Application shells remain
  unchanged and separate. Infrastructure helpers are isolated short-lived
  processes.
- **Reproducible Local Development**: PASS. Exact tags, local source build,
  Compose network, named volumes, safe variables, readiness checks, and
  idempotent scripts are documented.
- **English-Only Documentation**: PASS. All generated artifacts and planned
  documentation changes are English-only.
- **Observability Phase Boundary**: PASS. Only readiness checks are added.
  Phase 10 observability services remain absent.
