# Quickstart: Phase 1 Minimal Local Infrastructure

This quickstart defines the implementation validation flow for Phase 1. It is
not a runbook for a completed implementation until the generated tasks have
been executed.

## Prerequisites

- Docker Engine or Docker Desktop
- Docker Compose v2
- PowerShell for the host-side examples
- Internet access on first startup to pull images and build the pinned MinIO
  Community Edition source image

## Validate Configuration

```powershell
docker compose --env-file .env.example config
```

Expected result: Compose renders without missing variables, committed real
secrets, real external endpoints, or observability services.

## Start Infrastructure

```powershell
docker compose --env-file .env.example up -d --build
docker compose --env-file .env.example ps
```

Expected long-running services:

```text
nginx
redis
cassandra
redpanda
minio
clickhouse
keycloak
```

The first MinIO build is expected to take longer because Phase 1 builds the
server from the pinned official source release.

## Verify Readiness

Use the values from `.env.example` in the host-side URLs.

```powershell
Invoke-WebRequest http://localhost:<nginx-http-port>/healthz
docker compose --env-file .env.example exec -T redis redis-cli -a <redis-password> ping
docker compose --env-file .env.example exec -T cassandra cqlsh -e "DESCRIBE CLUSTER"
docker compose --env-file .env.example exec -T redpanda rpk cluster health
Invoke-WebRequest http://localhost:<minio-api-port>/minio/health/ready
docker compose --env-file .env.example exec -T clickhouse clickhouse-client --user <clickhouse-user> --password <clickhouse-password> --query "SELECT 1"
Invoke-WebRequest http://localhost:<keycloak-management-port>/health/ready
```

Expected result: each command succeeds only after its service is usable.

## Initialize Resources

Run each helper through the `init` profile:

```powershell
docker compose --env-file .env.example --profile init run --rm cassandra-init
docker compose --env-file .env.example --profile init run --rm redpanda-init
docker compose --env-file .env.example --profile init run --rm minio-init
docker compose --env-file .env.example --profile init run --rm clickhouse-init
```

Run the same four commands a second time. Both passes must succeed.

## Inspect Expected Resources

```powershell
docker compose --env-file .env.example exec -T cassandra cqlsh -e "DESCRIBE KEYSPACES"
docker compose --env-file .env.example exec -T redpanda rpk topic list
docker compose --env-file .env.example --profile init run --rm --entrypoint /bin/sh minio-init -c 'mc alias set local http://minio:9000 $MINIO_ROOT_USER $MINIO_ROOT_PASSWORD > /dev/null && mc ls local'
docker compose --env-file .env.example exec -T clickhouse clickhouse-client --user <clickhouse-user> --password <clickhouse-password> --query "SHOW DATABASES"
```

Expected result:

- Cassandra contains the configured local keyspace and no Phase 1 table.
- Redpanda contains `click-events` and no application message is required.
- MinIO contains `raw-events` and no object.
- ClickHouse contains the configured database and no Phase 1 table.

## Verify Nginx Boundaries

Inspect `infra/nginx/nginx.conf`.

Expected result:

- `/healthz` is active.
- Future redirect and authenticated API boundaries are comments or explicit
  placeholders only.
- No route proxies to Redirect Service, ApiGateway, or unavailable endpoints.

## Verify Security Boundaries

Inspect `.env.example` and rendered Compose output.

Expected result:

- Published host ports bind to `127.0.0.1`.
- Credentials are clearly local-only examples.
- No production credential or real external endpoint exists.
- Keycloak uses local development mode only.
- MinIO builds from exact source tag `RELEASE.2025-10-15T17-29-55Z`.

## Stop Infrastructure

```powershell
docker compose --env-file .env.example down
```

Expected result: containers and the Compose network stop without deleting named
volumes.

## Deferred Functionality

Phase 1 must not implement:

- URL creation or redirect resolution
- Application Redis cache logic or Cassandra repositories
- Redpanda producers or consumers
- MinIO Parquet writing or DuckDB processing
- ClickHouse tables or dashboard queries
- User API, ApiGateway, Ocelot, Angular, authentication flows, or JWT validation
- Full observability stack
- Application integration, end-to-end, or load tests
