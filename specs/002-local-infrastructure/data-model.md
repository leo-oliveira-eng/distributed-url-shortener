# Data Model: Phase 1 Minimal Local Infrastructure

Phase 1 introduces infrastructure resources only. It does not introduce
application business data, Cassandra tables, ClickHouse tables, messages,
objects, users, realms, clients, or authentication flows.

## Local Infrastructure Environment

**Purpose**: Represents the reproducible Compose environment required by later
phases.

**Fields**:

- `network`: `${COMPOSE_PROJECT_NAME}-app`
- `services`: `nginx`, `redis`, `cassandra`, `redpanda`, `minio`,
  `clickhouse`, `keycloak`
- `publishedPorts`: Loopback-only configurable host ports
- `volumes`: Named local volumes for stateful services
- `configurationSource`: `.env.example` plus tracked infrastructure files

**Validation Rules**:

- Exactly the seven required long-running services start by default.
- All services join the shared `app` network.
- All published ports bind to `127.0.0.1`.
- No production credentials, secrets, or external endpoints are committed.
- Existing application shells remain unchanged.

## Infrastructure Service

**Purpose**: Defines one long-running local dependency.

**Fields**:

- `name`: Stable Compose service name
- `image`: Exact tag, or pinned source build for MinIO
- `internalPorts`: Container ports used on the shared network
- `publishedPorts`: Optional loopback host bindings
- `volume`: Optional named volume
- `configurationFiles`: Optional tracked read-only configuration
- `healthCheck`: Compose health command
- `readinessCommand`: Documented host-side verification command

**Validation Rules**:

- A running process is not sufficient; every service has a readiness method.
- Stateful service data persists across normal `docker compose down`.
- Service-specific configuration remains local-development focused.

## Cassandra Keyspace Foundation

**Purpose**: Confirms Cassandra readiness for later query-driven schema work.

**Fields**:

- `name`: `${CASSANDRA_KEYSPACE}`
- `replicationClass`: `SimpleStrategy`
- `replicationFactor`: `1`
- `tables`: Empty

**Validation Rules**:

- Creation uses `CREATE KEYSPACE IF NOT EXISTS`.
- Re-execution preserves the keyspace.
- No table, secondary index, repository code, read, or write is added.
- Future tables must be justified by concrete query patterns.

## Redpanda Topic Foundation

**Purpose**: Confirms Kafka-compatible topic administration for the future
non-blocking analytics pipeline.

**Fields**:

- `name`: `${REDPANDA_CLICK_EVENTS_TOPIC}`
- `defaultValue`: `click-events`
- `partitions`: `1`
- `replicationFactor`: `1`
- `messages`: Empty

**Validation Rules**:

- Initialization creates the topic only when absent.
- Re-execution succeeds without duplicate resources.
- No producer, consumer, schema, or message is added.

## MinIO Bucket Foundation

**Purpose**: Confirms S3-compatible bucket administration for future raw
analytics files.

**Fields**:

- `name`: `${MINIO_RAW_EVENTS_BUCKET}`
- `defaultValue`: `raw-events`
- `objects`: Empty

**Validation Rules**:

- Initialization uses an ignore-existing operation.
- Re-execution succeeds without destructive recreation.
- No Parquet file or other application-owned object is written.

## ClickHouse Database Foundation

**Purpose**: Confirms ClickHouse readiness for future aggregate schema work.

**Fields**:

- `name`: `${CLICKHOUSE_DATABASE}`
- `tables`: Empty
- `views`: Empty

**Validation Rules**:

- Creation uses `CREATE DATABASE IF NOT EXISTS`.
- Re-execution preserves the database.
- No aggregate table, dashboard query, or application write is added.

## Nginx Boundary Placeholder

**Purpose**: Exposes local ingress health while documenting later route
ownership.

**Fields**:

- `activeEndpoint`: `GET /healthz`
- `futurePublicBoundary`: `GET /{shortCode}` to Redirect Service
- `futureAuthenticatedBoundary`: Authenticated API routes to ApiGateway
- `activeProxyRoutes`: Empty

**Validation Rules**:

- `/healthz` returns a successful response.
- No unavailable application endpoint is proxied.
- Redirect Service is not routed through ApiGateway.

## Keycloak Local Infrastructure Instance

**Purpose**: Confirms identity infrastructure startup only.

**Fields**:

- `mode`: `start-dev`
- `healthEndpoint`: Management interface `/health/ready`
- `bootstrapAdmin`: Safe local-only example credentials
- `realmImports`: Empty

**Validation Rules**:

- Readiness succeeds after startup completes.
- No realm, client, application login flow, or JWT validation is implemented.
- Configuration is explicitly unsuitable for production.

