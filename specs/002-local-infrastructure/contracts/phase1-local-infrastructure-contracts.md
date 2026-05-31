# Phase 1 Local Infrastructure Contracts

Phase 1 contracts define infrastructure readiness and initialization only. They
do not define application APIs or business behavior.

## Compose Service Contract

Default startup must create exactly these long-running services:

```text
nginx
redis
cassandra
redpanda
minio
clickhouse
keycloak
```

Rules:

- All services join the Compose network key `app`.
- Published host ports bind to `127.0.0.1`.
- Redis, Cassandra, Redpanda, MinIO, ClickHouse, and Keycloak use named volumes.
- Nginx mounts tracked configuration read-only.
- Normal startup does not create application-owned resources.
- Full observability services remain absent.

## Readiness Contract

Every long-running service must expose a practical readiness verification:

| Service | Readiness assertion |
|---------|---------------------|
| Nginx | `GET /healthz` succeeds |
| Redis | Authenticated `PING` returns `PONG` |
| Cassandra | `cqlsh -e "DESCRIBE CLUSTER"` succeeds |
| Redpanda | `rpk cluster health` reports a healthy one-node cluster |
| MinIO | `GET /minio/health/ready` succeeds |
| ClickHouse | `SELECT 1` succeeds |
| Keycloak | Management `GET /health/ready` succeeds |

A process that is still bootstrapping must not be reported as ready.

## Initialization Helper Contract

Initialization is explicit through four Compose services under profile `init`:

```text
cassandra-init
redpanda-init
minio-init
clickhouse-init
```

Rules:

- Every helper exits successfully when its required resource already exists.
- Every helper fails clearly when its dependency is unavailable.
- Helpers may create only the resources listed below.
- Helpers must not delete or recreate existing resources destructively.

## Resource Contract

| Service | Allowed default resource | Prohibited Phase 1 resource or behavior |
|---------|--------------------------|------------------------------------------|
| Cassandra | Configured local keyspace | Tables, indexes, repository reads or writes |
| Redpanda | `click-events` topic | Messages, producers, consumers, schemas |
| MinIO | `raw-events` bucket | Objects, Parquet files, retention processing |
| ClickHouse | Configured analytics database | Tables, views, inserts, dashboard queries |
| Keycloak | None | Realm import, clients, users, flows, JWT validation |

## Nginx Placeholder Contract

Nginx may actively expose only its own Phase 1 health route:

```http
GET /healthz
```

Future boundaries may be documented but must not proxy:

```text
Client -> Nginx -> Redirect Service
Authenticated client -> Nginx -> ApiGateway
```

Redirect Service must remain outside ApiGateway and independent from
authentication, Shortener API, dashboard queries, and analytics processing.

## Safe Local Configuration Contract

`.env.example` must:

- Retain Phase 0 variables.
- Add configurable service versions, ports, local credentials, and resource
  names.
- Use safe local-only example values.
- State that credentials are unsuitable for production.
- Avoid real secrets and external endpoints.

The Compose file must not use floating `latest` tags for long-running services.

